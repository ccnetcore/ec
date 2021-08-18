using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Core.ConsulExtend;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.UserMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        //private readonly IPLocation.IPLocationClient _IPLocationClient;
        //private readonly AbstractConsulDispatcher _AbstractConsulDispatcher = null;
        private readonly IConfiguration _IConfiguration = null;
        public UserController(IUserService userService, ILogger<UserController> logger, /*IPLocation.IPLocationClient ipLocationClient, AbstractConsulDispatcher abstractConsulDispatcher,*/ IConfiguration configuration)
        {
            this._userService = userService;
            this._logger = logger;
            //this._IPLocationClient = ipLocationClient;
            //this._AbstractConsulDispatcher = abstractConsulDispatcher;
            this._IConfiguration = configuration;
        }
        /**
         * 校验数据
         * @param data
         * @param type
         * @return
         */
        [Route("check/{data}/{type}")]
        [HttpGet]
        public Result CheckData(string data, int type)
        {
            return _userService.CheckData(data, type);
        }



        /**
		 * 发送验证码
		 * @param phone
		 * @return
		 */
        [Route("send")]
        [HttpPost]
        public Result SendVerifyCode(string phone)
        {
            //检查的时候，需要 ip--

            Result ajaxResult = this._userService.CheckPhoneNumberBeforeSend(phone);
            if (!ajaxResult.status)//校验失败
            {
                return ajaxResult;
            }
            else
            {

                return _userService.SendVerifyCode(phone);
            }
        }

        /**
		 * 用户注册
		 * @param user
		 * @param code
		 * @return
		 */
        [Route("register")]
        [HttpPost]
        //[TypeFilter(typeof(CustomAction2CommitFilterAttribute))]
        public Result Register(TbUser user, string code)
        {
            _userService.Register(user, code);
            return Result.Success("注册成功");
        }

        /**
		 * 根据用户名和密码查询用户
		 * @param username
		 * @param password
		 * @return
		 */
        [Route("query")]
        [HttpGet]
        public Result<TbUser> QueryUser(string username, string password)
        {
            Console.WriteLine($"This is {typeof(UserController).Name}{nameof(QueryUser)} username={username} password={password}");

            TbUser tbUser = _userService.QueryUser(username, password);

            return Result<TbUser>.Success().SetData(tbUser);
        }

        /**
	    * 根据用户名和密码查询用户
	    * @param username
	    * @param password
	    * @return
	    */
        [Route("/api/user/verify")]
        [HttpGet]
        [AllowAnonymousAttribute]//自己校验
        public Result CurrentUser()
        {
            Result ajaxResult = null;
            IEnumerable<Claim> claimlist = HttpContext.AuthenticateAsync().Result.Principal.Claims;
            if (claimlist != null && claimlist.Count() > 0)
            {
                string username = claimlist.FirstOrDefault(u => u.Type == "username").Value;
                string id = claimlist.FirstOrDefault(u => u.Type == "id").Value;

                ajaxResult = Result.Success().SetData(new
                {
                    id = id,
                    username = username,
                });
            }
            else
            {
                ajaxResult = Result.Error("token无效");
            }
            return ajaxResult;
        }
        //[Route("/api/user/location")]
        //[HttpGet]
        //[AllowAnonymousAttribute]
        //public JsonResult CurrentLocation()
        //{
        //    //string targetUrl = $"http://{this._IConsulDispatcher.ChooseAddress("LessonService")}";
        //    ////"http://localhost:8000"

        //    {
        //        //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        //        //LocationReply locationReply = null;
        //        //locationReply = this._IPLocationClient.Location(new IPRequest() { Ip = base.HttpContext.Connection.RemoteIpAddress.ToString() });
        //        //return new JsonResult(new AjaxResult()
        //        //{
        //        //    Result = true,
        //        //    Value = locationReply.LocationDetail
        //        //});
        //    }
        //    {
        //        string targetUrl = this._AbstractConsulDispatcher.GetAddress(this._IConfiguration["IPLibraryServiceUrl"]);
        //        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        //        using (var channel = GrpcChannel.ForAddress(targetUrl))
        //        {
        //            var client = new IPLocation.IPLocationClient(channel);
        //            {
        //                var locationReply = client.Location(new IPRequest() { Ip = base.HttpContext.Connection.RemoteIpAddress.ToString() });
        //                return new JsonResult(new AjaxResult()
        //                {
        //                    Result = true,
        //                    Value = locationReply.LocationDetail
        //                });
        //            }
        //        }
        //    }

        //}

    }
}
