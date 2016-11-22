using System.ComponentModel;

namespace WebCrawler
{
    public class CrawlingSettings
    {
        [DisplayName(@"URL для поиска")]
        [Category("Основные")]
        public string Url { get; set; }

        [DisplayName(@"Только на заданном сайте")]
        [Category("Основные")]
        public bool IsOnlyOnCurrentHost { get; set; }

        [DisplayName(@"Искомый шаблон")]
        [Category("Основные")]
        public string Pattern { get; set; }

        [DisplayName(@"Максимальная глубина")]
        [Category("Основные")]
        public int MaxDepth { get; set; }

        //[DisplayName(@"Использовать логин")]
        //[Category("Логин")]
        //public bool IsUseLogin { get; set; }

        //[DisplayName(@"URL для логина")]
        //[Category("Логин")]
        //public string LoginUrl { get; set; }

        //[DisplayName(@"HTML элемент для логина")]
        //[Category("Логин")]
        //public string LoginElemName { get; set; }

        //[DisplayName(@"Логин")]
        //[Category("Логин")]
        //public string Login { get; set; }

        //[DisplayName(@"HTML элемент для пароля")]
        //[Category("Логин")]
        //public string PasswordElemName { get; set; }

        //[DisplayName(@"Пароль")]
        //[Category("Логин")]
        //[PasswordPropertyText(true)]
        //public string Password { get; set; }
        
        //[DisplayName(@"Дополнительная строка запроса")]
        //[Category("Логин")]
        //public string AdditionalRequestString { get; set; }

        public CrawlingSettings(string url, bool isOnlyOnCurrentHost, 
            string pattern, int maxDepth, bool isUseLogin, string loginUrl,
            string loginElemName, string login, string passwordElemName,
            string password, string additionalRequestString)
        {
            Url = url ?? string.Empty;
            IsOnlyOnCurrentHost = isOnlyOnCurrentHost;
            Pattern = pattern ?? string.Empty;
            MaxDepth = maxDepth;
            //IsUseLogin = isUseLogin;
            //LoginUrl = loginUrl ?? string.Empty;
            //LoginElemName = loginElemName ?? string.Empty;
            //Login = login ?? string.Empty;
            //PasswordElemName = passwordElemName ?? string.Empty;
            //Password = password ?? string.Empty;
            //AdditionalRequestString = additionalRequestString ?? string.Empty;
        }
    }
}
