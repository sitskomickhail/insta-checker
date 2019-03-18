using InstaLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileLibrary
{
    public class Accounts
    {
        private List<Dictionary<string, string>> _users;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);
        private const string path = @"\base\InstaLogins\";
        private int _filesCount;
        private int _currentPosition;

        public bool UsersReady { get; private set; }

        public int CountUsers { get { return _users.Count(); } }

        public List<Dictionary<string, string>> Users { get { return _users; } }

        public Accounts()
        {
            UsersReady = false;
            _users = new List<Dictionary<string, string>>();
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

        private void SetUser(object state)
        {
            string filePath = (string)state;
            string[] str = File.ReadAllLines(filePath);

            foreach (string user in str)
            {
                try
                {
                    string[] splitted = user.Split(':');
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("instaLogin", splitted[0]);
                    dict.Add("instaPassword", splitted[1]);

                    _users.Add(dict);
                }
                catch { }
            }
            _currentPosition++;
            if (_filesCount == _currentPosition)
                UsersReady = true;

            string[] fileName = filePath.Split('\\');
            logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"file {fileName[fileName.Count() - 1]} returned {str.Count()} accounts", Method = "Account.SetUser" });
        }
    }
}
