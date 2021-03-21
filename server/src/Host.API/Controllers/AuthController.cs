using Host.API.DTOs;
using Host.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nesh.Repository.Models;
using Nesh.Repository.Repositories;
using Newtonsoft.Json;
using Orleans;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Host.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [Authorize]
    public class AuthController : Controller
    {
        private IMongoClient MongoClient { get; }
        private IClusterClient ClusterClient { get; }
        private IHttpClientFactory HttpClients { get; }

        private ILogger Logger { get; }
        private IJWTService JWT { get; }
        private IConfiguration Configuration { get; }

        private IAccountRepository AccountRepository { get; }

        public AuthController(ILogger<AuthController> logger,
            IJWTService jwt,
            IMongoClient mongo,
            IHttpClientFactory http_clients,
            IConfiguration config,
            IAccountRepository account)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            MongoClient = mongo ?? throw new ArgumentNullException(nameof(mongo));
            JWT = jwt ?? throw new ArgumentNullException(nameof(jwt));
            HttpClients = http_clients ?? throw new ArgumentNullException(nameof(http_clients));
            Configuration = config ?? throw new ArgumentNullException(nameof(config));
            AccountRepository = account ?? throw new ArgumentNullException(nameof(account));
        }

        [HttpPost("wx_login")]
        [AllowAnonymous]
        public async Task<ActionResult> WeChatLogin([FromBody] WeChatLoginRequestDTO request)
        {
            LoginResponseDTO res = new LoginResponseDTO();
            try
            {
                string app_id = Configuration.GetSection("WeChat")["AppId"];
                string app_secrect = Configuration.GetSection("WeChat")["AppSecret"];
                string url = $"https://api.weixin.qq.com/sns/jscode2session?appid={app_id}&secret={app_secrect}&js_code={request.code}&grant_type=authorization_code";

                var client = HttpClients.CreateClient();
                var json = await client.GetStringAsync(url);
                dynamic response = JsonConvert.DeserializeObject<dynamic>(json);

                if (response.errcode == 0)
                {
                    string unionid = (string)response.unionid;
                    Account account = await AccountRepository.GetByPlatformId(unionid);
                    if (account == null)
                    {
                        account = Account.Create();
                        account.platform = Platform.WeChat;
                        account.platform_id = unionid;

                        await AccountRepository.CreateAccount(account);
                    }

                    string access_token = JWT.GetAccessToken(account);

                    await AccountRepository.RefreshToken(account.user_id, access_token);
                    res.data = access_token;
                    res.result = LoginResult.Success;
                }
                else
                {
                    res.result = LoginResult.WeChatLoginFaild;
                    res.data = (string)response.errmsg;
                }
            }
            catch (Exception ex)
            {
                res.result = LoginResult.WeChatLoginFaild;
                res.data = ex.ToString();
                Logger.LogError(ex.ToString());
            }

            return Json(res);
        }

        [HttpPost("sim_login")]
        [AllowAnonymous]
        public async Task<ActionResult> SimLogin(string user_name)
        {
            LoginResponseDTO res = new LoginResponseDTO();
            try
            {
                Account account = await AccountRepository.GetByUserName(user_name);
                if (account == null)
                {
                    
                    account = Account.Create();
                    account.platform = Platform.Sim;
                    account.platform_id = account.user_id.ToString();
                    account.user_name = user_name;

                    await AccountRepository.CreateAccount(account);
                }

                string access_token = JWT.GetAccessToken(account);
                await AccountRepository.RefreshToken(account.user_id, access_token);

                res.data = access_token;
                res.result = LoginResult.Success;
            }
            catch (Exception ex)
            {
                res.result = LoginResult.None;
                res.data = ex.ToString();
                Logger.LogError(ex.ToString());
            }

            return Json(res);
        }
    }
}
