using System.Collections.Generic;

namespace WebCrawlerLib
{
    class Consts
    {
        public const int DefaultMaxDepth = 2;
        public static readonly List<string> LinkRegexes = 
            new List<string>
                {
                    //"href\\s*=\\s*(?:\"|'(?<1>[^\"|']*)\"|(?<1>\\S+))"
                    @"href\s*=\s*[""']{1}([^""']+)[""']{1}"
                };
    }
}
