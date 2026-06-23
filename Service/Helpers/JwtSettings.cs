namespace Service.Helpers
{
    public class JwtSettings
    {
        public string Site { get; set; }
        public string Audience { get; set; }
        public string ExpiryTime { get; set; }
        public string SecretKey { get; set; }
    }
}