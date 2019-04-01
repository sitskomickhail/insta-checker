using System;
using System.Net;

namespace InstagramLibrary
{
    public class HttpRequestBuilder
    {
        static HttpRequestBuilder()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        private static HttpWebRequest CreateRequest(Uri uri, string userAgent, CookieContainer cookies = null)
        {
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Timeout = 10000;
            request.Host = uri.Host;
            request.Accept = "*/*";
            request.KeepAlive = true;

            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            request.UserAgent = $"{userAgent}";
            if (cookies != null) request.CookieContainer = cookies;
            return request;
        }

        public static HttpWebRequest Post(string url,string userAgent, CookieContainer cookies = null)
        {
            var request = CreateRequest(new Uri(url), userAgent, cookies);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public static HttpWebRequest Get(string url, string userAgent, CookieContainer cookies = null)
        {
            var request = CreateRequest(new Uri(url), userAgent, cookies);
            request.Method = WebRequestMethods.Http.Get;
            return request;
        }
    }
}