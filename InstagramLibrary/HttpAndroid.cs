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

        public async Task<IResult<InstaLoginResult>> Login(int delay, string username, string instPass, string ip, int port, string login = null, string password = null)
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
            var requestDelay = RequestDelay.FromSeconds(delay, delay);

            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(requestDelay)
                .UseHttpClientHandler(httpHandler)
                .Build();

            IResult<InstaLoginResult> res = null;
                requestDelay.Disable();
            for (int i = 0; i < 5; i++)
            {
                res = await _instaApi.LoginAsync();
                if (res.Info.Message != "Произошла ошибка при отправке запроса." && res.Info.Message != "An error occurred while sending the request.")
                    break;
                Thread.Sleep(2000);                
            }
            requestDelay.Enable();

            _CsrfToken = userSession.CsrfToken;
            UserSession = userSession;
            return res;
        }
        
    }
}
