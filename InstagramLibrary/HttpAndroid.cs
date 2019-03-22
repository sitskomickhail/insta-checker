using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;
using System.Net;
using System.Net.Http;
using System.Threading;
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

        public UserSessionData UserSession { get; private set; }

        public async Task<IResult<InstaLoginResult>> Login(string username, string instPass, string ip, int port, string login = null, string password = null)
        {
            var userSession = new UserSessionData
            {
                UserName = username,
                Password = instPass,
            };


            var httpHandler = new HttpClientHandler();

            WebProxy wp = new WebProxy(ip, port);
            if (login != null && password != null)
                wp.Credentials = new NetworkCredential(login, password);
            

            httpHandler.Proxy = wp;
            var delay = RequestDelay.FromSeconds(11, 11);

            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .UseHttpClientHandler(httpHandler)
                .Build();

            IResult<InstaLoginResult> res = null;
            for (int i = 0; i < 5; i++)
            {
                res = await _instaApi.LoginAsync();
                if (res.Info.Message != "Произошла ошибка при отправке запроса.")
                    break;
                Thread.Sleep(1000);
            }
            _CsrfToken = userSession.CsrfToken;
            UserSession = userSession;

            return res;
        }
    }
}
