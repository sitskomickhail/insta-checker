using System;
using System.IO;
using System.Threading;
using AE.Net.Mail;
using HtmlAgilityPack;

namespace Instagram_Checker.BLL
{
    public class Mail
    {
        ImapClient ic;
        Random random;
        int r;
        public Mail(string login, string password)
        {
            random = new Random();
            r = Randomer.Next(0, 1000000);
            ic = new ImapClient("imap.mail.ru", login, password, AuthMethods.Login, 993, true, true);

        }

        public string GetMailPath(DateTime dt)
        {

            try
            {
                var res = ic.SelectMailbox("INBOX");
            }
            catch
            {
                return null;
            }
            AE.Net.Mail.MailMessage[] mm = ic.GetMessages(ic.GetMessageCount() - 1, ic.GetMessageCount());

            if (mm[mm.Length - 1].Date > dt)
            {
                MailMessage message = ic.GetMessage(mm[mm.Length - 1].Uid);
                
                string path = $"message{r}.html";
                FileStream filestream = new FileStream(path, FileMode.Create);
                filestream.Close();
                StreamWriter file = new StreamWriter(path);
                file.Write(message.Body);
                file.Close();

                Thread.Sleep(5000);
                HtmlWeb web = new HtmlWeb();

                HtmlDocument doc = web.Load(Environment.CurrentDirectory + @"\" + path);

                var nodes = doc.DocumentNode.SelectNodes("//a");
                string result = nodes[0].GetAttributeValue("href", null);

                File.Delete(path);

                return result;
            }
            else
            {
                Thread.Sleep(5000);
                return GetMailPath(dt);
            }
        }
    }
}
