using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Task04
{
    static class Program
    {
        public static async Task Main()
        {
            string url = "https://github.com/EgorkaKulikov/spbu-se2019-autumn/pulls";
            try
            {
                string data = GetDataFromUrl(url);
                var matches = Regex.Matches(data, @"<a.*href=""(http|https)://(\S*)""")
                    .Select(match => match.Value)
                    .Select(tag =>
                    {
                        var startIndex = tag.IndexOf("href=\"", StringComparison.Ordinal) + "href=\"".Length;
                        return tag.Substring(startIndex,tag.Length - startIndex - 1);
                    })
                    .ToList();
                await ProcessUrls(matches);
            }
            catch (WebException)
            {
                Console.WriteLine($"Troubles with response from {url} or status code is not 200");
            }
        }

        public static async Task ProcessUrls(List<String> urls)
        {
            try
            {
                await Task.WhenAll(urls.Select(ProcessUrlAsync));
            }
            catch (WebException)
            {
                Console.WriteLine("Troubles with response from one of urls or status code is not 200");
            }
        }

        private static Task ProcessUrlAsync(String url)
        {
            return Task.Run(() => Console.WriteLine($"{url} -- {GetDataFromUrl(url).Length}"));
        }

        private static string GetDataFromUrl(String url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            using HttpWebResponse response = request?.GetResponse() as HttpWebResponse;
            Stream receiver = response?.GetResponseStream();

            if (response != null && receiver != null && response.StatusCode == HttpStatusCode.OK)
            {
                using StreamReader reader = new StreamReader(receiver, Encoding.GetEncoding(response.CharacterSet));
                return reader.ReadToEnd();
            }
            throw new WebException();
        }
    }
}