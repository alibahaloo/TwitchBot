namespace TwitchBot.Data
{
    public class TwitchChatterDataDTO
    {
        public string user_id { get; set; } = string.Empty;
        public string user_login { get; set; } = string.Empty;
        public string user_name { get; set; } = string.Empty;
    }
    public class TwitchChattersDTO
    {
        public int total { get; set; } = 0;
        public List<TwitchChatterDataDTO> Data { get; set; } = new List<TwitchChatterDataDTO>();
    }
    public class TokenDTO
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
