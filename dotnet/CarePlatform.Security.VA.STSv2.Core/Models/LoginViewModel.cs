namespace CarePlatform.Security.VA.STSv2.Models
{
    public class LoginViewModel
    {
        public string PostBackUrl { get; set; }
        public string EncodedToken { get; set; }
        public string OriginalUri { get; set; }
    }
}
