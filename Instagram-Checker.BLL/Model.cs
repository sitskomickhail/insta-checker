using FileLibrary;
using InstagramLibrary;
using InstaLog;
using InstaSharper.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Instagram_Checker.BLL
{
    public class Model
    {
        public object[] locker = new object[1];

        private Proxy _proxy;
        private Accounts _account;
        private HttpAndroid _android;
        private AccountsMail _accMails;
        private FileWorker _fileWorker;

        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);


        private List<object> _objectsInstaLogs;
        private List<object> _objectsInstaProx;

        public bool IsProgramComplitlyEnded { get; private set; }
        public bool IsAccountInited { get; private set; }
        public bool IsObjectsReady { get; private set; }
        public bool IsProxyInited { get; private set; }
        public bool IsMailsReady { get; private set; }
        public int ProxySwitched { get; private set; }
        public int ProxyBlocked { get; private set; }
        public int AccsSwitched { get; private set; }
        public int AccsBlocked { get; private set; }

        public bool NeedMoreProxy { get; set; }


        public List<string> AccountInfoDataSet_Required { get; private set; }
        public List<string> AccountInfoDataSet_Success { get; private set; }

        public Proxy GetProxy { get { return _proxy; } }
        public Accounts GetAccounts { get { return _account; } }
        public AccountsMail GetAccountsMail { get { return _accMails; } }

        public Model()
        {
            AccsBlocked = 0;
            ProxyBlocked = 0;
            AccsSwitched = 0;
            ProxySwitched = 0;
            IsMailsReady = false;
            IsProxyInited = false;
            IsObjectsReady = false;
            IsAccountInited = false;
            IsProgramComplitlyEnded = false;
            AccountInfoDataSet_Success = new List<string>();
            AccountInfoDataSet_Required = new List<string>();

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

        public void InitObjects(int countThreads, int splitCount)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += InitObjects_DoWork;
            worker.RunWorkerCompleted += InitObjects_RunWorkerCompleted;

            object[] objs = new object[4] { _account.Users, _proxy.InstaProxies, countThreads, splitCount };
            worker.RunWorkerAsync(objs);
        }

        public void InitAccountsMail()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += AccountsMail_DoWork;
            worker.RunWorkerCompleted += AccountsMail_RunWorkerCompleted; ;
            worker.RunWorkerAsync();
        }

        public void UpdateProxy(string key)
        {
            if (key != null)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += UpdateProxy_DoWork;
                worker.RunWorkerCompleted += UpdateProxy_RunWorkerCompleted;
            }
        }


        public void CheckAllAccounts(int delay)
        {
            int pos = 0;
            var proxy = _proxy.InstaProxies;
            foreach (var obj in _objectsInstaLogs)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += CheckInsta_DoWork;

                try
                {
                    object[] options = new object[] { _objectsInstaProx[pos], obj, delay };
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
            int delay = (int)arg[2];

            if (proxyOptions[0] == null || instOptions[0] == null)
                return;
            int proxyCount = (int)Math.Floor((decimal)instOptions.Count / proxyOptions.Count);
            if (proxyCount == 0)
                proxyCount = 1;

            int proxyPos = 0;
            bool check = true;
            int checkPos = 0;
            for (int i = 0; i < instOptions.Count; i++)
            {
                HttpAndroid android = new HttpAndroid();
                IResult<InstaLoginResult> result = null;
                try
                {
                    result = await android.Login(delay, instOptions[i]["instaLogin"], instOptions[i]["instaPassword"],
                        proxyOptions[proxyPos]["ip"].ToString(), Int32.Parse(proxyOptions[proxyPos]["port"].ToString()), proxyOptions[proxyPos]["proxyLogin"].ToString(), proxyOptions[proxyPos]["proxyPassword"].ToString());
                }
                catch
                {
                    try
                    {
                        result = await android.Login(delay, instOptions[i]["instaLogin"], instOptions[i]["instaPassword"],
                            proxyOptions[proxyPos]["ip"].ToString(), Int32.Parse(proxyOptions[proxyPos]["port"].ToString()));
                    }
                    catch (Exception ex)
                    {
                        if (check)
                        {
                            for (int z = 0; z < 60; z++)
                            {
                                try
                                {
                                    proxyOptions.Add(_proxy.InstaProxies[z]);
                                    _proxy.InstaProxies.Remove(_proxy.InstaProxies[z]);
                                }
                                catch
                                {
                                    NeedMoreProxy = true;
                                }
                            }
                            logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"{ex.Message}", Method = "Model.CheckInsta" });
                            i--;
                            check = false;
                        }
                        else
                            check = true;
                        continue;
                    }
                }
                if (result.Value.ToString() == "Success")
                {
                    if (result.Succeeded == true)
                    {
                        lock (locker)
                        {
                            AccountInfoDataSet_Success.Add(android.UserSession.UserName + ":" + android.UserSession.Password + ":" + "empty" + ":" + "empty");
                        }
                    }
                    if (result.Info.Message == "Please wait a few minutes before you try again.")
                    {
                        logging.Invoke("EasyLog.log", new Log() { UserName = $"{android.UserSession.UserName}:{android.UserSession.Password}", Date = DateTime.Now, LogMessage = $"Success! Аккаунт успешно залогинен", Method = "Model.CheckInsta" });
                        Thread.Sleep(30000);
                        i--;
                    }
                    if (result.Info.Message == "To secure your account, we've reset your password. Tap \"Get help signing in\" on the login screen and follow the instructions to access your account.")
                    {
                        Random rand = new Random();
                        if (Randomer.Next(1, 3) == 1)
                            AccountInfoDataSet_Success.Add(android.UserSession.UserName + ":" + android.UserSession.Password + ":" + "empty" + ":" + "empty");
                    }
                    if (result.Info.Message == "Your account has been disabled for violating our terms. Learn how you may be able to restore your account.")
                    {
                        logging.Invoke("EasyLog.log", new Log() { UserName = $"{android.UserSession.UserName}:{android.UserSession.Password}", Date = DateTime.Now, LogMessage = $"Block! Аккаунт заблокирован в следствие преувелечения полномочиями сервиса", Method = "Model.CheckInsta" });
                        AccsBlocked++;
                    }
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Success! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                }
                else if (result.Value.ToString() == "ChallengeRequired")
                {
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Challenge required! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                    //var res = await android.Verify_Login();
                    lock (locker)
                    {
                        AccountInfoDataSet_Required.Add(android.UserSession.UserName + ":" + android.UserSession.Password);
                    }
                }
                else if (result.Info.Message == "Произошла ошибка при отправке запроса." || result.Info.Message == "An error occurred while sending the request.")
                {
                    if (result.Info.Exception.InnerException.Message.Contains("403") || result.Info.Exception.InnerException.Message.Contains("503"))
                    {
                        ProxyBlocked++;
                        logging.Invoke("EasyLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Sentry block! Смена прокси...", Method = "Model.CheckInsta" });
                    }
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Ошибка при отправке запроса! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                    proxyPos++;
                    ProxySwitched++;
                    checkPos = 0;
                    i--;
                }
                else
                {
                    logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"{result.Value}! {result.Info.Message} - {result.Succeeded}", Method = "Model.CheckInsta" });
                }
                checkPos++;
                AccsSwitched++;

                if (checkPos % 10 == 0)
                {
                    ProxySwitched++;
                    proxyPos++;
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
            _proxy.ResolveProxy(_accMails.CountMails);

            var usrs = _account.Users;
            var instProxy = _proxy.InstaProxies;
            int count = (int)((object[])e.Argument)[2];

            _objectsInstaLogs = new List<object>();
            _objectsInstaProx = new List<object>();

            int initMaxPos = (int)((object[])e.Argument)[3];
            int pos = 0;
            int maxPos = initMaxPos;
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
                pos += initMaxPos;
                maxPos += initMaxPos;
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

            _proxy.ClearInsta();
        }


        private void UpdateProxy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            NeedMoreProxy = false;
        }
        private void UpdateProxy_DoWork(object sender, DoWorkEventArgs e)
        {
            string key = (string)e.Argument;
            _proxy.GetProxy(key);
            _proxy.ResolveProxy(_accMails.CountMails);
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
