using AuthCodeFlow.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AuthCodeFlow.Controllers
{
    public class HomeController : Controller
    {
        static string _tokenEndpoint = string.Format(
            ConfigurationManager.AppSettings["b2c:TokenEndPoint"],
            ConfigurationManager.AppSettings["b2c:Tenant"],
            ConfigurationManager.AppSettings["b2c:SignupinPolicy"]);
        static string _b2cTenantEndpoint = ConfigurationManager.AppSettings["b2c:TenantEndPoint"];
        static string _clientId = ConfigurationManager.AppSettings["b2c:ClientId"];
        static string _clientSecret = ConfigurationManager.AppSettings["app:Secret"];
        static string _redirectUri = ConfigurationManager.AppSettings["app:RedirectUri"];
        static string _scopes = string.Join(" ", ConfigurationManager.AppSettings["b2c:Scopes"].Split(' ').Select(s =>
            string.Format(ConfigurationManager.AppSettings["b2c:ScopePart"],
            ConfigurationManager.AppSettings["b2c:Tenant"], ConfigurationManager.AppSettings["app:ApiAppId"], s)).ToArray()) + " offline_access profile";

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Token(TokenRequest tokenRequest)
        {
            HttpResponseMessage response = null;
            TokenResponse tokenResponse = null;

            var baseAddress = new Uri(_b2cTenantEndpoint);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string intent = "", intentValue = "";
                if (string.IsNullOrEmpty(tokenRequest.RefreshToken))
                {
                    intent = "code";
                    intentValue = tokenRequest.Code;
                }
                else
                {
                    intent = "refresh_token";
                    intentValue = tokenRequest.RefreshToken;
                }

                var req = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
                {
                    Content = new FormUrlEncodedContent(new[] {
                        new KeyValuePair<string, string>("grant_type", tokenRequest.GrantType),
                        new KeyValuePair<string, string>("client_id", _clientId),
                        new KeyValuePair<string, string>("scope", _scopes),
                        new KeyValuePair<string, string>(intent, intentValue),
                        new KeyValuePair<string, string>("redirect_uri", _redirectUri),
                        new KeyValuePair<string, string>("client_secret", _clientSecret)
                    }) };
                cookieContainer.Add(baseAddress, new Cookie("stsservicecookie", "cpim_te"));
                cookieContainer.Add(baseAddress, new Cookie("x-ms-gateway-slice", "001-000"));
                response = await client.SendAsync(req);
                if (response.IsSuccessStatusCode)
                {
                    // Get the URI of the created resource.  
                    string responseString = await response.Content.ReadAsStringAsync();
                    tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseString);
                }
            }
            if (tokenResponse == null)
                return Json(new { error = response.ReasonPhrase });
            if (string.IsNullOrEmpty(tokenResponse.Error))
                return Json(new { accessToken = tokenResponse.AccessToken, refreshToken = tokenResponse.RefreshToken });
            else
                return Json(new { error = tokenResponse.Error, errorDescription = tokenResponse.ErrorDescription});
        }
    }
}