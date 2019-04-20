using InstaLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileLibrary
{
    public class AccountsMail
    {
        private List<Dictionary<string, string>> _mails;
        private List<string> _paths;

        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);
        private const string path = @"\base\Mails\";
        private int _filesCount;
        private int _currentPosition;

        public object[] locker = new object[1];


        public bool MailsReady { get; private set; }
        public int CountMails { get { return _mails.Count(); } }
        public List<Dictionary<string, string>> Mails { get { return _mails; } }

        public List<string> MailsForDeleting { get; private set; }
        private List<Dictionary<string, string>> _mailAndHisFile;


        public AccountsMail()
        {
            MailsReady = false;
            _mails = new List<Dictionary<string, string>>();
            MailsForDeleting = new List<string>();
            _paths = new List<string>();
            _mailAndHisFile = new List<Dictionary<string, string>>();
        }

        public void GetMailsFromBaseFile()
        {
            if (Directory.Exists(Environment.CurrentDirectory + path))
            {
                var result = Directory.GetFiles(Environment.CurrentDirectory + path, "*.txt", SearchOption.AllDirectories).ToList();

                List<Thread> threads = new List<Thread>();
                _filesCount = result.Count();

                foreach (string path in result)
                {
                    Task.Run(() => SetMail_Run(path));
                }
            }
        }

        private void SetMail_Run(string filePath)
        {
            string[] str = File.ReadAllLines(filePath);

            _paths.Add(filePath);
            foreach (string user in str)
            {
                Dictionary<string, string> saveUser = new Dictionary<string, string>();
                saveUser["fileName"] = filePath;
                saveUser["mail"] = user;
                _mailAndHisFile.Add(saveUser);
                try
                {
                    string[] splitted = user.Split(':');
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("mailLogin", splitted[0]);
                    dict.Add("mailPassword", splitted[1]);

                    _mails.Add(dict);
                }
                catch { }
            }

            _currentPosition++;
            if (_filesCount == _currentPosition)
                MailsReady = true;

            string[] fileName = filePath.Split('\\');
            lock(LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"file {fileName[fileName.Count() - 1]} returned {str.Count()} mails", Method = "AccMails.SetMails" });
            _currentPosition++;
        }

        public void DeleteMailFromFile()
        {
            logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = $"Start delete mails. Count paths = {_paths.Count}", Method = "DeleteMailFromFile" });
            foreach (var path in _paths)
            {
                Task.Run(() => MailDeleter_Run(path));
            }
        }


        private void MailDeleter_Run(string path)
        {
            List<string> allUsersFromFile;
            lock(locker) allUsersFromFile = File.ReadAllLines(path).ToList();
            bool check = false;
            lock (locker)
            {
                foreach (string user in MailsForDeleting)
                {
                    check = true;
                    allUsersFromFile.Remove(user);
                }
            }
            if (check)
                lock(locker) File.WriteAllLines(path, allUsersFromFile);
        }
    }
}