using System.IO;

namespace InstaLog
{
    public static class LogIO
    {
        public static string mainLog = "Log.log";
        public static string easyPath = "EasyLog.log";
        public delegate void Logging(string text, Log log);
        public static void WriteLog(string path, Log log)
        {
            try
            {
                File.AppendAllText(path, log.ToString() + "\n");
            }
            catch { }
        }
    }
}
