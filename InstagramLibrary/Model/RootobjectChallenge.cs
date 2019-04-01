namespace InstagramLibrary.Model
{
    public class RootobjectChallenge
    {
        public string message { get; set; }
        public Challenge challenge { get; set; }
        public string status { get; set; }
        public string error_type { get; set; }
    }

    public class Challenge
    {
        public string url { get; set; }
        public string api_path { get; set; }
        public bool hide_webview_header { get; set; }
        public bool _lock { get; set; }
        public bool logout { get; set; }
        public bool native_flow { get; set; }
    }
}