using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TwitchBot.Data;

namespace TwitchBot.Bot
{
    public class TwitchAuth
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TwitchAuth> _logger;

        public TwitchAuth(IServiceProvider serviceProvider, ILogger<TwitchAuth> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task<string> GetAccessToken()
        {
            var _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var record = await _context.TwitchCodes.FirstOrDefaultAsync();

            if (record == null)
            {
                _logger.LogCritical(BotConfigurations.Log("GetAccessToken", "No Records Found!"));
                return string.Empty;
            }

            //Validate the accessToken
            if (await ValidateAccessToken(record.AccessToken)) return record.AccessToken;

            //we get here if accessToken is not valid, then get a new one using refresh
            return await RefreshAccessToken(record.RefreshToken);
        }
        private async Task SaveToken(string accessToken, string refreshToken, string code = "")
        {
            var _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var record = await _context.TwitchCodes.FirstOrDefaultAsync();

            if (record == null)
            {
                _context.Add(new TwitchCode
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Code = code
                });
            }
            else
            {
                record.AccessToken = accessToken;
                record.RefreshToken = refreshToken;
                if (code != "") record.Code = code;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<string> CreateAccessToken(string code)
        {
            //Check for resrouces
            if (TwitchInfo.client_id == string.Empty) { return "Error: Missing client_id"; }
            if (TwitchInfo.client_secret == string.Empty) { return "Error: Missing client_secret"; }
            if (TwitchInfo.redirect_uri == string.Empty) { return "Error: Missing redirect_uri"; }

            HttpClient httpClient = new();

            var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", TwitchInfo.client_id),
                    new KeyValuePair<string, string>("client_secret", TwitchInfo.client_secret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", TwitchInfo.redirect_uri),
                    new KeyValuePair<string, string>("code", code),
                });

            var response = await httpClient.PostAsync(BotConfigurations.TwitchTokenEndpoint, requestContent);

            try
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var body = JsonConvert.DeserializeObject<TokenDTO>(responseContent.ToString());

                if (body != null)
                {
                    //Record access token and refresh token in DB
                    await SaveToken(body.access_token, body.refresh_token, code);
                    return body.access_token;
                }
                else
                {
                    _logger.LogError(BotConfigurations.Log("CreateAccessToken", "responseContent is null"));
                    return string.Empty;
                }

            }
            catch (Exception e)
            {
                _logger.LogError(BotConfigurations.Log("CreateAccessToken", e.Message));
                return string.Empty;
            }
            finally { httpClient.Dispose(); }
        }
        private async Task<string> RefreshAccessToken(string refreshToken)
        {
            HttpClient httpClient = new();

            var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", TwitchInfo.client_id),
                    new KeyValuePair<string, string>("client_secret", TwitchInfo.client_secret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                });

            var response = await httpClient.PostAsync(BotConfigurations.TwitchTokenEndpoint, requestContent);

            try
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var body = JsonConvert.DeserializeObject<TokenDTO>(responseContent.ToString());

                if (body != null)
                {
                    //Record access token and refresh token in DB
                    await SaveToken(body.access_token, body.refresh_token);
                    return body.access_token;
                }
                else
                {
                    _logger.LogError(BotConfigurations.Log("RefreshAccessToken", "responseContent is null"));
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                //Invalid RefreshToken
                _logger.LogCritical(BotConfigurations.Log("RefreshAccessToken", $"Need new AccessToken + RefreshToken : {e.Message}"));
                return string.Empty;
            }
            finally { httpClient.Dispose(); }
        }
        private async Task<bool> ValidateAccessToken(string accessToken)
        {
            HttpClient httpClient = new();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(BotConfigurations.TwitchTokenValidateEndpoint),
                Headers =
                {
                    { "Accept", "" },
                    { "Authorization", $"Bearer {accessToken}" },
                }
            };

            var response = await httpClient.SendAsync(request);

            try
            {
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(BotConfigurations.Log("ValidateAccessToken", e.Message));
                return false;
            }
            finally { httpClient.Dispose(); }
        }
    }
}
