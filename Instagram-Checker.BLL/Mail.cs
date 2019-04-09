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


        public Mail(string login, string password)
        {
            random = new Random();
            r = Randomer.Next(0, 1000000);
            ic = new ImapClient("imap.mail.ru", login, password, AuthMethods.Login, 993, true, true);
            logging.Invoke("MailLog.log", new Log() { UserName = $"{login}:{password}", Date = DateTime.Now,
                LogMessage = $"{ic.IsAuthenticated} + {ic.IsConnected}", Method = "Mail.Ctor"});
            Thread.Sleep(1000);
        }

        public string GetMailPath(DateTime dt)
        {
            logging.Invoke(LogIO.mainLog, new Log()
            {
                UserName = null,
                Date = DateTime.Now,
                LogMessage = $"Entering into method",
                Method = "Mail.GetMailPath"
            });

            int length = 1;
            for (int i = 0; i < 10; i++)
            {
                MailMessage[] mm = ic.GetMessages(ic.GetMessageCount() - 1, ic.GetMessageCount());
                if (mm.Length == 0)
                {
                    Thread.Sleep(5000);
                    return GetMailPath(dt);
                }

                if (mm[mm.Length - length].Date >= dt)
                {
                    MailMessage message = ic.GetMessage(mm[mm.Length - 1].Uid);
                    Thread.Sleep(1000);
                    logging.Invoke(LogIO.easyPath, new Log()
                    {
                        UserName = null,
                        Date = DateTime.Now,
                        LogMessage = $"Getting messages",
                        Method = "Mail.GetMailPath"
                    });
                    logging.Invoke(LogIO.mainLog, new Log()
                    {
                        UserName = null,
                        Date = DateTime.Now,
                        LogMessage = $"Getting messages",
                        Method = "Mail.GetMailPath"
                    });

                    string path = $"message{r}.html";
                    FileStream filestream = new FileStream(path, FileMode.Create);
                    filestream.Close();
                    StreamWriter file = new StreamWriter(path);
                    file.Write(message.Body);
                    file.Close();
                    logging.Invoke(LogIO.mainLog, new Log()
                    {
                        UserName = null,
                        Date = DateTime.Now,
                        LogMessage = $"File Created",
                        Method = "Mail.GetMailPath"
                    });

                    Thread.Sleep(5000);
                    HtmlWeb web = new HtmlWeb();

                    HtmlDocument doc = web.Load(Environment.CurrentDirectory + @"\" + path);
                    logging.Invoke("MailLog.log", new Log()
                    {
                        UserName = null,
                        Date = DateTime.Now,
                        LogMessage = $"Getting info from file",
                        Method = "Mail.GetMailPath"
                    });
                    var nodes = doc.DocumentNode.SelectNodes("//a");
                    string result = nodes[0].GetAttributeValue("href", null);
                    Thread.Sleep(1000);
                    logging.Invoke(LogIO.mainLog, new Log()
                    {
                        UserName = null,
                        Date = DateTime.Now,
                        LogMessage = $"\"href\" finded = {result}",
                        Method = "Mail.GetMailPath"
                    });

                    File.Delete(path);
                    logging.Invoke(LogIO.mainLog, new Log()
                    {
                        UserName = null,
                        Date = DateTime.Now,
                        LogMessage = $"FileDeleted",
                        Method = "Mail.GetMailPath"
                    });
                    Thread.Sleep(1000);

                    if (result.Contains("https://instagram.com/accounts/confirm_email/"))
                    {
                        logging.Invoke(LogIO.mainLog, new Log()
                        {
                            UserName = null,
                            Date = DateTime.Now,
                            LogMessage = $"Returning result",
                            Method = "Mail.GetMailPath"
                        });
                        return result;
                    }
                    else
                    {
                        length++;
                        logging.Invoke(LogIO.mainLog, new Log()
                        {
                            UserName = null,
                            Date = DateTime.Now,
                            LogMessage = $"{length - 1} message not correct",
                            Method = "Mail.GetMailPath"
                        });
                        logging.Invoke(LogIO.easyPath, new Log()
                        {
                            UserName = null,
                            Date = DateTime.Now,
                            LogMessage = $"{length - 1} message not correct",
                            Method = "Mail.GetMailPath"
                        });
                        continue;
                    }

                }
                else
                {
                    Thread.Sleep(5000);
                    return GetMailPath(dt);
                }
            }
            return null;
        }
    }
}
