using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WebProvider
{
    public class WebWorker
    {
        #region Consts

        private const string SetCookie = "Set-Cookie";

        private const string Cookie = "Cookie";

        #endregion

        #region Parameters

        private string _currentCookieString;

        private bool _isLogined = false;

        #endregion

        #region Main methods

        /// <summary>
        /// Return page content as string variable by url
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="encoding">Encoding source website</param>
        /// <returns>Page content</returns>
        public string LoadHtml(string url, Encoding encoding)
        {
            try
            {
                if (!IsUrl(url))
                {
                    throw new Exception("Incorrect URL");
                }
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "text/html";
                if (_isLogined && !string.IsNullOrEmpty(_currentCookieString))
                    request.Headers[Cookie] = _currentCookieString;
                    
                return new StreamReader(request.GetResponse().GetResponseStream(), encoding).ReadToEnd();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Login on the web application
        /// </summary>
        public void LoginByPostRequest(string url, string loginElementName, 
            string passwordElementName, string login, string password, 
            string sidKey, string additionalRequestString)
        {
            try
            {
                if (!IsUrl(url))
                {
                    throw new Exception("Incorrect URL");
                }
                var uri = new Uri(url);
                var urlWithOutParams = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, uri.AbsolutePath).Trim(' ', '/');
                var urlWithParams = urlWithOutParams
                                    //+ "?login=vovanmob88%40mail.ru&password=moh42lul&remember=1&x=81&y=21";
                    + string.Format("?{0}={1}&{2}={3}&{4}", loginElementName, login, 
                    passwordElementName, password, additionalRequestString.Trim(' ', '&'));

                var requestGet = (HttpWebRequest)WebRequest.Create(urlWithOutParams);
                requestGet.Method = "GET";
                var responseGet = (HttpWebResponse)requestGet.GetResponse();
                var cookiesAfterGet = responseGet.Headers[SetCookie] ?? string.Empty;

                //var PHPSESSID = GetParamFromCookieString(cookiesAfterGet, "PHPSESSID");
                //var uid = GetParamFromCookieString(cookiesAfterGet, "uid");

                var request = (HttpWebRequest)WebRequest.Create(urlWithParams);
                request.Method = "POST";
                request.Headers[Cookie] = cookiesAfterGet;
                //////request.Headers.Add("Host", "www.livemaster.ru");
                //////User-Agent	Mozilla/5.0 (Windows; U; Windows NT 6.0; ru; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12
                ////request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                ////request.Headers.Set("Accept-Language", "ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3");
                ////request.Headers.Set("Accept-Encoding", "gzip,deflate");
                ////request.Headers.Set("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.7");
                //////request.KeepAlive = 115;
                //////request.Connection = "keep-alive";
                //request.Headers[Cookie] =
                //    "migrated=1; " +
                //    string.Format("uid={0}; ", uid) +
                //    //"__utma=1.1241400063.1294752694.1294754824.1294823630.3; " + 
                //    //"__utmz=1.1294752694.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); " +
                //    //"__utmc=1; " +
                //    string.Format("PHPSESSID={0}; ", PHPSESSID);// + 
                //    //"__utmb=1.6.10.1294823630";
                var response = (HttpWebResponse)request.GetResponse();

                var answer = new StreamReader(response.GetResponseStream(), Encoding.Default).ReadToEnd();
                
                _currentCookieString = response.Headers[SetCookie] ?? string.Empty;

                //string url = "http://websiteToSubmitTo";
                //HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                //string proxy = null;

                //string data = String.Format("parameter1={0}&parameter2={1}&parameter3={2}", parameter1, parameter2, parameter3);
                //byte[] buffer = Encoding.UTF8.GetBytes(data);

                //req.Method = "POST";
                //req.ContentType = "application/x-www-form-urlencoded";
                //req.ContentLength = buffer.Length;
                //req.Proxy = new WebProxy(proxy, true); // ignore for local addresses 
                //req.CookieContainer = new CookieContainer(); // enable cookies 

                //Stream reqst = req.GetRequestStream(); // add form data to request stream 
                //reqst.Write(buffer, 0, buffer.Length);
                //reqst.Flush();
                //reqst.Close(); 


                _isLogined = true;
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Resolve URL by base URL
        /// </summary>
        public static string ResolveUrl(string url, string baseUrl)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(baseUrl)) return string.Empty;
            if (!IsUrl(baseUrl)) return url;
            var processingUrl = new Uri(baseUrl);

            if (url.Trim().StartsWith("/"))
                return string.Format("{0}://{1}{2}", processingUrl.Scheme, processingUrl.Authority, url);
            try
            {
                var uri = new Uri(url.Trim());
                return uri.ToString();
            }
            catch (Exception)
            {
                return string.Format("{0}://{1}/{2}", processingUrl.Scheme, processingUrl.Authority, url.Trim('/'));
            }
        }

        /// <summary> 
        /// Check format url
        /// </summary>
        /// <param name="url">checking URL</param> 
        private static bool IsUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url.IndexOf(' ') < 0) && Regex.IsMatch(url, Consts.URL_REGEX));
        }

        private static Dictionary<string, string> GetParamsFromCookieString(string cookieString)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(cookieString)) return result;
            var regex = new Regex(Consts.COOKIE_PARSING_REGEX);
            var matches = regex.Matches(cookieString);
            foreach (var matche in matches)
            {
                var onePair = matche.ToString().Trim(' ', ';');
                if (!onePair.Contains("=")) continue;
                var splitIndex = onePair.IndexOf("=");
                var key = onePair.Substring(0, splitIndex);
                var value = onePair.Substring(splitIndex + 1);
                if (value.IndexOf(",") >= 0)
                    value = value.Substring(0, value.IndexOf(","));

                if (!result.ContainsKey(key))
                    result.Add(key, value);
            }
            return result;
        }

        /// <summary>
        /// Get session ID from cookie string
        /// </summary>
        private static string GetParamFromCookieString(string cookieString, string key)
        {
            if (string.IsNullOrEmpty(cookieString) || string.IsNullOrEmpty(key)) return string.Empty;
            var regex = new Regex(string.Format(@"{0}\s*=\s*[^;=]+;", key));
            var result = regex.Match(cookieString).ToString();
            if (string.IsNullOrEmpty(result)) return string.Empty;
            return result.Substring(result.IndexOf("=") + 1).Trim(' ', ';');
        }

        #endregion
    }
}
