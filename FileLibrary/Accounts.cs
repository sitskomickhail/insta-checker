using InstaLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FileLibrary
{
    public class Accounts
    {
        private List<Dictionary<string, string>> _users;
        private List<string> _paths;
        private List<string> _all_paths;
        private const string path = "\\base\\InstaLogins\\";
        private int _filesCount;
        private int _currentPosition;
        private int _checkDelete;
        private LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        public object[] locker = new object[1];
        public bool AllPathChecked { get; set; }

        public int CountUsers { get { return _users.Count(); } }

        public List<Dictionary<string, string>> Users { get { return _users; } }

        public List<string> UsersForDeleting
        {
            get;
            private set;
        }

        public bool UsersReady { get; private set; }

        public Accounts()
        {
            UsersReady = false;
            AllPathChecked = false;
            UsersForDeleting = new List<string>();
            _users = new List<Dictionary<string, string>>();
            _paths = new List<string>();
            _all_paths = new List<string>();
        }

        private void Delete_Run(string path)
        {
            List<string> allUsersFromFile;
            lock (locker)
            {
                allUsersFromFile = File.ReadAllLines(path).ToList();
            }

            int usersCount = allUsersFromFile.Count;
            logging.Invoke(LogIO.mainLog, new Log() { Method = "Account.Delete_Run", Date = DateTime.Now, LogMessage = $"file returned - {usersCount}", UserName = path });
            bool check = false;
            lock (locker)
            {
                foreach (string user in UsersForDeleting)
                {
                    check = true;
                    allUsersFromFile.Remove(user);
                }

                logging.Invoke(LogIO.mainLog, new Log() { Method = "Account.Delete_Run", Date = DateTime.Now, LogMessage = $"Users removed - {usersCount - allUsersFromFile.Count}", UserName = path });

                _checkDelete++;
                if (_checkDelete == _all_paths.Count)
                {
                    logging.Invoke(LogIO.mainLog, new Log() { Method = "Account.Delete_Run", Date = DateTime.Now, LogMessage = "Deleting users from UsersForDeleting", UserName = path });

                    foreach (string user in UsersForDeleting)
                    {
                        string[] delUser = user.Split(new char[] { ':' });
                        Dictionary<string, string> tempDict = new Dictionary<string, string>()
                        {
                            { "instaLogin", delUser[0] },
                            { "instaPassword", delUser[1] }
                        };
                        for (int i = 0; i < _users.Count; i++)
                        {
                            if (_users[i]["instaLogin"] == tempDict["instaLogin"] && _users[i]["instaPassword"] == tempDict["instaPassword"])
                            {
                                _users.Remove(_users[i]);

                            }
                        }
                    }
                    UsersForDeleting.Clear();
                }
                logging.Invoke(LogIO.mainLog, new Log() { Method = "Account.Delete_Run", Date = DateTime.Now, LogMessage = "UsersForDeleting cleared", UserName = path });
            }
            if (check)
            {
                logging.Invoke(LogIO.mainLog, new Log() { Method = "Account.Delete_Run", Date = DateTime.Now, LogMessage = "UsersDeleted", UserName = path });
                lock (locker) File.WriteAllLines(path, allUsersFromFile);
            }
            if (allUsersFromFile.Count < 10)
            {
                lock (locker) File.Delete(path);
                lock (locker) _all_paths.Remove(path);
                if (_all_paths.Count == 0)
                {
                    logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, Method = "Account.Delete_Run", LogMessage = "Set User, as _all_paths = 0" });
                    SetUser();
                }
                logging.Invoke(LogIO.mainLog, new Log() { Method = "Account.Delete_Run", Date = DateTime.Now, LogMessage = "File removed", UserName = path });
            }
        }

        public void DeleteAccountsFromFile()
        {
            logging.Invoke(LogIO.mainLog, new Log() { LogMessage = $"Start delete accounts. Count paths = {_all_paths.Count}", Date = DateTime.Now, Method = "Account.DeleteAccountsFromFile" });
            _checkDelete = 0;
            foreach (string _allPath in _all_paths)
            {
                Task.Run(() => Delete_Run(_allPath));
            }
        }

        public void GetAccountsFromBaseFile()
        {
            if (Directory.Exists(string.Concat(Environment.CurrentDirectory, @"\base\InstaLogins\")))
            {
                _paths = Directory.GetFiles(string.Concat(Environment.CurrentDirectory, @"\base\InstaLogins\"), "*.txt", SearchOption.AllDirectories).ToList();
                _filesCount = _paths.Count();
                logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = $"Getting files. filesCount = {_paths.Count}", Method = "Account.GetAccountsFromBaseFile" });
                SetUser();
                UsersReady = true;
            }
        }

        public void SetUser()
        {
            if (!AllPathChecked)
            {
                string filePath = _paths[0];
                string[] str = File.ReadAllLines(filePath);
                _all_paths.Add(filePath);
                string[] strArrays = str;
                for (int i = 0; i < strArrays.Length; i++)
                {
                    string user = strArrays[i];
                    if (!string.IsNullOrEmpty(user))
                    {
                        Dictionary<string, string> saveUser = new Dictionary<string, string>();
                        saveUser["fileName"] = filePath;
                        saveUser["user"] = user;
                        try
                        {
                            string[] splitted = user.Split(new char[] { ':' });
                            Dictionary<string, string> dict = new Dictionary<string, string>()
                            {
                                { "instaLogin", splitted[0] },
                                { "instaPassword", splitted[1] }
                            };
                            if ((!string.IsNullOrEmpty(splitted[0]) ? true : !string.IsNullOrEmpty(splitted[1])))
                            {
                                lock (locker)
                                {
                                    _users.Add(dict);
                                }
                            }
                        }
                        catch { }
                    }
                }
                _currentPosition++;
                _paths.Remove(_paths[0]);
                if (_filesCount == _currentPosition)
                {
                    AllPathChecked = true;
                }
                string[] fileName = filePath.Split(new char[] { '\\' });
                lock (LogIO.locker)
                {
                    logging(LogIO.mainLog, new Log()
                    {
                        Date = DateTime.Now,
                        LogMessage = string.Format("File {0} returned {1} accounts", fileName[fileName.Count() - 1], str.Count()),
                        Method = "Account.SetUser"
                    });
                }
            }
            else
                logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = $"All path checked... filesCount = {_filesCount} currentPosition = {_currentPosition}", Method = "Account.SetUser" });
        }
    }
}