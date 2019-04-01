using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using InstaLog;

namespace FileLibrary
{
    public class Proxy
    {
        private List<Dictionary<string, object>> _insta_proxies;
        private List<Dictionary<string, object>> _mail_proxies;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        public object[] locker = new object[1];

        private List<Thread> _threads;

        private const string _instaProxyPath = "Proxy_Insta.txt";
        private const string _mailProxyPath = "Proxy_Mail.txt";

        public Proxy()
        {
            _threads = new List<Thread>();
            _insta_proxies = new List<Dictionary<string, object>>();
            _mail_proxies = new List<Dictionary<string, object>>();
        }

        public int CountProxy { get { return _insta_proxies.Count() + _mail_proxies.Count; } }

        public bool GetProxy(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy key doesn't exist", Method = "Proxy.JSonProxy" });
                return false;
            }

            HttpWebRequest webRequest = WebRequest.Create
                ($"http://api.best-proxies.ru/proxylist.json?key={key}&limit=0")
                            as HttpWebRequest;
            if (webRequest == null)
                return false;

            webRequest.ContentType = "application/json";

            try
            {
                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var contributorsAsJson = sr.ReadToEnd();
                        var result = JsonConvert.DeserializeObject<List<ApiProxy>>(contributorsAsJson);

                        int position = result.Count / 2;
                        for (int i = 0; i < position; i++)
                        {
                            //if (result[i].http != "1" && result[i].https != "1")
                            //    continue;
                            Dictionary<string, object> prxInst = new Dictionary<string, object>();
                            prxInst.Add("ip", result[i].real_ip);
                            prxInst.Add("port", Int32.Parse(result[i].port));

                            _insta_proxies.Add(prxInst);

                            try
                            {
                                Dictionary<string, object> prxMail = new Dictionary<string, object>();
                                prxMail.Add("ip", result[i + position].real_ip);
                                prxMail.Add("port", Int32.Parse(result[i + position].port));

                                _mail_proxies.Add(prxMail);
                            }
                            catch { }
                        }
                        logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Api returned {result.Count} proxy accounts", Method = "Proxy.JSonProxy" });
                    }
                }
            }
            catch (Exception e)
            {
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = e.Message, Method = "Proxy.JSonProxy" });
                return false;
            }
            return true;
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
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy_Insta.txt returned {count} proxies", Method = "Proxy.InstaProxy_Init" });
            }
            else
            {
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = "Proxy_Insta.txt doesn't exist", Method = "Proxy.InstaProxy_Init" });
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
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy_Mail.txt returned {count} proxies", Method = "Proxy.MailProxy_Init" });
            }
            else
            {
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = "Proxy_Mail.txt doesn't exist", Method = "Proxy.MailProxy_Init" });
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
                    logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $@"file \base\{fileName[fileName.Count() - 1]} returned {count} proxies", Method = "Proxy.GetBase" });
                }
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

        #region PROPS
        public List<Dictionary<string, object>> InstaProxies { get { lock (locker) { return _insta_proxies; } } }
        public List<Dictionary<string, object>> MailProxies { get { lock (locker) { return _mail_proxies; } } }
        #endregion
    }
}


public class ApiProxy
{
    public string ip { get; set; }
    public string port { get; set; }
    public string http { get; set; }
    public string https { get; set; }
    public string socks4 { get; set; }
    public string socks5 { get; set; }
    public string level { get; set; }
    public string yandex { get; set; }
    public string google { get; set; }
    public string mailru { get; set; }
    public string twitter { get; set; }
    public string country_code { get; set; }
    public string response { get; set; }
    public string good_count { get; set; }
    public string bad_count { get; set; }
    public string last_check { get; set; }
    public string city { get; set; }
    public string region { get; set; }
    public string real_ip { get; set; }
    public string test_time { get; set; }
    public object me { get; set; }
}