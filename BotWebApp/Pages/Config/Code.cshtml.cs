using Microsoft.AspNetCore.Mvc.RazorPages;
using TwitchBot.Bot;

namespace TwitchBot.Pages.Config
{
    public class CodeModel : PageModel
    {
        private readonly TwitchAuth _twitchAuth;
        public bool IsTokenValid { get; set; } = false;
        public bool CodeCaptured { get; set; } = false;
        public string CodeLink = $"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={TwitchInfo.client_id}&redirect_uri={TwitchInfo.redirect_uri}&scope={TwitchInfo.scope}&state=c3ab8aa609ea11e793ae92361f002671";
        public string Error { get; set; } = string.Empty;
        public CodeModel(TwitchAuth twitchAuth)
        {
            _twitchAuth = twitchAuth;
        }

        public async Task OnGet(string code)
        {
            string referrer = Request.Headers.Referer.ToString();

            if (referrer == "https://id.twitch.tv/")
            {
                var result = await _twitchAuth.CreateAccessToken(code);

                if (result.Contains("Error"))
                {
                    Error = result;
                } else
                {
                    CodeCaptured = true;
                }
            }
            //Validate the tokens
            IsTokenValid = (await _twitchAuth.GetAccessToken() != string.Empty);
        }
    }
}
