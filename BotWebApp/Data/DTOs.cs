namespace TwitchBot.Data
{
    public class TwitchChatterDTO
    {
        public string user_id { get; set; } = string.Empty;
        public string user_login { get; set; } = string.Empty;
        public string user_name { get; set; } = string.Empty;
    }
    public class TwitchChatDTO
    {
        public int total { get; set; } = 0;
        public List<TwitchChatterDTO> Data { get; set; } = new List<TwitchChatterDTO>();
    }
    public class TwitchTokenDTO
    {
        public string access_token { get; set; } = string.Empty;
        public string expires_in { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        //These are for non 200 responses
        public string status { get;set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }
}
