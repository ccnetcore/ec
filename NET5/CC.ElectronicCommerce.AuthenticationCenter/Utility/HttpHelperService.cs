using CC.ElectronicCommerce.AuthenticationCenter.Model;
using CC.ElectronicCommerce.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.AuthenticationCenter.Utility
{
    public class HttpHelperService
    {

        #region Option注入
        private readonly JWTTokenOptions _JWTTokenOptions;
        public HttpHelperService(IOptionsMonitor<JWTTokenOptions> jwtTokenOptions)
        {
            this._JWTTokenOptions = jwtTokenOptions.CurrentValue;
        }
        #endregion

        /// <summary>
        /// 调用校验服务
        /// </summary>
        /// <param name="userUrl"></param>
        /// <returns></returns>
        public Result<User> VerifyUser(string userUrl)
        {
            Result<User> ajaxResult = null;
            HttpResponseMessage sResult = this.HttpRequest(userUrl, HttpMethod.Get, null);
            if (sResult.IsSuccessStatusCode)
            {
                string content = sResult.Content.ReadAsStringAsync().Result;
                ajaxResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Result<User>>(content);
            }
            else
            {
                ajaxResult = Result<User>.Error();
            }
            return ajaxResult;
        }

        public HttpResponseMessage HttpRequest(string url, HttpMethod httpMethod, Dictionary<string, string> parameter)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpRequestMessage message = new HttpRequestMessage()
                {
                    Method = httpMethod,
                    RequestUri = new Uri(url)
                };
                if (parameter != null)
                {
                    var encodedContent = new FormUrlEncodedContent(parameter);
                    message.Content = encodedContent;
                }
                return httpClient.SendAsync(message).Result;
            }
        }


    }
}
