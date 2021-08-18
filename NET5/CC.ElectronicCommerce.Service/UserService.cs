using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;

namespace CC.ElectronicCommerce.Service
{
    public class UserService : IUserService
    {
        private OrangeContext _orangeContext;
        private CacheClientDB _cacheClientDB;
        public UserService(OrangeContext orangeContext, CacheClientDB cacheClientDB)
        {
            _orangeContext = orangeContext;
            _cacheClientDB = cacheClientDB;
        }
        private static readonly string KEY_PREFIX = "user:verify:code:";

        /// <summary>
        /// 检查数据重复
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">1 账号  2手机号</param>
        /// <returns></returns>
        public Result CheckData(string data, int type)
        {
            int exist = 0;
            //判断校验数据的类型
            Result myResult;
            switch (type)
            {
                case 1:
                    exist = _orangeContext.TbUser.Count(u => u.Username.Equals(data));
                 return  myResult = exist == 0 ? Result.Success("校验成功") :Result.Error("校验失败，用户名重复");
                case 2:
                    exist = _orangeContext.TbUser.Count(u => u.Phone.Equals(data));
                    exist = _orangeContext.TbUser.Count(u => u.Username.Equals(data));
                    return myResult = exist == 0 ? Result.Success("校验成功") : Result.Error("校验失败，电话号重复");
                default:
                    return Result.Error("校验失败，参数错误");
            }
        }
        /// <summary>
        /// 根据账号密码查询用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public TbUser QueryUser(string username, string password)
        {
            //首先根据用户名查询用户
            TbUser user = _orangeContext.TbUser.Where(m => m.Username == username).FirstOrDefault();

            if (user == null)
            {
                throw new Exception("查询的用户不存在！");
            }

            if (MD5Helper.MD5EncodingWithSalt(password, user.Salt) != user.Password)
            {
                //密码不正确
                throw new Exception("密码错误");
            }
            return user;
        }
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        public void Register(TbUser user, string code)
        {
            string key = KEY_PREFIX + user.Phone;

            lock (Redis_Lock)//单线程，避免重复提交
            {
                string value = _cacheClientDB.Get<string>(key);
                if (!code.Equals(value))
                {
                    //验证码不匹配
                    throw new Exception("验证码不匹配");
                }
                _cacheClientDB.Remove(key);//把验证码从Redis中删除
            }

            user.Salt = MD5Helper.MD5EncodingOnly(user.Username);
            string md5Pwd = MD5Helper.MD5EncodingWithSalt(user.Password, user.Salt);
            user.Password = md5Pwd;
            _orangeContext.Add(user);
            int count = _orangeContext.SaveChanges();
            if (count != 1)
            {
                throw new Exception("用户注册失败");
            }
        }

        private static readonly object Redis_Lock = new object();


        /// <summary>
        /// 发送验证码方法
        /// </summary>
        /// <param name="phone"></param>
        public Result SendVerifyCode(string phone)
        {
            Random random = new Random();
            string code = random.Next(100000, 999999).ToString();// 生成随机6位数字验证码
            string key = KEY_PREFIX + phone;
            _cacheClientDB.Set(key, code, TimeSpan.FromMinutes(5));// 把验证码存储到redis中  5分钟有效,有则覆盖
            _cacheClientDB.Set(key + "1m1t", code, TimeSpan.FromMinutes(1));//一分钟只能发一次

            //return SMSTool.SendValidateCode(phone, code);// 调用发送短信的方法
            return Result.Success("验证码为："+code);
        }

        /// <summary>
        /// 1  数据库不存在
        /// 2  Redis注册频次
        /// 3  该号码一天多少次短信
        /// 4  该IP一天多少次短信
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public Result CheckPhoneNumberBeforeSend(string phone)
        {
            var list = this._orangeContext.TbUser.Where(u => u.Phone.Equals(phone)).ToList();
            if (list.Count > 0)
            {
                return Result.Error("手机号码重复");
            }

            string key = KEY_PREFIX + phone;
            if (!string.IsNullOrWhiteSpace(_cacheClientDB.Get<string>(key + "1m1t")))
            {
                return Result.Error("一分钟只能发送一次");
            }

            return Result.Success("发送成功");
        }
    }
}
