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
        private const string _badValid = @"\Result\Bad_pass.txt";
        private const string _checkpoint = @"\Result\Checkpoint.txt";
        private const string _addphone = @"\Result\Add_phone.txt";
        private const string _badmail = @"\Result\Bad_mail.txt";

        public FileWorker()
        {
            if (Directory.Exists("Result"))
            {
                if (!File.Exists(Environment.CurrentDirectory + _goodValid))
                    File.Create(Environment.CurrentDirectory + _goodValid);
                if (!File.Exists(Environment.CurrentDirectory + _goodValid))
                    File.Create(Environment.CurrentDirectory + _badValid);
                if (!File.Exists(Environment.CurrentDirectory + _badValid))
                    File.Create(Environment.CurrentDirectory + _badValid);
                if (!File.Exists(Environment.CurrentDirectory + _checkpoint))
                    File.Create(Environment.CurrentDirectory + _checkpoint);
                if (!File.Exists(Environment.CurrentDirectory + _addphone))
                    File.Create(Environment.CurrentDirectory + _addphone);
                if (!File.Exists(Environment.CurrentDirectory + _badmail))
                    File.Create(Environment.CurrentDirectory + _badmail);
            }
            else
                Directory.CreateDirectory("Result");
        }

        public void GoodValid(string account)
        {

        }
    }
}
