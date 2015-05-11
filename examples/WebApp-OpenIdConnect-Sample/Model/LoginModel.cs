namespace WebApp.Model
{
    public class LoginModel
    {
        public string ReturnUrl { get; set; }

        public string State { get; set; }

        public string Nonce { get; set; }
    }
}
