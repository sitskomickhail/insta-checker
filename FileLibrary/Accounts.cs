using InstaLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileLibrary
{
    public class Accounts
    {
        private List<Dictionary<string, string>> _users;
        private List<string> _paths;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);
        private const string path = @"\base\InstaLogins\";
        private int _filesCount;
        private int _currentPosition;

        private int _checkDelete;

        public object[] locker = new object[1];

        #region PROPS
        public bool UsersReady { get; private set; }
        public int CountUsers { get { return _users.Count(); } }
        public List<Dictionary<string, string>> Users { get { lock (locker) { return _users; } } }

        public List<string> UsersForDeleting { get; private set; }
        private List<Dictionary<string, string>> _userAndHisFile;
        #endregion

        public Accounts()
        {
            UsersReady = false;
            UsersForDeleting = new List<string>();
            _userAndHisFile = new List<Dictionary<string, string>>();
            _users = new List<Dictionary<string, string>>();
            _paths = new List<string>();
        }



        public void GetAccountsFromBaseFile()
        {
            if (Directory.Exists(Environment.CurrentDirectory + path))
            {
                var result = Directory.GetFiles(Environment.CurrentDirectory + path, "*.txt", SearchOption.AllDirectories);

                List<Thread> threads = new List<Thread>();
                _filesCount = result.Count();
                for (int i = 0; i < _filesCount; i++)
                {
                    ThreadPool.QueueUserWorkItem(SetUser, result[i]);
                }
            }
        }


        public void DeleteAccountsFromFile()
        {
            foreach (var path in _paths)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Delete_DoWork;
                worker.RunWorkerAsync(path);
            }

        }

        private void Delete_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = (string)e.Argument;

            List<string> allUsersFromFile = File.ReadAllLines(path).ToList();
            bool check = false;
            lock (locker)
            {
                foreach (string user in UsersForDeleting)
                {
                    check = true;
                    allUsersFromFile.Remove(user);
                }
            }

            _checkDelete++;

            if (UsersForDeleting.Count >= 8000 && _checkDelete == _paths.Count)
                lock (locker)
                {
                    UsersForDeleting.Clear();
                    _checkDelete = 0;
                }

            if (check)
                File.WriteAllLines(path, allUsersFromFile);
        }

        private void SetUser(object state)
        {
            string filePath = (string)state;
            string[] str = File.ReadAllLines(filePath);
            _paths.Add(filePath);

            foreach (string user in str)
            {
                if (String.IsNullOrEmpty(user))
                    continue;

                Dictionary<string, string> saveUser = new Dictionary<string, string>();
                saveUser["fileName"] = filePath;
                saveUser["user"] = user;
                _userAndHisFile.Add(saveUser);
                try
                {
                    string[] splitted = user.Split(':');
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("instaLogin", splitted[0]);
                    dict.Add("instaPassword", splitted[1]);
                    if (!(String.IsNullOrEmpty(splitted[0]) && String.IsNullOrEmpty(splitted[1])))
                        _users.Add(dict);
                }
                catch { }
            }
            _currentPosition++;
            if (_filesCount == _currentPosition)
                UsersReady = true;

            string[] fileName = filePath.Split('\\');
            logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"file {fileName[fileName.Count() - 1]} returned {str.Count()} accounts", Method = "Account.SetUser" });
        }
    }
}
