using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace FileLibrary
{
    public class Proxy
    {
        private List<Dictionary<string, object>> _insta_proxies;
        private List<Dictionary<string, object>> _mail_proxies;

        private List<Thread> _threads;

        private const string _instaProxyPath = "Proxy_Insta.txt";
        private const string _mailProxyPath = "Proxy_Mail.txt";

        public Proxy()
        {
            _threads = new List<Thread>();
            _insta_proxies = new List<Dictionary<string, object>>();
            _mail_proxies = new List<Dictionary<string, object>>();
        }


        public bool GetProxy()
        {
            var connectionStrings = ConfigurationManager.ConnectionStrings;
            string proxyKey = connectionStrings["proxyKey"].ConnectionString;

            HttpWebRequest webRequest = WebRequest.Create
                ($"http://api.best-proxies.ru/proxylist.json?key={proxyKey}&limit=0")
                            as HttpWebRequest;
            if (webRequest == null)
                return false;

            webRequest.ContentType = "application/json";

            using (var s = webRequest.GetResponse().GetResponseStream())
            {
                using (var sr = new StreamReader(s))
                {
                    var contributorsAsJson = sr.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<Rootobject>(contributorsAsJson);

                    for (int i = 0; i < result.Property1.Count() / 2; i++)
                    {
                        
                    }

                    s.Close();
                    sr.Close();
                }
            }
            return true;
        }

        public bool InitProxyDictionary()
        {
            var time = DateTime.Now;
            if (File.Exists(_instaProxyPath) && File.Exists(_mailProxyPath))
            {
                int linesAVGcount = 100;

                _threads.Add(new Thread(p =>
                {
                    List<Thread> thrds = new List<Thread>();
                    string[] res = File.ReadAllLines(_instaProxyPath);

                    int countPosition = 0;
                    int count = 0;
                    int threadsCount = (int)Math.Floor((double)res.Count() % linesAVGcount);
                    for (int i = 0; i < threadsCount; i++)
                    {

                        thrds.Add(new Thread(r =>
                        {
                            if (countPosition == threadsCount - 1)
                                count = res.Count();
                            else
                                count = countPosition * linesAVGcount + linesAVGcount + 100;
                            countPosition++;

                            for (int j = countPosition * linesAVGcount; j < count; j++)
                            {
                                string str = res[j];
                                string[] proxyInit = str.Split(':');

                                Dictionary<string, object> dict = new Dictionary<string, object>();
                                dict.Add("ip", proxyInit[0].ToString());
                                //dict.Add("port", Int32.Parse(proxyInit[1]));
                                //dict.Add("login", proxyInit[2].ToString());
                                //dict.Add("pass", proxyInit[3].ToString());

                                _insta_proxies.Add(dict);
                            }

                        }));
                    }

                    foreach (Thread item in thrds)
                    {
                        item.Start();
                        Thread.Sleep(1000);
                    }
                }));

                _threads.Add(new Thread(p =>
                {
                    List<Thread> thrds = new List<Thread>();
                    string[] res = File.ReadAllLines(_mailProxyPath);

                    int countPosition = 0;
                    int count = 0;
                    int threadsCount = (int)Math.Floor((double)res.Count() % linesAVGcount);
                    for (int i = 0; i < threadsCount; i++)
                    {

                        thrds.Add(new Thread(r =>
                        {
                            if (countPosition == threadsCount - 1)
                                count = res.Count();
                            else
                                count = countPosition * linesAVGcount + linesAVGcount + 100;
                            countPosition++;

                            for (int j = countPosition * linesAVGcount; j < count; j++)
                            {
                                string str = res[j];
                                string[] proxyInit = str.Split(':');

                                Dictionary<string, object> dict = new Dictionary<string, object>();
                                dict.Add("ip", proxyInit[0].ToString());
                                //dict.Add("port", Int32.Parse(proxyInit[1]));
                                //dict.Add("login", proxyInit[2].ToString());
                                //dict.Add("pass", proxyInit[3].ToString());

                                _mail_proxies.Add(dict);
                            }

                        }));
                    }

                    foreach (Thread item in thrds)
                    {
                        item.Start();
                        Thread.Sleep(1000);
                    }
                }));


                foreach (var thread in _threads)
                {
                    thread.Start();
                }
            }
            else
            {

            }
            return true;
        }


        private void GetAllInfoFromFile(string filePath)
        {

        }

        #region PROPS
        public List<Dictionary<string, object>> InstaProxies { get { return _insta_proxies; } }
        #endregion
    }
}



public class Rootobject
{
    public ApiProxy[] Property1 { get; set; }
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
    public string me { get; set; }
}
