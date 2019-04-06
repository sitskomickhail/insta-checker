using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLibrary
{
    public class FileWorker
    {
        private const string _goodValid = @"\Result\Good_valid.txt";
        private const string _badValid = @"\Result\Bad_valid.txt";
        private const string _checkpoint = @"\Result\Checkpoint.txt";
        private const string _addphone = @"\Result\Add_phone.txt";
        private const string _badmail = @"\Result\Bad_mail.txt";

        public object[] locker = new object[1];

        public FileWorker()
        {
            if (Directory.Exists("Result"))
            {
                if (!File.Exists(Environment.CurrentDirectory + _goodValid))
                    File.Create(Environment.CurrentDirectory + _goodValid);
                if (!File.Exists(Environment.CurrentDirectory + _goodValid))
                    File.Create(Environment.CurrentDirectory + _badValid);
                if (!File.Exists(Environment.CurrentDirectory + _checkpoint))
                    File.Create(Environment.CurrentDirectory + _checkpoint);
                if (!File.Exists(Environment.CurrentDirectory + _addphone))
                    File.Create(Environment.CurrentDirectory + _addphone);
                if (!File.Exists(Environment.CurrentDirectory + _badmail))
                    File.Create(Environment.CurrentDirectory + _badmail);
            }
            else
            {
                Directory.CreateDirectory("Result");
                File.Create(Environment.CurrentDirectory + _goodValid);
                File.Create(Environment.CurrentDirectory + _badValid);
                File.Create(Environment.CurrentDirectory + _checkpoint);
                File.Create(Environment.CurrentDirectory + _addphone);
                File.Create(Environment.CurrentDirectory + _badmail);
            }
        }

        public void GoodValid(string account)
        {
            lock (locker) { File.AppendAllText(Environment.CurrentDirectory + "\\" + _goodValid, account + "\n"); }
        }

        public void BadValid(string account)
        {
            lock (locker) { File.AppendAllText(Environment.CurrentDirectory + "\\" + _badValid, account + "\n"); }
        }

        public void Checkpoint(string account)
        {
            lock (locker) { File.AppendAllText(Environment.CurrentDirectory + "\\" + _checkpoint, account + "\n"); }
        }

        public void AddPhone(string account)
        {
            lock (locker) { File.AppendAllText(Environment.CurrentDirectory + "\\" + _addphone, account + "\n"); }
        }

        public void BadMail(string account)
        {
            lock (locker) { File.AppendAllText(Environment.CurrentDirectory + "\\" + _badmail, account + "\n"); }
        }
    }
}