using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WebProvider;

namespace WebCrawlerLib
{
    public enum AccessType
    {
        Read = 0,
        Write = 1
    }

    public class WebCrawlerMgr
    {
        #region Parameters

        /// <summary>
        /// Start URL for scaning
        /// </summary>
        private readonly string _startUrl;

        /// <summary>
        /// Max depth for scaning
        /// </summary>
        private readonly int _maxDepth;

        /// <summary>
        /// Finding patterns
        /// </summary>
        private readonly List<string> _patterns;

        /// <summary>
        /// Is finding only host of start URL
        /// </summary>
        private readonly bool _isOnlyStartUrlHost;

        #endregion

        #region Runtime vars

        /// <summary>
        /// Current level of depth
        /// </summary>
        private int _currentDepth = 1;

        /// <summary>
        /// Thread for asynhronical execution of scaning
        /// </summary>
        private Thread _threadForScan;

        /// <summary>
        /// Is scanig complete
        /// </summary>
        public bool IsComplete = true;

        /// <summary>
        /// Result of scaning
        /// </summary>
        private List<UrlCrawlingResult> _visitedUrls;

        /// <summary>
        /// Authentification context
        /// </summary>
        private readonly UserAutentificationContext _authContext;

        /// <summary>
        /// Web worker
        /// </summary>
        private readonly WebWorker _webWorker;

        private readonly bool _isUsingLogin;

        #endregion

        #region Ctors

        public WebCrawlerMgr(string startUrl, int maxDepth, List<string> patterns, bool isOnlyStartUrlHost)
        {
            _startUrl = startUrl ?? string.Empty;
            _maxDepth = maxDepth > 0 ? maxDepth : Consts.DefaultMaxDepth;
            _patterns = patterns ?? new List<string>();
            _isOnlyStartUrlHost = isOnlyStartUrlHost;
            _authContext = null;
            _isUsingLogin = false;
            _webWorker = new WebWorker();
        }

        public WebCrawlerMgr(string startUrl, int maxDepth, List<string> patterns, bool isOnlyStartUrlHost, UserAutentificationContext authContext)
            : this(startUrl, maxDepth, patterns, isOnlyStartUrlHost)
        {
            _isUsingLogin = true;
            _authContext = authContext;
        }

        #endregion

        #region API

        public void StartScan()
        {
            IsComplete = false;
            _threadForScan = new Thread(ScanInThread);
            _threadForScan.Start();
        }

        public void StopScan()
        {
            IsComplete = true;
            if (_threadForScan != null) _threadForScan.Abort();
        }

        #endregion

        #region Private methods

        private void ScanInThread()
        {
            if (_isUsingLogin && _authContext != null)
                _webWorker.LoginByPostRequest(_authContext.LoginUrl, _authContext.LoginElementName,
                    _authContext.PasswordElementName, _authContext.Login, _authContext.Password, 
                    _authContext.SidKey, _authContext.AdditionalRequestString);
            ScanUrls(new List<string> { _startUrl }, _patterns);
            IsComplete = true;
        }

        private void ScanUrls(List<string> urls, List<string> patterns)
        {
            if (urls == null || patterns == null || urls.Count == 0 || patterns.Count == 0) return;

            var htmlsOfUrls = new List<string>();

            foreach(var url in urls)
            {
                var checkingUrl = ClearUrl(url);
                //Проверяем пробегали ли мы уже эту страницу
                var existItem = GetVisitedUrls().SingleOrDefault(g => g.Url == checkingUrl);
                if (existItem != null)
                {
                    existItem.VisitingCount++;
                    continue;
                }

                //Результаты поиска на текущей странице
                string currentHtml;
                ScanUrl(checkingUrl, patterns, out currentHtml);

                if (!string.IsNullOrEmpty(currentHtml))
                    htmlsOfUrls.Add(currentHtml);
            }

            _currentDepth++;
            if (_currentDepth > _maxDepth) return;

            var linksForNextScan = new List<string>();
            foreach (var currentHtml in htmlsOfUrls)
                linksForNextScan.AddRange(HtmlUtils.GetLinksFromHtml(currentHtml)
                    .Select(g => ClearUrl(WebWorker.ResolveUrl(g, _startUrl)))
                    .Where(h => GetVisitedUrls().SingleOrDefault(g => g.Url == h) == null
                        && IsInHost(h, _startUrl)));

            htmlsOfUrls = null;
            urls = null;

            ScanUrls(linksForNextScan, patterns);
        }

        private void ScanUrl(string url, List<string> patterns, out string html)
        {
            if (string.IsNullOrEmpty(url))
            {
                html = string.Empty;
                return;
            }
                
            patterns = patterns ?? new List<string>();
            var currentStringResults = HtmlUtils.GetMatchesByPatternsInHtmlByUrl(_webWorker, url, patterns, out html);
            AddToVisitedUrls(new UrlCrawlingResult(url) { MatchingResult = currentStringResults });
        }

        #endregion

        #region Visited urls access

        /// <summary>
        /// Safely adding result to list of results
        /// </summary>
        private void AddToVisitedUrls(UrlCrawlingResult ucResult)
        {
            AccessToVisitedUrls(AccessType.Write, ucResult);
        }

        /// <summary>
        /// Safely get results
        /// </summary>
        public List<UrlCrawlingResult> GetVisitedUrls()
        {
            return AccessToVisitedUrls(AccessType.Read);
        }

        private List<UrlCrawlingResult> AccessToVisitedUrls(AccessType accessType, UrlCrawlingResult addingElement = null, int startElem = 0)
        {
            lock (this)
            {
                if (_visitedUrls == null) _visitedUrls = new List<UrlCrawlingResult>();
                switch (accessType)
                {
                    case AccessType.Read:
                        if (startElem >= _visitedUrls.Count) return new List<UrlCrawlingResult>();
                        var localResult = new List<UrlCrawlingResult>();
                        for (var i = startElem; i < _visitedUrls.Count; i++)
                            localResult.Add(_visitedUrls[i]);
                        return localResult;
                    case AccessType.Write:
                        if (addingElement != null)
                            _visitedUrls.Add(addingElement);
                        return null;
                    default:
                        return null;
                }
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Clear URL from different symbols
        /// </summary>
        private static string ClearUrl(string url)
        {
            url = url ?? string.Empty;
            return url.Trim(' ', '/');
        }

        /// <summary>
        /// Return true if URL in host of hostUrl
        /// </summary>
        private bool IsInHost(string url, string hostUrl)
        {
            try
            {
                if (!_isOnlyStartUrlHost) return true;
                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(hostUrl)) return false;
                var checkUri = new Uri(url);
                var hostUri = new Uri(hostUrl);
                return checkUri.Authority == hostUri.Authority;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
