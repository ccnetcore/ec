using CC.ElectronicCommerce.AuthenticationCenter.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.AuthenticationCenter.Utility
{
    public class CustomHSJWTService : ICustomJWTService
    {
        #region Option注入
        private readonly JWTTokenOptions _JWTTokenOptions;
        public CustomHSJWTService(IOptionsMonitor<JWTTokenOptions> configInformation)
        {
            this._JWTTokenOptions = configInformation.CurrentValue;
        }
        #endregion
        /// <summary>
        /// 用户登录成功以后，用来生成Token的方法
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public string GetToken(User user)
        {
            var claims = new[]
            {
                 new Claim("username", user.username),
                 new Claim("id", user.id.ToString()) 
            };

            //需要加密：需要加密key:
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JWTTokenOptions.SecurityKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new JwtSecurityToken(
             issuer: _JWTTokenOptions.Issuer,
             audience: _JWTTokenOptions.Audience,
             claims: claims,
             expires: DateTime.Now.AddDays(1),
             signingCredentials: creds);
            string returnToken = new JwtSecurityTokenHandler().WriteToken(token);
            return returnToken;
        }
    }
}
