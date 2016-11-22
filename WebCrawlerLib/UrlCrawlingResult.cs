using System.Collections.Generic;

namespace WebCrawlerLib
{
    public class UrlCrawlingResult
    {
        public string Url { get; set; }

        public int VisitingCount { get; set; }

        public List<string> MatchingResult { get; set; }

        public UrlCrawlingResult(string url)
        {
            Url = url ?? string.Empty;
            VisitingCount = 1;
            MatchingResult = new List<string>();
        }
    }
}
