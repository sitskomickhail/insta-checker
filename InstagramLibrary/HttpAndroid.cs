using InstagramLibrary.Model;
using InstaLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InstagramLibrary
{
    public class HttpAndroid
    {
        private CookieContainer mCoockieC;

        public string Username { get; set; }
        public string Password { get; set; }

        private string _userAgent;
        private WebProxy _proxy;

        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        public HttpAndroid(string username, string password, Dictionary<string, object> proxy, string userAgent)
        {
            mCoockieC = new CookieContainer();

            Username = username;
            Password = password;
            _userAgent = userAgent;
            try
            {
                _proxy = new WebProxy(proxy["ip"].ToString(), Int32.Parse(proxy["port"].ToString()));
                if (proxy.Count > 2)
                    _proxy.Credentials = new NetworkCredential(proxy["proxyLogin"].ToString(), proxy["proxyPassword"].ToString());
            }
            catch (Exception ex)
            {
                if (_proxy == null)
                {
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log()
                    {
                        Date = DateTime.Now,
                        LogMessage = $"Hе объявлен порт или айпи адрес: {ex.Message}",
                        Method = "HttpAndroid.Ctor",
                        UserName = null
                    });
                    throw new Exception($"Hе объявлен порт или айпи адрес: {ex.Message}");
                }
            }

        }

        public async Task<LoginResult> LogIn()
        {
            Debug.WriteLine("Login");
            bool GZip = false;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var bootstrapRequest = HttpRequestBuilder.Get("https://www.instagram.com/accounts/login/", _userAgent, mCoockieC);
                    bootstrapRequest.Proxy = _proxy;
                    bootstrapRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*;q=0.8";
                    bootstrapRequest.Headers["Upgrade-Insecure-Requests"] = "1";
                    using (var bootstrapResponse = await bootstrapRequest.GetResponseAsync() as HttpWebResponse)
                    {
                        if (bootstrapResponse.Cookies.Count == 0)
                            continue;
                        mCoockieC.Add(bootstrapResponse.Cookies);
                    }
                }
                catch (Exception bex)
                {
                    Debug.WriteLine("Bootstrap progress meet exception " + bex.Message);
                    throw bex; //Status==ConnectFailure
                }
                try
                {
                    string data = $"username={Username}&password={Password}";
                    byte[] content = Encoding.UTF8.GetBytes(data);

                    HttpWebRequest request = HttpRequestBuilder.Post("https://www.instagram.com/accounts/login/ajax/", _userAgent, mCoockieC);


                    request.Referer = "https://www.instagram.com/accounts/login/";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = content.Length;
                    request.KeepAlive = false;
                    request.Headers["Accept-Encoding"] = "gzip, deflate, br";
                    request.Headers["Accept-Language"] = "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7";

                    request.Headers["Origin"] = "https://www.instagram.com";

                    request.Headers["X-CSRFToken"] = mCoockieC.GetCookies(new Uri("https://instagram.com"))["csrftoken"].Value;
                    request.Headers["X-Instagram-AJAX"] = "0cf1e9388e80"; // You may need to change this one to get rid of 404 bad request.
                    request.Headers["X-Requested-With"] = "XMLHttpRequest";

                    request.AllowAutoRedirect = true;
                    request.Proxy = _proxy;
                    // Send login data
                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        using (StreamWriter dataStreamWriter = new StreamWriter(requestStream))
                        {
                            requestStream.Write(content, 0, content.Length);
                        }
                        HttpWebResponse response = null;
                        try
                        {
                            response = await request.GetResponseAsync() as HttpWebResponse;
                        }
                        catch (WebException e)
                        {
                            response = (HttpWebResponse)e.Response;
                        }

                        string responseData;

                        //GZip convert
                        if (GZip)
                            using (Stream dataS = response.GetResponseStream())
                            using (var gzipStream = new GZipStream(dataS, CompressionMode.Decompress))
                            using (var streamReader = new StreamReader(dataS))
                                responseData = streamReader.ReadToEnd();
                        else
                        {
                            using (Stream dataS = response.GetResponseStream())
                            using (var streamReader = new StreamReader(dataS))
                                responseData = streamReader.ReadToEnd();

                            if (responseData.Contains("0"))
                            {
                                GZip = true;
                                continue;
                            }
                        }

                        mCoockieC.Add(response.Cookies);

                        if (responseData == "Request Timeout")
                            return new LoginResult() { status = responseData };

                        if (GZip)
                        {
                            try
                            {
                                LoginResult result = JsonConvert.DeserializeObject<LoginResult>(responseData);
                                if (result.authenticated == true)
                                    return new LoginResult() { status = "success" };
                                else
                                    return new LoginResult() { status = "ok" };
                            }
                            catch { return new LoginResult() { status = "success" }; }
                        }

                        try
                        {

                            RootobjectChallenge result = JsonConvert.DeserializeObject<RootobjectChallenge>(responseData);
                            if (result.message == "checkpoint_required")
                            {
                                return new LoginResult() { status = "checkpoint_required" };
                            }
                            return new LoginResult() { status = "ok" };
                        }
                        catch
                        {
                            return new LoginResult() { status = "ok" };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Login progress occur exception " + ex.Message);
                    logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Exception! {ex.Message}", Method = "HttpAndroid.LogIn" });
                    mCoockieC = new CookieContainer();

                    continue;
                }
            }
            return null;
        }

        public async Task<ProfileResult> GetProfile()
        {
            Debug.WriteLine("Get profile");
            CookieContainer reserve = mCoockieC;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var request = HttpRequestBuilder.Get("https://www.instagram.com/accounts/edit/?__a=1", _userAgent, mCoockieC);
                    request.Referer = $"https://www.instagram.com/{Username}/";
                    request.Headers["X-Requested-With"] = "XMLHttpRequest";
                    request.AllowAutoRedirect = false;
                    request.Proxy = _proxy;
                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    {
                        mCoockieC.Add(response.Cookies);
                        using (var responseStream = response.GetResponseStream())
                        using (var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        using (var streamReader = new StreamReader(gzipStream))
                        {
                            var data = streamReader.ReadToEnd();
                            return JsonConvert.DeserializeObject<ProfileResult>(data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetProfile progress occur exception " + ex.Message);
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Exception! {ex.Message}", Method = "HttpAndroid.GetProfile" });
                    mCoockieC = reserve;
                    continue;
                }
            }
            return null;
        }

        public async Task<UpdateProfileResult> UpdateProfile(ProfileResult profile, string mail)
        {
            Debug.WriteLine("Update profile");
            CookieContainer reserve = mCoockieC;
            Exception exeption = new Exception();
            for (int i = 0; i < 5; i++)
            {
                string chainingEnable = "", data = "";

                try
                {
                    chainingEnable = profile.form_data.chaining_enabled ? "on" : "off";
                    data = $"first_name={WebUtility.UrlEncode(profile.form_data.first_name)}&email={mail}&username={WebUtility.UrlEncode(profile.form_data.username)}&phone_number={WebUtility.UrlEncode(String.Empty)}&gender={profile.form_data.gender}&biography={WebUtility.UrlEncode(profile.form_data.biography)}&external_url={WebUtility.UrlEncode(profile.form_data.external_url)}&chaining_enabled={chainingEnable}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"{ex.Message}", Method = "HttpAndroid.LogIn" });
                    throw;
                }
                try
                {
                    var content = Encoding.ASCII.GetBytes(data);
                    var request = HttpRequestBuilder.Post("https://www.instagram.com/accounts/edit/", _userAgent, mCoockieC);
                    request.Referer = "https://www.instagram.com/accounts/edit/";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = content.Length;
                    request.KeepAlive = false;
                    request.Headers["Origin"] = "https://www.instagram.com";

                    // maybe exception if mCookieC not contain csrftoken
                    request.Headers["X-CSRFToken"] = mCoockieC.GetCookies(new Uri("https://www.instagram.com"))["csrftoken"].Value;
                    request.Headers["X-Instagram-AJAX"] = "1";
                    request.Headers["X-Requested-With"] = "XMLHttpRequest";
                    request.Proxy = _proxy;
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(content, 0, content.Length);
                        HttpWebResponse response = null;
                        try
                        {
                            response = await request.GetResponseAsync() as HttpWebResponse;
                        }
                        catch (WebException e)
                        {
                            response = (HttpWebResponse)e.Response;
                        }
                        using (var responseStream = response.GetResponseStream())
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            // If we get result, it always return status ok. Otherwise, exception will occur.                                           
                            mCoockieC.Add(response.Cookies);
                            var responseData = streamReader.ReadToEnd();
                            return JsonConvert.DeserializeObject<UpdateProfileResult>(responseData);
                        }

                    }
                }
                catch (Exception ex)
                {
                    // When you change your username with existed username, you will receive 404 error
                    // and obviously exception will occur. In this case, just return false
                    exeption = ex;
                    Debug.WriteLine("UpdateProfile progress occur exception " + ex.Message);
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"Exception! {ex.Message}", Method = "HttpAndroid.UpdateProfile" });
                    mCoockieC = reserve;
                    continue;
                }
            }
            throw exeption;
        }

        public async Task<PasswordChangeResult> ChangePassword(string newPassword)
        {
            Debug.WriteLine("Change password");

            try
            {
                string data = $"old_password={Password}&new_password1={newPassword}&new_password2={newPassword}";

                var content = Encoding.ASCII.GetBytes(data);
                var request = HttpRequestBuilder.Post("https://www.instagram.com/accounts/password/change/?hl=ru", _userAgent, mCoockieC);
                request.Referer = "https://www.instagram.com/accounts/password/change/?hl=ru";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = content.Length;
                request.KeepAlive = false;
                request.Headers["Origin"] = "https://www.instagram.com";
                // maybe exception if mCookieC not contain csrftoken
                request.Headers["X-CSRFToken"] = mCoockieC.GetCookies(new Uri("https://www.instagram.com"))["csrftoken"].Value;
                request.Headers["X-Instagram-AJAX"] = "1";
                request.Headers["X-IG-App-ID"] = "936619743392459";
                request.Proxy = _proxy;
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    requestStream.Write(content, 0, content.Length);
                    HttpWebResponse response = null;
                    try
                    {
                        response = await request.GetResponseAsync() as HttpWebResponse;
                    }
                    catch (WebException e)
                    {
                        response = (HttpWebResponse)e.Response;
                    }

                    using (var responseStream = response.GetResponseStream())
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        // If we get result, it always return status ok. Otherwise, exception will occur.                                           
                        mCoockieC.Add(response.Cookies);
                        var responseData = streamReader.ReadToEnd();
                        return JsonConvert.DeserializeObject<PasswordChangeResult>(responseData);
                    }
                }
            }
            catch (Exception ex)
            {
                // When you change your username with existed username, you will receive 404 error
                // and obviously exception will occur. In this case, just return false
                Debug.WriteLine(ex.Message);
                return new PasswordChangeResult() { status = "false" };
            }
        }

        public bool ConfirmMail(string path, Dictionary<string, object> mailProxyDictionary)
        {
            bool check = false;
            for (int i = 0; i < 2; i++)
            {
                WebProxy mailProxy = new WebProxy();
                try
                {
                    mailProxy.Address = new Uri($"{mailProxyDictionary["ip"]}:{mailProxyDictionary["port"]}");
                    if (mailProxyDictionary.Count > 2)
                        mailProxy.Credentials = new NetworkCredential(mailProxyDictionary["proxyLogin"].ToString(), mailProxyDictionary["proxyPassword"].ToString());
                }
                catch
                {
                    mailProxy = _proxy;
                }
                if (check)
                    mailProxy = _proxy;

                try
                {
                    var mailRequest = HttpRequestBuilder.Get($"{path}", _userAgent, mCoockieC);
                    mailRequest.Proxy = mailProxy;
                    mailRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
                    mailRequest.Headers["Upgrade-Insecure-Requests"] = "1";
                    using (var bootstrapResponse = mailRequest.GetResponse() as HttpWebResponse)
                    {
                        using (Stream dataS = bootstrapResponse.GetResponseStream())
                        using (var streamReader = new StreamReader(dataS))
                        {
                            string responseData = streamReader.ReadToEnd();
                            return true;
                        }
                    }
                }
                catch (Exception bex)
                {
                    Debug.WriteLine("Bootstrap progress meet exception " + bex.Message);
                    lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log()
                    {
                        UserName = null,
                        Date = DateTime.Now,
                        LogMessage = $"Finded exception {bex.Data}",
                        Method = "HttpAndroid.ConfirmMail"
                    });
                    if (!check)
                        check = true;
                    else
                        return false;
                }
            }
            return false;
        }
    }
}