﻿namespace InstagramLibrary.Model
{
    public class LoginResult
    {
        public string status { get; set; }
        public bool reactivated { get; set; }
        public bool authenticated { get; set; }
        public string user { get; set; }
    }
}
