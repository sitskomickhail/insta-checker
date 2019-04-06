using InstaLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileLibrary
{
    public class AccountsMail
    {
        private List<Dictionary<string, string>> _mails;
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
            _mailAndHisFile = new List<Dictionary<string, string>>();
        }

        public void GetMailsFromBaseFile()
        {
            if (Directory.Exists(Environment.CurrentDirectory + path))
            {
                var result = Directory.GetFiles(Environment.CurrentDirectory + path, "*.txt", SearchOption.AllDirectories);

                List<Thread> threads = new List<Thread>();
                _filesCount = result.Count();
                for (int i = 0; i < _filesCount; i++)
                {
                    ThreadPool.QueueUserWorkItem(SetMail, result[i]);
                }
            }
        }

        public void DeleteMailFromFile()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += MailDeleter_DoWork;
            worker.RunWorkerAsync();
        }

        private void MailDeleter_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> mails;
            lock (locker)
            {
                mails = MailsForDeleting;
                MailsForDeleting.Clear();
            }

            foreach (var account in mails)
            {
                for (int i = 0; i < _mailAndHisFile.Count; i++)
                {
                    if (_mailAndHisFile[i]["mail"] == account)
                    {
                        string path = _mailAndHisFile[i]["fileName"];

                        var file = new List<string>(System.IO.File.ReadAllLines(path));
                        file.Remove(account);
                        File.WriteAllLines(path, file.ToArray());
                    }
                }
            }
        }

        private void SetMail(object state)
        {
            string filePath = (string)state;
            string[] str = File.ReadAllLines(filePath);

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

            string[] fileName = filePath.Split('\\');
            logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"file {fileName[fileName.Count() - 1]} returned {str.Count()} mails", Method = "AccMails.SetMails" });
            
            _currentPosition++;
            if (_filesCount == _currentPosition)
                MailsReady = true;
        }
    }
}
