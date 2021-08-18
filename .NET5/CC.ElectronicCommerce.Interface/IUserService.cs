using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Interface
{
    public interface IUserService
    {
        /**
    * 校验用户对象数据类型
    * @param data
    * @param type
    * @return
    */
        Result CheckData(string data, int type);

        /**
         * 发送验证码
         * @param phone
         */
        Result SendVerifyCode(string phone);

        /// <summary>
        /// 发送验证码前验证
        /// </summary>
        /// <param name="phone"></param>
        Result CheckPhoneNumberBeforeSend(string phone);

        /**
         * 用户注册
         * @param user
         * @param code
         */
        void Register(TbUser user, string code);

        /**
         * 根据账号和密码查询用户信息
         * @param username
         * @param password
         * @return
         */
        TbUser QueryUser(string username, string password);
    }
}
