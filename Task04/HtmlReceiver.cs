using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Task04
{
    public static class HtmlUtils
    {
        public static string GetData(String url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            using var response = request?.GetResponse() as HttpWebResponse;
            Stream receiver = response?.GetResponseStream();

            if (receiver != null && response?.StatusCode == HttpStatusCode.OK)
            {
                using StreamReader reader = new StreamReader(receiver, Encoding.GetEncoding(response.CharacterSet));
                return reader.ReadToEnd();
            }
            throw new WebException();
        }
        
        public static async Task TraverseUrlsAsync(List<String> urls)
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
            return Task.Run(() => Console.WriteLine($"{url} -- {GetData(url).Length}"));
        }
    }
}