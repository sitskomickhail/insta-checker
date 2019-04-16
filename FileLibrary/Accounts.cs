using InstaLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileLibrary
{
    public class Accounts
    {
        private List<Dictionary<string, string>> _users;
        private List<string> _paths;
        private List<string> _all_paths;
        private const string path = @"\base\InstaLogins\";
        private int _filesCount;
        private int _currentPosition;
        private int _checkDelete;

        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        public object[] locker = new object[1];

        #region PROPS
        public bool UsersReady { get; private set; }
        public bool AllPathChecked { get; set; }
        public int CountUsers { get { return _users.Count(); } }
        public List<Dictionary<string, string>> Users { get { lock (locker) { return _users; } } }

        public List<string> UsersForDeleting { get; private set; }
        #endregion

        public Accounts()
        {
            UsersReady = false;
            AllPathChecked = false;
            UsersForDeleting = new List<string>();
            _users = new List<Dictionary<string, string>>();
            _paths = new List<string>();
            _all_paths = new List<string>();
        }


        public void GetAccountsFromBaseFile()
        {
            if (Directory.Exists(Environment.CurrentDirectory + path))
            {
                _paths = Directory.GetFiles(Environment.CurrentDirectory + path, "*.txt", SearchOption.AllDirectories).ToList();
                
                _filesCount = _paths.Count();
                SetUser();
                UsersReady = true;
            }
        }


        public void DeleteAccountsFromFile()
        {
            foreach (var path in _all_paths)
            {
                Task.Run(() => Delete_Run(path));
            }
        }

        private void Delete_Run(string path)
        {
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

            if (UsersForDeleting.Count >= 3500 && _checkDelete == _paths.Count)
            {
                lock (locker)
                {
                    foreach (var user in UsersForDeleting)
                    {
                        string[] delUser = user.Split(':');
                        Dictionary<string, string> tempDict = new Dictionary<string, string>();
                        tempDict.Add("instaLogin", delUser[0]);
                        tempDict.Add("instaPassword", delUser[1]);
                        _users.Remove(tempDict);
                    }

                    UsersForDeleting.Clear();
                }
                _checkDelete = 0;
            }

            if (check)
                File.WriteAllLines(path, allUsersFromFile);
        }

        public void SetUser()
        {
            if (AllPathChecked)
                return;
            string filePath = _paths[0];
            string[] str = File.ReadAllLines(filePath);
            _all_paths.Add(filePath);
            foreach (string user in str)
            {
                if (String.IsNullOrEmpty(user))
                    continue;

                Dictionary<string, string> saveUser = new Dictionary<string, string>();
                saveUser["fileName"] = filePath;
                saveUser["user"] = user;
                try
                {
                    string[] splitted = user.Split(':');
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("instaLogin", splitted[0]);
                    dict.Add("instaPassword", splitted[1]);
                    if (!(String.IsNullOrEmpty(splitted[0]) && String.IsNullOrEmpty(splitted[1])))
                        lock (locker)
                            _users.Add(dict);
                }
                catch { }
            }
            _currentPosition++;
            _paths.Remove(_paths[0]);
            if (_filesCount == _currentPosition)
                AllPathChecked = true;

            string[] fileName = filePath.Split('\\');
           lock(LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"file {fileName[fileName.Count() - 1]} returned {str.Count()} accounts", Method = "Account.SetUser" });
        }
    }
}