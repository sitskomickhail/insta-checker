using FileLibrary;
using InstagramLibrary;
using InstaLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Checker.BLL
{
    public class Model
    {
        private Proxy _proxy;
        private Accounts _account;
        private HttpAndroid _android;
        private AccountsMail _accMails;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        private List<object> _objects;

        public bool IsProxyInited { get; private set; }
        public bool IsAccountInited { get; private set; }
        public bool IsObjectsReady { get; private set; }
        public bool IsMailsReady { get; private set; }

        public Proxy GetProxy { get { return _proxy; } }
        public Accounts GetAccounts { get { return _account; } }
        public AccountsMail GetAccountsMail { get { return _accMails; } }

        public Model()
        {
            IsProxyInited = false;
            IsAccountInited = false;
            IsObjectsReady = false;
            IsMailsReady = false;

            _proxy = new Proxy();
            _account = new Accounts();
            _android = new HttpAndroid();
            _accMails = new AccountsMail();
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
            var proxy = _proxy.InstaProxies;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += CheckInsta_DoWork;
            string[] userOptions = new string[6] { "vi.tik", "кактусик", proxy[0]["ip"].ToString(), proxy[0]["port"].ToString(),
                proxy[0]["login"].ToString(), proxy[0]["password"].ToString() };

            worker.RunWorkerAsync(userOptions);
        }

        #region BackgroundMethods

        private async void CheckInsta_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] arg = (object[])e.Argument;
            List<Dictionary<string, string>> proxyOptions = (List<Dictionary<string, string>>)arg[0];
            List<Dictionary<string, string>> instOptions = (List<Dictionary<string, string>>)arg[1];

            int proxyPos = (int)Math.Floor((decimal)instOptions.Count / proxyOptions.Count);
            if (proxyPos == 0)
                proxyPos = 1;

            for (int i = 0; i < instOptions.Count; i++)
            {
                HttpAndroid android = new HttpAndroid();
                var result = await android.Login(instOptions[0]["instaLogin"], instOptions[1]["instaPassword"],
                    proxyOptions[0]["ip"], Int32.Parse(proxyOptions[proxyPos]["port"]), proxyOptions[2]["proxyLogin"], proxyOptions[3]["proxyPassword"]);
                if (result.Info.Message == "Success")
                {

                }
                else if (result.Info.Message == "Challenge is required")
                {

                }
                else
                {

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
            int count = usrs.Count / 10000;
            _objects = new List<object>();
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
                if (check)
                    break;
                pos += 10000;
                maxPos += 10000;
                _objects.Add(forObj);
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
