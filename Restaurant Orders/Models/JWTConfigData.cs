namespace Restaurant_Orders.Models
{
    public class JWTConfigData
    {
        public const string ConfigSectionName = "JWTInfo";

        public string Subject { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public int ExpireInMinutes { get; set; }
        public string Secret { get; set; }
    }
}
