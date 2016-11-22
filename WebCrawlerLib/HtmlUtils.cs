using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WebProvider;
using System.Web;

namespace WebCrawlerLib
{
    class HtmlUtils
    {
        #region Links utils

        /// <summary>
        /// Get links in page with current URL
        /// </summary>
        public static List<string> GetLinksFromHtmlByUrl(WebWorker webWorkerInst, string url, out string html)
        {
            if (string.IsNullOrEmpty(url) || webWorkerInst == null)
            {
                html = string.Empty;
                return new List<string>();
            }

            try
            {
                html = webWorkerInst.LoadHtml(url, Encoding.Default);
                return GetLinksFromHtml(html);
            }
            catch (Exception)
            {
                html = string.Empty;
                return new List<string>();
            }
        }

        /// <summary>
        /// Get link from HTML
        /// </summary>
        public static List<string> GetLinksFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return new List<string>();
            var result = new List<string>();

            foreach (var pattern in Consts.LinkRegexes)
            {
                try
                {
                    var regexMgr = new Regex(pattern);
                    var matches = regexMgr.Matches(html);
                    foreach (var matche in matches)
                        result.Add(HttpUtility.HtmlDecode(GetLinkCleanByTags(matche.ToString())));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return result;
        }

        /// <summary>
        /// Get clear link by match
        /// </summary>
        private static string GetLinkCleanByTags(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var tempString = value.Substring(value.IndexOf("href")).TrimStart('h', 'r', 'e', 'f', ' ', '=');
            return tempString.Trim('"', '\'');
        }

        #endregion

        #region Matches utils

        /// <summary>
        /// Get matches by pattern in HTML
        /// </summary>
        public static List<string> GetMatchesByPatternsInHtmlByUrl(WebWorker webWorkerInst, string url, List<string> patterns, out string html)
        {
            if (string.IsNullOrEmpty(url) || webWorkerInst == null)
            {
                html = string.Empty;
                return new List<string>();
            }

            patterns = patterns ?? new List<string>();
            try
            {
                html = webWorkerInst.LoadHtml(url, Encoding.Default);
                return GetMatchesByPatternsInHtml(html, patterns);
            }
            catch (Exception)
            {
                html = string.Empty;
                return new List<string>();
            }
        }

        /// <summary>
        /// Get matches by pattern ib HTML by URL
        /// </summary>
        public static List<string> GetMatchesByPatternsInHtml(string html, List<string> patterns)
        {
            if (string.IsNullOrEmpty(html)) return new List<string>();
            patterns = patterns ?? new List<string>();
            var result = new List<string>();

            foreach (var pattern in patterns)
            {
                try
                {
                    var regexMgr = new Regex(pattern);
                    var matches = regexMgr.Matches(html);
                    foreach (var matche in matches)
                        result.Add(matche.ToString());
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return result;
        }

        #endregion
    }
}
