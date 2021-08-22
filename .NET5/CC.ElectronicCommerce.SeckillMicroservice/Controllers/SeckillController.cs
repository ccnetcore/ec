using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Service;
using CC.ElectronicCommerce.WebCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CC.ElectronicCommerce.SeckillMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeckillController : ControllerBase
    {
        private readonly ISeckillService _seckillService;
        private readonly CacheClientDB _cacheClientDB;
        private readonly IGoodsService _goodsService;

        public SeckillController(ISeckillService seckillService,
            CacheClientDB cacheClientDB, IGoodsService goodsService)
        {
            this._seckillService = seckillService;
            this._cacheClientDB = cacheClientDB;
            this._goodsService = goodsService;
        }
        /**
     * @param path
     * @param goodsId
     * @return 前端秒杀API
     */
        [HttpPost]
        [Route("{path}/seck/{skuId}")]
        // [Authorize]
        public Result secKillOrder(string path, long skuId)
        {
            // 获取登录的用户对象
            //UserInfo userInfo = base.HttpContext.GetCurrentUserInfo();//这里测试用 用户id1
            UserInfo userInfo = new UserInfo() {id=1,username= "gerry" };

            #region 5. 判断是否已经重复秒杀
            {
                if (this._cacheClientDB.HashContainsEntry($"{SeckillService.KEY_PREFIX_USERRECORD}_{skuId}", $"{userInfo.id}"))
                {
                    //Console.WriteLine($"{userInfo.id}已经秒杀过");
                    //return Result.Error("你已重复参加秒杀");
                    //-----------------------------------------------【标记】
                    Console.WriteLine("开启测试模式");
                    //压测:不验证重复秒杀
                }
                //long? result = _seckillService.checkSeckillOrder(goodsId, userInfo.id);
                //if (result != null)
                //{
                //    return new JsonResult(new AjaxResult()
                //    {
                //        Result = false,
                //        Message = "已经参与该商品秒杀，请勿重复秒杀",
                //    });
                //}
            }
            #endregion


            //1.验证路径
            bool check = _seckillService.checkSeckillPath(skuId, userInfo.id, path);
            if (!check)
            {
                return Result.Error("秒杀码非法");
            }




            #region 先检查下库存是否已结束
            if (!this._cacheClientDB.HashContainsEntry(SeckillService.KEY_PREFIX_STOCK, skuId.ToString()))
            {
                return Result.Error("秒杀已结束");
            }
            if (int.Parse(this._cacheClientDB.GetValueFromHash(SeckillService.KEY_PREFIX_STOCK, skuId.ToString())) == 0)
            {
                return Result.Error("秒杀已结束");
            }
            #endregion

            ////2.内存标记，减少redis访问
            //boolean over = localOverMap.get(goodsId);
            //if (over)
            //{
            //	return Result.error(CodeMsg.MIAO_SHA_OVER);
            //}

            

            #region 确认库存
            {
                // 创建库存对象
                StockParam param = new StockParam();

                //3.读取库存，减一后更新缓存
                long stock = this._cacheClientDB.IncrementValueInHash(SeckillService.KEY_PREFIX_STOCK, skuId.ToString(), -1);
                param.stock = stock + 1;
                param.goodsId = skuId;

                //4.库存不足直接返回----超库存请求都在这里拦截
                if (stock < 0)
                {
                    this._cacheClientDB.IncrementValueInHash(SeckillService.KEY_PREFIX_STOCK, skuId.ToString(), 1);//放回去
                    return Result.Error("库存不足");
                }
                else
                {
                    if (stock == 0)//库存已被秒杀完---前端根据库存控制样式
                    {
                        //可以加个开关
                    }

                    #region 缓存该用户的秒杀记录
                    this._cacheClientDB.SetEntryInHash($"{SeckillService.KEY_PREFIX_USERRECORD}_{skuId}", $"{userInfo.id}", DateTime.Now.Ticks.ToString());
                    //秒杀活动结束全部清除
                    #endregion
                    //6.库存充足，请求入队
                    // 获取秒杀的商品信息
                    SeckillGoods seckillGoods = _seckillService.queryGoodsInfoFormCache(skuId);
                    SeckillDTO dto = new SeckillDTO(userInfo, seckillGoods);//获取用户信息
                    _seckillService.sendMessage(dto);//发送异步任务

                    return Result.Success("秒杀成功！请尽快支付，5分钟后取消！");
                }
            }
            #endregion
        }

        /**
		* 获取秒杀路径()
		* @param secKillParam
		* @return
		*/
        [HttpPost]
        [Route("getPath/")]
        //[Authorize]
        public Result getSecKillPath( SecKillParam secKillParam)
        {
            long goodsId = secKillParam.goodsId;
            //UserInfo userInfo = base.HttpContext.GetCurrentUserInfo();//测试用
            UserInfo userInfo = new UserInfo() { id = 1 };
            if (userInfo == null)
            {
                return Result.Error("用户身份失效");
            }

            bool check = _seckillService.checkVerifyCode(userInfo, goodsId, secKillParam.verifyCode);
            if (!check)
            {
                return Result.Error("验证码错误");
            }

            // 获取秒杀的地址组成可变部分
            string path = _seckillService.createPath(goodsId, userInfo.id);
            return Result.Success("获取秒杀码成功！").SetData(path);
        }

        [HttpGet]
        [Route("verifyCode/{goodsId}/{userId}")]
        //[Authorize]
        public IActionResult getSecKillVerifyCode(long goodsId, long userId)
        {
            //UserInfo userInfo = base.HttpContext.GetCurrentUserInfo();
            UserInfo userInfo = new UserInfo()
            {
                id = userId
            };
            byte[] imgbyte = _seckillService.createVerifyCode(userInfo, goodsId.ToString());
            return File(imgbyte, "image/png");

        }
        /**
		 * 根据userId查询秒杀订单号
		 *
		 * @param goodsId
		 * @return 前端轮询判断是否秒杀成功
		 */

        [HttpGet]
        [Route("result/{goodsId}")]
        // [Authorize]
        public Result checkSeckillOrder(long goodsId)
        {
            //UserInfo userInfo = base.HttpContext.GetCurrentUserInfo();//测试用
            UserInfo userInfo = new UserInfo() { id = 1 };
            long? result = _seckillService.checkSeckillOrder(goodsId, long.Parse(userInfo.id.ToString()));//也可以换成Redis--processor生成个redis
            if (result == null)
            {
                return Result.Error("秒杀结果处理中！");
            }
            else
            {
                return Result.Success("获取秒杀订单号成功").SetData(new
                {
                    status = 200,
                    orderId = long.Parse(result.ToString())
                });

            }
        }

        /// <summary>
        /// 查询可以秒的商品列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("list")]
        public List<SeckillGoods> listSeckillGoods()
        {
            return _seckillService.querySecKillList();
        }

        /// <summary>
        /// 查询可以秒杀的商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public SeckillGoods querySeckillGoods(long id)
        {
            return _seckillService.queryGoodsInfoFormCache(id);
        }

        /// <summary>
        /// 添加秒杀商品
        /// </summary>
        /// <param name="seckillParameter"></param>
        /// <returns></returns>
        [Route("/seckill/addSeckillGoods")]
        [HttpPost]
        public Result AddSeckillGoods(SeckillParameter seckillParameter)
        {
            _goodsService.AddSeckillGoods(seckillParameter);


            return Result.Success();
        }

    }
}
