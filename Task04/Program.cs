using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Task04
{
    class Program
    {
        public static void Main()
        {
            string url = "https://github.com/EgorkaKulikov/spbu-se2019-autumn/pulls";
            try
            {
                string data = HtmlUtils.GetData(url);
                var matches = 
                    Regex.Matches(data, @"<a.*href=""(http|https)://(\S*)""")
                    .Select(match => match.Value)
                    .Select(tag =>
                    {
                        string prefix = "href=\"";
                        var startIndex = tag.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length;
                        var length = tag.Length - startIndex - 1;
                        return tag.Substring(startIndex, length);
                    })
                    .ToList();
                HtmlUtils.TraverseUrlsAsync(matches).GetAwaiter().GetResult();
            }
            catch (WebException)
            {
                Console.WriteLine($"Troubles with response from {url} or status code is not 200");
            }
        }
    }
}