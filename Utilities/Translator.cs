using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Linguist.Utilities
{
    public static class Translator
    {
        // TODO: improve the cache
        private static readonly ConcurrentDictionary<string, string> _cachedTranslation = new();

        public static string Translate(string lang, string text)
        {
            var key = lang + ":" + text.Trim();
            if (_cachedTranslation.ContainsKey(key))
                return _cachedTranslation[key];
            
            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query.Add("client", "dict-chrome-ex");
                query.Add("sl", "auto");
                query.Add("tl", lang);
                query.Add("q", text);

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");
                httpClient.Timeout = TimeSpan.FromSeconds(2);
                var json = JArray.Parse(httpClient.GetStringAsync("https://clients5.google.com/translate_a/t?" + query)
                    .GetAwaiter().GetResult());

                var builder = new StringBuilder();
                foreach (var sentence in json)
                {
                    builder.Append(sentence);
                    builder.Append(' ');
                }

                var translation = builder.ToString().TrimEnd();
                _cachedTranslation.TryAdd(key, translation);
                
                // purge cache if too many entries
                if (_cachedTranslation.Count > 100)
                    _cachedTranslation.Clear();

                return translation;
            }
            catch
            {
                return text;
            }
        }
    }
}