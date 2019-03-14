using FileLibrary;
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
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        public bool IsProxyInited { get; private set; }
        public bool IsAccountInited { get; private set; }

        public Model()
        {
            IsProxyInited = false;
            IsAccountInited = false;
            _proxy = new Proxy();
            _account = new Accounts();
        }

        public void InitAccounts()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(Worker_InitAccounts);
            worker.RunWorkerCompleted += Worker_InitAccountsCompleted;
            worker.RunWorkerAsync();
        }


        public void InitProxy(bool isApiNeed)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(Worker_InitProxy);
            worker.RunWorkerCompleted += Worker_InitProxyCompleted;
            worker.RunWorkerAsync(isApiNeed);
        }


        #region BackgroundMethods
        private void Worker_InitProxyCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy inited! {_proxy.CountProxy} proxies are ready", Method = "Model.InitProxy" });
            IsProxyInited = true;
        }

        private void Worker_InitProxy(object sender, DoWorkEventArgs e)
        {
            bool isApiNeed = (bool)e.Argument;
            if (isApiNeed)
                _proxy.GetProxy();
            _proxy.GetAllInfoFromFile();
            _proxy.InstaProxy_Init();
            _proxy.MailProxy_Init();
        }

        private void Worker_InitAccounts(object sender, DoWorkEventArgs e)
        {
            _account.GetAccountsFromBaseFile();
            while(true)
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
