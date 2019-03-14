using System.IO;

namespace InstaLog
{
    public static class LogIO
    {
        public static string path = "Log.log";
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
