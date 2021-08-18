using CC.ElectronicCommerce.AuthenticationCenter.Model;
using CC.ElectronicCommerce.AuthenticationCenter.Utility;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Core.ConsulExtend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace CC.ElectronicCommerce.AuthenticationCenter.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ICustomJWTService _iJWTService = null;
        private readonly HttpHelperService _HttpHelperService = null;
        private readonly IConfiguration _IConfiguration = null;
        private readonly AbstractConsulDispatcher _IConsulDispatcher = null;
        public AuthController(ILogger<AuthController> logger, ICustomJWTService service, HttpHelperService httpHelperService, IConfiguration configuration, AbstractConsulDispatcher consulDispatcher)
        {
            this._logger = logger;
            this._iJWTService = service;
            this._HttpHelperService = httpHelperService;
            this._IConfiguration = configuration;
            this._IConsulDispatcher = consulDispatcher;
        }

        [Route("api/auth/accredit")]
        [HttpPost]
        public Result Accredit(LoginModel loginModel)
        {
            string requestUrl = $"{this._IConfiguration["VerifyUserUrl"]}?username={loginModel.username}&password={loginModel.password}";
            string realUrl = this._IConsulDispatcher.GetAddress(requestUrl);
            string token="错误";

            Console.WriteLine($"{requestUrl}--{realUrl}");
            Result<User> ajaxResult = _HttpHelperService.VerifyUser(realUrl);
            if (ajaxResult.status)
            {
                token = this._iJWTService.GetToken(ajaxResult.data);
            }
            Console.WriteLine($"Accredit Result : {JsonConvert.SerializeObject(ajaxResult)}");
            return Result.Success().SetData(token);
        }
    }
}
