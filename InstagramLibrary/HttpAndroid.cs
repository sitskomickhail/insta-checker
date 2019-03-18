using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InstagramLibrary
{
    public class HttpAndroid
    {

        private string _CsrfToken;
        public string CsrfToken { get { return _CsrfToken; } }

        private IInstaApi _instaApi;
        public IInstaApi InstaApi
        { get { return _instaApi; } }


        public async Task<IResult<InstaLoginResult>> Login(string username, string instPass, string ip, int port, string login = null, string password = null)
        {
            var userSession = new UserSessionData
            {
                UserName = username,
                Password = instPass,
            };


            var httpHandler = new HttpClientHandler();

            WebProxy wp = new WebProxy(ip, port);
            wp.Credentials = new NetworkCredential(login, password);
            httpHandler.Proxy = wp;
            var delay = RequestDelay.FromSeconds(1, 1);

            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .UseHttpClientHandler(httpHandler)
                .Build();

            var res = await _instaApi.LoginAsync();

            _CsrfToken = userSession.CsrfToken;

            if (res.Info.Message == "Challenge is required")
            {
                var resul = await _instaApi.ResetChallenge();
                var verif = await _instaApi.ChooseVerifyMethod(1);
            }
            return res;
        }
    }
}
