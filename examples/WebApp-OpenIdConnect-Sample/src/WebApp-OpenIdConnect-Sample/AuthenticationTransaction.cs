namespace WebApp
{
    public class AuthenticationTransaction
    {
        public string Nonce
        {
            get;
        }

        public string State
        {
            get;
        }

        public AuthenticationTransaction(string nonce, string state)
        {
            this.Nonce = nonce;
            this.State = state;
        }
    }
}
