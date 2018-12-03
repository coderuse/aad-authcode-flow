namespace AuthCodeFlow.Models
{
    public class TokenRequest
    {
        public string Code { get; set; }
        public string GrantType { get; set; }
        public string RefreshToken { get; set; }
    }
}