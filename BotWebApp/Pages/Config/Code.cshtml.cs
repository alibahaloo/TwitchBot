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
        public List<string> Errors { get; set; } = new List<string> { };
        public List<string> ResrouceErrors { get; set; } = new List<string> { };
        public CodeModel(TwitchAuth twitchAuth)
        {
            _twitchAuth = twitchAuth;
        }

        public async Task<PageResult> OnGet(string code)
        {
            if (TwitchInfo.client_id == string.Empty) { ResrouceErrors.Add("Error: Missing client_id"); }
            if (TwitchInfo.client_secret == string.Empty) { ResrouceErrors.Add("Error: Missing client_secret"); }
            if (TwitchInfo.redirect_uri == string.Empty) { ResrouceErrors.Add("Error: Missing redirect_uri"); }

            if (ResrouceErrors.Any()) return Page();

            string referrer = Request.Headers.Referer.ToString();

            if (referrer == "https://id.twitch.tv/")
            {
                var createResult = await _twitchAuth.CreateAccessToken(code);

                if (createResult.Errors.Any())
                {
                    Errors = createResult.Errors;
                } else
                {
                    CodeCaptured = true;
                }
            }

            var accessToken = await _twitchAuth.GetAccessToken();

            if (accessToken.Errors.Any())
            {
                Errors = accessToken.Errors;
                IsTokenValid = false;
            } else
            {
                IsTokenValid = true;
            }

            return Page();
        }
    }
}
