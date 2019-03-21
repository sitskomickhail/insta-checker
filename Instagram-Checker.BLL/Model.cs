using FileLibrary;
using InstagramLibrary;
using InstaLog;
using InstaSharper.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Instagram_Checker.BLL
{
    public class Model
    {
        private Proxy _proxy;
        private Accounts _account;
        private HttpAndroid _android;
        private AccountsMail _accMails;
        private FileWorker _fileWorker;

        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);


        private List<object> _objectsInstaLogs;
        private List<object> _objectsInstaProx;
        object locker = new object();

        public bool IsProxyInited { get; private set; }
        public bool IsAccountInited { get; private set; }
        public bool IsObjectsReady { get; private set; }
        public bool IsMailsReady { get; private set; }
        public bool IsProgramComplitlyEnded { get; private set; }
        public List<string> AccountInfoDataSet { get; set; }

        public Proxy GetProxy { get { return _proxy; } }
        public Accounts GetAccounts { get { return _account; } }
        public AccountsMail GetAccountsMail { get { return _accMails; } }



        public Model()
        {
            IsProxyInited = false;
            IsAccountInited = false;
            IsObjectsReady = false;
            IsMailsReady = false;
            IsProgramComplitlyEnded = false;
            AccountInfoDataSet = new List<string>();

            _proxy = new Proxy();
            _account = new Accounts();
            _android = new HttpAndroid();
            _accMails = new AccountsMail();
            _fileWorker = new FileWorker();
        }

        public void InitAccounts()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(Worker_InitAccounts);
            worker.RunWorkerCompleted += Worker_InitAccountsCompleted;
            worker.RunWorkerAsync();
        }

        public void InitProxy(bool isApiNeed, string key = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(Worker_InitProxy);
            worker.RunWorkerCompleted += Worker_InitProxyCompleted;
            object[] objs = new object[2] { isApiNeed, key };
            worker.RunWorkerAsync(objs);
        }

        public void InitObjects()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += InitObjects_DoWork;
            worker.RunWorkerCompleted += InitObjects_RunWorkerCompleted;

            object[] objs = new object[2] { _account.Users, _proxy.InstaProxies };
            worker.RunWorkerAsync(objs);
        }

        public void InitAccountsMail()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += AccountsMail_DoWork;
            worker.RunWorkerCompleted += AccountsMail_RunWorkerCompleted; ;
            worker.RunWorkerAsync();
        }


        public void CheckAllAccounts()
        {
            int pos = 0;
            var proxy = _proxy.InstaProxies;
            foreach (var obj in _objectsInstaLogs)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += CheckInsta_DoWork;

                try
                {
                    object[] options = new object[] { _objectsInstaProx[pos], obj };
                    worker.RunWorkerAsync(options);
                    pos++;
                }
                catch { break; }
            }
        }

        #region BackgroundMethods

        private async void CheckInsta_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] arg = (object[])e.Argument;
            List<Dictionary<string, object>> proxyOptions = (List<Dictionary<string, object>>)arg[0];
            List<Dictionary<string, string>> instOptions = (List<Dictionary<string, string>>)arg[1];

            

            int proxyCount = (int)Math.Floor((decimal)instOptions.Count / proxyOptions.Count);
            if (proxyCount == 0)
                proxyCount = 1;

            int proxyPos = 0;
            for (int i = 0; i < instOptions.Count; i++)
            {
                HttpAndroid android = new HttpAndroid();
                IResult<InstaLoginResult> result = null;
                try
                {
                    result = await android.Login(instOptions[i]["instaLogin"], instOptions[i]["instaPassword"],
                        proxyOptions[proxyPos]["ip"].ToString(), Int32.Parse(proxyOptions[proxyPos]["port"].ToString()), proxyOptions[proxyPos]["proxyLogin"].ToString(), proxyOptions[proxyPos]["proxyPassword"].ToString());
                }
                catch
                {
                    result = await android.Login(instOptions[i]["instaLogin"], instOptions[i]["instaPassword"],
                        proxyOptions[proxyPos]["ip"].ToString(), Int32.Parse(proxyOptions[proxyPos]["port"].ToString()));
                }
                if (result.Value.ToString() == "Success")
                {
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Success! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                }
                else if (result.Value.ToString() == "Challenge is required")
                {
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Challenge required! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                }
                else if (result.Info.Message == "Произошла ошибка при отправке запроса.")
                {
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Ошибка при отправке запроса! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                    i--;
                    proxyPos++;
                }
                else
                {
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"{result.Value}! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                    Console.WriteLine("wow");
                }
            }
        }






        private void AccountsMail_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsMailsReady = true;
        }

        private void AccountsMail_DoWork(object sender, DoWorkEventArgs e)
        {
            _accMails.GetMailsFromBaseFile();
            while (_accMails.MailsReady) { }
        }


        private void InitObjects_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsObjectsReady = true;
        }

        private void InitObjects_DoWork(object sender, DoWorkEventArgs e)
        {
            var usrs = _account.Users;
            var instProxy = _proxy.InstaProxies;
            int count = usrs.Count / 10000;

            _objectsInstaLogs = new List<object>();
            _objectsInstaProx = new List<object>();

            int pos = 0;
            int maxPos = 10000;
            for (int i = 0; i < count; i++)
            {
                bool check = false;
                List<Dictionary<string, string>> forObj = new List<Dictionary<string, string>>();
                for (int j = pos; j < maxPos; j++)
                {
                    try
                    {
                        Dictionary<string, string> user = usrs[j];
                        forObj.Add(user);
                    }
                    catch { check = true; break; }
                }
                if (forObj.Count > 0)
                    _objectsInstaLogs.Add(forObj);
                if (check)
                    break;
                pos += 10000;
                maxPos += 10000;
            }

            int countProx = instProxy.Count / _objectsInstaLogs.Count;
            if (countProx == 0)
                countProx = instProxy.Count;
            pos = 0;
            maxPos = countProx;
            for (int i = 0; i < count; i++)
            {
                bool check = false;
                List<Dictionary<string, object>> forObj = new List<Dictionary<string, object>>();
                for (int j = pos; j < maxPos; j++)
                {
                    try
                    {
                        Dictionary<string, object> proxy = instProxy[j];
                        forObj.Add(proxy);
                    }
                    catch { check = true; break; }
                }
                if (forObj.Count > 0)
                    _objectsInstaProx.Add(forObj);
                if (check)
                    break;
                pos += countProx;
                maxPos += countProx;
            }
        }


        private void Worker_InitProxyCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy inited! {_proxy.CountProxy} proxies are ready", Method = "Model.InitProxy" });
            IsProxyInited = true;
        }

        private void Worker_InitProxy(object sender, DoWorkEventArgs e)
        {
            object[] opts = (object[])e.Argument;
            if ((bool)opts[0])
                _proxy.GetProxy((string)opts[1]);
            _proxy.GetAllInfoFromFile();
            _proxy.InstaProxy_Init();
            _proxy.MailProxy_Init();
        }


        private void Worker_InitAccounts(object sender, DoWorkEventArgs e)
        {
            _account.GetAccountsFromBaseFile();
            while (true)
            {
                if (_account.UsersReady)
                    break;
            }
        }

        private void Worker_InitAccountsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Accounts inited! {_account.CountUsers} accounts are ready", Method = "Model.InitAccounts" });
            IsAccountInited = true;
        }

        #endregion
    }
}
