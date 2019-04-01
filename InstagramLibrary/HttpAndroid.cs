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
                _proxy.Credentials = new NetworkCredential(proxy["proxyLogin"].ToString(), proxy["proxyPassword"].ToString());
            }
            catch (Exception ex)
            {
                if (_proxy == null)
                {
                    logging.Invoke(LogIO.mainLog, new Log()
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
            try
            {
                var bootstrapRequest = HttpRequestBuilder.Get("https://www.instagram.com/accounts/login/", _userAgent, mCoockieC);
                bootstrapRequest.Proxy = _proxy;
                bootstrapRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*;q=0.8";
                bootstrapRequest.Headers["Upgrade-Insecure-Requests"] = "1";
                using (var bootstrapResponse = await bootstrapRequest.GetResponseAsync() as HttpWebResponse)
                {
                    mCoockieC.Add(bootstrapResponse.Cookies);
                }
            }
            catch (Exception bex)
            {
                Debug.WriteLine("Bootstrap progress meet exception " + bex.Message);
                throw bex;
            }
            try
            {
                string data = $"username={Username}&password={Password}";
                byte[] content = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = HttpRequestBuilder.Post("https://www.instagram.com/accounts/login/ajax/", _userAgent, mCoockieC);


                request.Referer = "https://www.instagram.com/accounts/login/";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = content.Length;
                request.KeepAlive = true;
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
                    try
                    {
                        using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                        {
                            using (Stream dataS = response.GetResponseStream())
                            using (var streamReader = new StreamReader(dataS))
                            {
                                string responseData = streamReader.ReadToEnd();
                                Console.WriteLine(responseData);
                                mCoockieC.Add(response.Cookies);
                                try
                                {
                                    LoginResult logResult = JsonConvert.DeserializeObject<LoginResult>(responseData);
                                    return logResult;
                                }
                                catch
                                {
                                    try
                                    {
                                        RootobjectChallenge result = JsonConvert.DeserializeObject<RootobjectChallenge>(responseData);
                                        if (result.message == "challenge_required")
                                        {
                                            return new LoginResult() { status = "challenge_required" };
                                        }
                                        return new LoginResult() { status = "ok" };
                                    }
                                    catch
                                    {
                                        return new LoginResult() { status = "success" };
                                    }
                                }
                            }
                        }
                    }
                    catch (WebException e)
                    {
                        using (HttpWebResponse response = (HttpWebResponse)e.Response)
                        {
                            HttpWebResponse httpResponse = (HttpWebResponse)response;
                            Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                            using (Stream dataS = response.GetResponseStream())
                            using (var streamReader = new StreamReader(dataS))
                            {
                                string responseData = streamReader.ReadToEnd();
                                mCoockieC.Add(response.Cookies);
                                try
                                {
                                    LoginResult logResult = JsonConvert.DeserializeObject<LoginResult>(responseData);
                                    return logResult;
                                }
                                catch
                                {
                                    try
                                    {
                                        RootobjectChallenge result = JsonConvert.DeserializeObject<RootobjectChallenge>(responseData);
                                        if (result.message == "challenge_required")
                                        {
                                            return new LoginResult() { status = "challenge_required" };
                                        }
                                        return new LoginResult() { status = "ok" };
                                    }
                                    catch
                                    {
                                        return new LoginResult() { status = "success" };
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Login progress occur exception " + ex.Message);
                return null;
                throw ex;
            }
        }

        public async Task<ProfileResult> GetProfile()
        {
            Debug.WriteLine("Get profile");
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

        public async Task<UpdateProfileResult> UpdateProfile(ProfileResult profile, string mail)
        {
            Debug.WriteLine("Update profile");

            string chainingEnable = "", data = "";

            try
            {
                chainingEnable = profile.form_data.chaining_enabled ? "on" : "off";
                data = $"first_name={WebUtility.UrlEncode(profile.form_data.first_name)}&email={mail}&username={WebUtility.UrlEncode(profile.form_data.username)}&phone_number={WebUtility.UrlEncode(String.Empty)}&gender={profile.form_data.gender}&biography={WebUtility.UrlEncode(profile.form_data.biography)}&external_url={WebUtility.UrlEncode(profile.form_data.external_url)}&chaining_enabled={chainingEnable}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, LogMessage = $"{ex.Message}", Method = "HttpAndroid.LogIn" });
                throw;
            }
            try
            {
                var content = Encoding.ASCII.GetBytes(data);
                var request = HttpRequestBuilder.Post("https://www.instagram.com/accounts/edit/", _userAgent, mCoockieC);
                request.Referer = "https://www.instagram.com/accounts/edit/";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = content.Length;
                request.KeepAlive = true;
                request.Headers["Origin"] = "https://www.instagram.com";

                // maybe exception if mCookieC not contain csrftoken
                request.Headers["X-CSRFToken"] = mCoockieC.GetCookies(new Uri("https://www.instagram.com"))["csrftoken"].Value;
                request.Headers["X-Instagram-AJAX"] = "1";
                request.Headers["X-Requested-With"] = "XMLHttpRequest";

                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(content, 0, content.Length);
                    request.Proxy = _proxy;
                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    {
                        mCoockieC.Add(response.Cookies);
                        using (var responseStream = response.GetResponseStream())
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            // If we get result, it always return status ok. Otherwise, exception will occur.                                           
                            var responseData = streamReader.ReadToEnd();
                            return JsonConvert.DeserializeObject<UpdateProfileResult>(responseData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // When you change your username with existed username, you will receive 404 error
                // and obviously exception will occur. In this case, just return false
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        public bool ConfirmMail(string path)
        {
            return true;
        }
    }
}