namespace WebCrawlerLib
{
    public class UserAutentificationContext
    {
        public string LoginUrl { get; set; }

        public string LoginElementName { get; private set; }

        public string PasswordElementName { get; private set; }

        public string Login { get; private set; }

        public string Password { get; private set; }

        public string SidKey { get; private set; }

        public string AdditionalRequestString { get; set; }

        public UserAutentificationContext(string loginUrl, string loginElementName, 
            string passwordElementName, string login, string password,
            string sidKey, string additionalRequestString)
        {
            LoginUrl = loginUrl ?? string.Empty;
            LoginElementName = loginElementName ?? string.Empty;
            PasswordElementName = passwordElementName ?? string.Empty;
            Login = login ?? string.Empty;
            Password = password ?? string.Empty;
            SidKey = sidKey ?? string.Empty;
            AdditionalRequestString = additionalRequestString ?? string.Empty;
        }
    }
}
