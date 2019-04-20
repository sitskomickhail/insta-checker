using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using InstaLog;
using System.Threading.Tasks;

namespace FileLibrary
{
    public class Proxy
    {
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        private List<Dictionary<string, object>> _insta_proxies;
        private List<Dictionary<string, object>> _mail_proxies;
        private const string _instaProxyPath = "Proxy_Insta.txt";
        private const string _mailProxyPath = "Proxy_Mail.txt";

        private int _countLinks;
        public object[] locker = new object[1];


        #region PROPS
        public List<Dictionary<string, object>> InstaProxies { get { lock (locker) { return _insta_proxies; } } }
        public List<Dictionary<string, object>> MailProxies { get { lock (locker) { return _mail_proxies; } } }
        public bool IsProxyReady { get; set; }
        #endregion

        public Proxy()
        {
            _insta_proxies = new List<Dictionary<string, object>>();
            _mail_proxies = new List<Dictionary<string, object>>();

            IsProxyReady = true;
        }

        public int CountProxy { get { return _insta_proxies.Count() + _mail_proxies.Count; } }


        public void GetRefProxy(List<string> hrefs)
        {
            IsProxyReady = false;
            _countLinks = hrefs.Count;
            foreach (var href in hrefs)
            {
                if (String.IsNullOrWhiteSpace(href))
                {
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Link doesn't exist", Method = "Proxy.GetRefProxy" });
                    _countLinks--;
                    continue;
                }

                Task.Run(() => AWMProxySetter_DoWork(href));
            }
        }

        public bool InstaProxy_Init()
        {
            var time = DateTime.Now;
            if (System.IO.File.Exists(_instaProxyPath))
            {
                int count = 0;
                string[] proxies = File.ReadAllLines(_instaProxyPath);
                foreach (string instaProxy in proxies)
                {
                    string[] splited = instaProxy.Split(':');

                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    try
                    {
                        dict.Add("port", Int32.Parse(splited[1]));
                        dict.Add("ip", splited[0]);

                        count++;
                        try
                        {
                            dict.Add("proxyLogin", splited[2]);
                            dict.Add("proxyPassword", splited[3]);
                        }
                        catch { }

                        _insta_proxies.Add(dict);
                    }
                    catch { }
                }
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy_Insta.txt returned {count} proxies", Method = "Proxy.InstaProxy_Init" });
            }
            else
            {
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = "Proxy_Insta.txt doesn't exist", Method = "Proxy.InstaProxy_Init" });
                return false;
            }
            return true;
        }

        public bool MailProxy_Init()
        {
            var time = DateTime.Now;
            if (System.IO.File.Exists(_mailProxyPath))
            {
                int count = 0;
                string[] proxies = File.ReadAllLines(_mailProxyPath);
                foreach (string instaProxy in proxies)
                {
                    string[] splited = instaProxy.Split(':');

                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    try
                    {
                        count++;
                        dict.Add("port", Int32.Parse(splited[1]));
                        dict.Add("ip", splited[0]);

                        try
                        {
                            dict.Add("proxyLogin", splited[2]);
                            dict.Add("proxyPassword", splited[3]);
                        }
                        catch { }

                        _mail_proxies.Add(dict);
                    }
                    catch { }
                }
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy_Mail.txt returned {count} proxies", Method = "Proxy.MailProxy_Init" });
            }
            else
            {
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = "Proxy_Mail.txt doesn't exist", Method = "Proxy.MailProxy_Init" });
                return false;
            }
            return true;
        }

        public void GetAllInfoFromFile()
        {
            string path = @"\base\Proxy\";

            if (Directory.Exists(Environment.CurrentDirectory + path))
            {
                int position = 0;
                var result = Directory.GetFiles(Environment.CurrentDirectory + path, "*.txt", SearchOption.AllDirectories);

                List<Thread> threads = new List<Thread>();
                foreach (string file in result)
                {
                    int count = 0;
                    string[] proxy = File.ReadAllLines(file);
                    for (int i = 0; i < proxy.Count(); i++)
                    {
                        proxy[i] = proxy[i].Replace(" ", String.Empty);
                        string[] splited = proxy[i].Split(':');

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        try
                        {
                            dict.Add("port", Int32.Parse(splited[1]));
                            dict.Add("ip", splited[0]);

                            try
                            {
                                dict.Add("proxyLogin", splited[2]);
                                dict.Add("proxyPassword", splited[3]);
                            }
                            catch { }

                            if (position == 0)
                            {
                                _mail_proxies.Add(dict);
                                position = 1;
                            }
                            else
                            {
                                _insta_proxies.Add(dict);
                                position = 0;
                            }
                            count++;
                        }
                        catch { }
                    }
                    string[] fileName = file.Split('\\');
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $@"file \base\{fileName[fileName.Count() - 1]} returned {count} proxies", Method = "Proxy.GetBase" });
                }

                if (_countLinks == 0)
                    IsProxyReady = true;
            }
        }

        public void ResolveProxy(int count)
        {
            while (_mail_proxies.Count > count)
            {
                _insta_proxies.Add(_mail_proxies[0]);
                _mail_proxies.Remove(_mail_proxies[0]);
            }
        }

        public void ClearInsta()
        {
            _insta_proxies.Clear();
        }

        #region BACKGROUND
        private int _linksUsed;
        private void AWMProxySetter_DoWork(string href)
        {
            HttpWebRequest webRequest = WebRequest.Create($"{href}") as HttpWebRequest;
            if (webRequest == null)

                webRequest.ContentType = "text/plain;charset=UTF-8";

            List<string> parsed;
            using (var s = webRequest.GetResponse().GetResponseStream())
            {
                using (var sr = new StreamReader(s))
                {
                    var contributorsAsJson = sr.ReadToEnd();
                    parsed = contributorsAsJson.Split('\n').ToList();
                }
            }
            
            for (int i = 0; i < parsed.Count; i++)
            {
                string[] proxy = parsed[i].Split(':');

                try
                {
                    Dictionary<string, object> prxInst = new Dictionary<string, object>();
                    prxInst.Add("ip", proxy[0]);
                    prxInst.Add("port", Int32.Parse(proxy[1]));

                    _insta_proxies.Add(prxInst);
                }
                catch { }
            }
            
            _linksUsed++;
            if (_linksUsed == _countLinks)
            {
                IsProxyReady = true;
                _linksUsed = 0;
            }
        }
        #endregion
    }
}