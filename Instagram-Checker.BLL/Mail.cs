using System;
using System.IO;
using System.Threading;
using AE.Net.Mail;
using HtmlAgilityPack;
using InstaLog;

namespace Instagram_Checker.BLL
{
    public class Mail
    {
        ImapClient ic;
        Random random;
        int r;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);
        private static bool tryAgainResult = false;
        private string UserPass { get; set; }

        public Mail(string login, string password)
        {
            UserPass = $"{login}:{password}";
            random = new Random();
            r = Randomer.Next(0, 1000000);
            ic = new ImapClient("imap.mail.ru", login, password, AuthMethods.Login, 993, true, true);
            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = $"{login}:{password}", Date = DateTime.Now, LogMessage = $"{ic.IsAuthenticated} + {ic.IsConnected}", Method = "Mail.Ctor" });
            Thread.Sleep(1000);
        }

        public string GetMailPath(DateTime dt)
        {
            lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log()
            {
                UserName = null,
                Date = DateTime.Now,
                LogMessage = $"Entering into method {UserPass}",
                Method = "Mail.GetMailPath"
            });

            int length = 1;
            for (int i = 0; i < 10; i++)
            {
                MailMessage[] mm = ic.GetMessages(ic.GetMessageCount() - 1, ic.GetMessageCount());
                if (mm.Length == 0)
                {
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = $"Waiting for message {UserPass}", Method = "Mail.GetMailPath" });
                    lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { Date = DateTime.Now, LogMessage = $"Waiting for message {UserPass}", Method = "Mail.GetMailPath" });
                    Thread.Sleep(5000);
                    return GetMailPath(dt);
                }

                if (mm[mm.Length - length].Date >= dt)
                {
                    MailMessage message = ic.GetMessage(mm[mm.Length - 1].Uid);
                    Thread.Sleep(1000);
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, LogMessage = $"Getting messages {UserPass}", Method = "Mail.GetMailPath" });
                    lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { Date = DateTime.Now, LogMessage = $"Getting messages {UserPass}", Method = "Mail.GetMailPath" });

                    string path = $"message{r}.html";
                    FileStream filestream = new FileStream(path, FileMode.Create);
                    filestream.Close();
                    StreamWriter file = new StreamWriter(path);
                    file.Write(message.Body);
                    file.Close();
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"File Created {UserPass}", Method = "Mail.GetMailPath" });
                    lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"File Created {UserPass}", Method = "Mail.GetMailPath" });

                    Thread.Sleep(5000);
                    HtmlWeb web = new HtmlWeb();

                    HtmlDocument doc = web.Load(Environment.CurrentDirectory + @"\" + path);
                    lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Getting info from file {UserPass}", Method = "Mail.GetMailPath" });
                    var nodes = doc.DocumentNode.SelectNodes("//a");
                    string result = nodes[0].GetAttributeValue("href", null);
                    Thread.Sleep(1000);
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"\"href\" finded = {result} ---- {UserPass}", Method = "Mail.GetMailPath" });
                    lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"\"href\" finded = {result} ---- {UserPass}", Method = "Mail.GetMailPath" });

                    File.Delete(path);
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"FileDeleted {UserPass}", Method = "Mail.GetMailPath" });
                    lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"FileDeleted {UserPass}", Method = "Mail.GetMailPath" });
                    Thread.Sleep(1000);

                    if (result.Contains("https://instagram.com/accounts/confirm_email/"))
                    {
                        lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Returning result {UserPass}", Method = "Mail.GetMailPath" });
                        lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Returning result {UserPass}", Method = "Mail.GetMailPath" });

                        return result;
                    }
                    else
                    {
                        length++;
                        lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"{length - 1} message not correct {UserPass}", Method = "Mail.GetMailPath" });
                        lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"{length - 1} message not correct", Method = "Mail.GetMailPath" });
                        continue;
                    }
                }
                else
                {
                    Thread.Sleep(2000);
                }
            }
            if (tryAgainResult == false)
            {
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Try to  find again {UserPass}", Method = "Mail.GetMailPath" });
                lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Try to  find again {UserPass}", Method = "Mail.GetMailPath" });
                tryAgainResult = true;
                return GetMailPath(dt);
            }
            else
            {
                tryAgainResult = false;
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Mail not found", Method = "Mail.GetMailPath" });
                lock (LogIO.locker) logging.Invoke("MailLog.log", new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Mail not found", Method = "Mail.GetMailPath" }); tryAgainResult = false;
                return null;
            }
        }
    }
}
