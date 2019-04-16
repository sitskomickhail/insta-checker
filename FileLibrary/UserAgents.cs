using InstaLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileLibrary
{
    public class UserAgents
    {
        private List<string> _agents;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);
        private const string path = @"\base\UserAgents\";
        private int _filesCount;
        private int _currentPosition;

        public object[] locker = new object[1];

        public bool AgentsReady { get; private set; }

        public int CountAgents { get { return _agents.Count(); } }

        public List<string> Agents { get { lock (locker) { return _agents; } } }

        public UserAgents()
        {
            AgentsReady = false;
            _agents = new List<string>();
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
                    ThreadPool.QueueUserWorkItem(SetAgent, result[i]);
                }
            }
        }

        private void SetAgent(object state)
        {
            string filePath = (string)state;
            string[] str = File.ReadAllLines(filePath);

            foreach (string agent in str)
            {
                try
                {
                    _agents.Add(agent);
                }
                catch { }
            }
            _currentPosition++;
            if (_filesCount == _currentPosition)
                AgentsReady = true;

            string[] fileName = filePath.Split('\\');
            lock(LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"file {fileName[fileName.Count() - 1]} returned {str.Count()} Agents", Method = "UserAgent.SetUser" });
        }


    }
}
