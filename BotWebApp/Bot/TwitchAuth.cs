using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TwitchBot.Data;

namespace TwitchBot.Bot
{
    public class Response
    {
        public string Result { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
    }
    public class TwitchAuth
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TwitchAuth> _logger;

        public TwitchAuth(IServiceProvider serviceProvider, ILogger<TwitchAuth> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task<Response> GetAccessToken()
        {
            Response response = new();

            var _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var record = await _context.TwitchCodes.FirstOrDefaultAsync();

            if (record == null)
            {
                _logger.LogCritical(BotConfigurations.Log("GetAccessToken", "No Records Found!"));
                response.Errors.Add("Error: No Records Found!");
                return response;
            }

            //Validate the accessToken
            if (await ValidateAccessToken(record.AccessToken)) //return record.AccessToken;
            {
                response.Result = record.AccessToken;
            } else
            {
                //we get here if accessToken is not valid, then get a new one using refresh
                response = await RefreshAccessToken(record.RefreshToken);
            }

            return response;
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
        public async Task<Response> CreateAccessToken(string code)
        {
            Response response = new();

            //Check for resrouces
            if (TwitchInfo.client_id == string.Empty) response.Errors.Add("Error: Missing client_id");
            if (TwitchInfo.client_secret == string.Empty) response.Errors.Add("Error: Missing client_secret");
            if (TwitchInfo.redirect_uri == string.Empty) response.Errors.Add("Error: Missing redirect_uri");
            
            if (response.Errors.Any()) { return response; }

            HttpClient httpClient = new();

            var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", TwitchInfo.client_id),
                    new KeyValuePair<string, string>("client_secret", TwitchInfo.client_secret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", TwitchInfo.redirect_uri),
                    new KeyValuePair<string, string>("code", code),
                });

            var httpResponse = await httpClient.PostAsync(BotConfigurations.TwitchTokenEndpoint, requestContent);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<TwitchTokenDTO>(responseContent.ToString());

            if (body == null)
            {
                _logger.LogError(BotConfigurations.Log("CreateAccessToken", "responseContent is null"));
                response.Errors.Add("responseContent is null");
                return response;
            }
            try
            {
                httpResponse.EnsureSuccessStatusCode();

                //Record access token and refresh token in DB
                await SaveToken(body.access_token, body.refresh_token, code);
                response.Result = body.access_token;

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(BotConfigurations.Log("CreateAccessToken", $"{e.Message} : {body.message}"));
                response.Errors.Add(BotConfigurations.Log("CreateAccessToken", $"{e.Message} : {body.message}"));
                return response;
            }
            finally { httpClient.Dispose(); }
        }
        private async Task<Response> RefreshAccessToken(string refreshToken)
        {
            Response response = new();

            //Check for resrouces
            if (TwitchInfo.client_id == string.Empty) response.Errors.Add("Error: Missing client_id") ;
            if (TwitchInfo.client_secret == string.Empty) response.Errors.Add("Error: Missing client_secret");

            if (response.Errors.Any()) { return response; }

            HttpClient httpClient = new();

            var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", TwitchInfo.client_id),
                    new KeyValuePair<string, string>("client_secret", TwitchInfo.client_secret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                });

            var httpResponse = await httpClient.PostAsync(BotConfigurations.TwitchTokenEndpoint, requestContent);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<TwitchTokenDTO>(responseContent.ToString());

            if (body == null)
            {
                _logger.LogError(BotConfigurations.Log("RefreshAccessToken", "responseContent is null"));
                response.Errors.Add("responseContent is null");
                return response;
            }

            try
            {
                httpResponse.EnsureSuccessStatusCode();

                //Record access token and refresh token in DB
                await SaveToken(body.access_token, body.refresh_token);
                response.Result = body.access_token;

                return response;
            }
            catch (Exception e)
            {
                //Invalid RefreshToken
                _logger.LogCritical(BotConfigurations.Log("RefreshAccessToken", $"{e.Message} : {body.message}"));
                response.Errors.Add(BotConfigurations.Log("RefreshAccessToken", $"{e.Message} : {body.message}"));
                return response;
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
