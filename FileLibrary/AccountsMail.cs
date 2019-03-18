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
    public class AccountsMail
    {
        private List<Dictionary<string, string>> _mails;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);
        private const string path = @"\base\Mails\";
        private int _filesCount;
        private int _currentPosition;

        public bool MailsReady { get; private set; }

        public int CountMails { get { return _mails.Count(); } }

        public List<Dictionary<string, string>> Mails { get { return _mails; } }

        public AccountsMail()
        {
            MailsReady = false;
            _mails = new List<Dictionary<string, string>>();
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
            logging.Invoke(LogIO.path, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"file {fileName[fileName.Count() - 1]} returned {str.Count()} mails", Method = "AccMails.SetMails" });
        }
    }
}
