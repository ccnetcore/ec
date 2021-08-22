using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using Hei.Captcha;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Service
{
    public class SeckillService : ISeckillService
    {
        public static string KEY_PREFIX_PATH = "yt:seckill:path";
        public static string KEY_PREFIX_STOCK = "yt:seckill:stock";
        public static string KEY_PREFIX_GOODS = "yt:seckill:goods";
        public static string KEY_PREFIX_VERIFY = "yt:verify:code";
        public static string KEY_PREFIX_USERRECORD = "yt:verify:userrecord";
        private readonly OrangeContext _orangeContext;
        private readonly CacheClientDB _cacheClientDB;
        private readonly RabbitMQInvoker _rabbitMQInvoker;
        private readonly SecurityCodeHelper _securityCode;

        public SeckillService(OrangeContext orangeContext, CacheClientDB cacheClientDB, RabbitMQInvoker rabbitMQInvoker, SecurityCodeHelper securityCodeHelper)
        {
            _orangeContext = orangeContext;
            _cacheClientDB = cacheClientDB;
            _rabbitMQInvoker = rabbitMQInvoker;
            _securityCode = securityCodeHelper;

        }

        /// <summary>
        /// 发送消息到秒杀队列当中
        /// </summary>
        /// <param name="seckillDTO"></param>
        public void sendMessage(SeckillDTO seckillDTO)
        {
            this._rabbitMQInvoker.Send(new RabbitMQConsumerModel() {ExchangeName= RabbitMQExchangeQueueName.Seckill_Exchange ,QueueName=RabbitMQExchangeQueueName.Seckill_Order_Queue} , JsonConvert.SerializeObject(seckillDTO));
        }
        /// <summary>
        /// 根据用户id查询秒杀订单
        /// </summary>
        /// <param name="goodsId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>

        public long? checkSeckillOrder(long goodsId, long userId)
        {
            TbSeckillOrder seckillOrder = _orangeContext.TbSeckillOrder.Where(m => m.UserId == userId && m.SkuId == goodsId).FirstOrDefault();

            if (seckillOrder == null)
            {
                return null;
            }
            return seckillOrder.OrderId;
        }
        /// <summary>
        /// 创建秒杀地址
        /// </summary>
        /// <param name="goodsId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string createPath(long goodsId, long id)
        {
            string str = MD5Helper.MD5EncodingOnly(goodsId.ToString() + id);
            string key = id.ToString() + "_" + goodsId;
            _cacheClientDB.SetEntryInHash(KEY_PREFIX_PATH, key, str);
            //_cacheClientDB.ExpireEntryIn(KEY_PREFIX_PATH, TimeSpan.FromSeconds(60));
            _cacheClientDB.ExpireEntryIn(KEY_PREFIX_PATH, TimeSpan.FromSeconds(60 * 60));//压测:不验证重复秒杀
            return str;

        }

        /// <summary>
        /// 验证秒杀地址
        /// </summary>
        /// <param name="goodsId"></param>
        /// <param name="id"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool checkSeckillPath(long goodsId, long id, string path)
        {
            string key = id.ToString() + "_" + goodsId;
            string encodePath = _cacheClientDB.GetValueFromHash(KEY_PREFIX_PATH, key);
            return path == encodePath;
        }
        /// <summary>
        /// 创建验证码
        /// </summary>
        /// <param name="user"></param>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public byte[] createVerifyCode(UserInfo user, string goodsId)
        {
            var code = _securityCode.GetRandomEnDigitalText(4);
            var imgbyte = _securityCode.GetEnDigitalCodeByte(code);
            _cacheClientDB.Set<string>(KEY_PREFIX_VERIFY + "_" + user.id + "," + goodsId, code, TimeSpan.FromSeconds(60));
            return imgbyte;
        }

        /// <summary>
        /// 验证验证码
        /// </summary>
        /// <param name="user"></param>
        /// <param name="goodsId"></param>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public bool checkVerifyCode(UserInfo user, long goodsId, string verifyCode)
        {
            var vc = _cacheClientDB.Get<string>(KEY_PREFIX_VERIFY + "_" + user.id + "," + goodsId);
            return vc.Equals(verifyCode, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 从缓存中获取秒杀的商品信息
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public SeckillGoods queryGoodsInfoFormCache(long skuId)
        {
            // 获取缓存中获取Hash
            SeckillGoods seckillGoods =
            _cacheClientDB.GetValueFromHash<SeckillGoods>(KEY_PREFIX_GOODS, skuId.ToString());
            seckillGoods.CurrentTime = DateTime.Now;
            seckillGoods.Stock = int.Parse(_cacheClientDB.GetValueFromHash(KEY_PREFIX_STOCK, skuId.ToString()));
            return seckillGoods;
        }

        /// <summary>
        /// 查询秒杀商品列表
        /// </summary>
        /// <returns></returns>
        public List<SeckillGoods> querySecKillList()
        {
            // 缓存存在则查询缓存,缓存不在,查询数据库,然后再缓存
            List<SeckillGoods> list = new List<SeckillGoods>();
            //keys 其实就是goodsid的集合
            var keys = _cacheClientDB.GetHashKeys(SeckillService.KEY_PREFIX_GOODS);
            foreach (var key in keys)
            {
                var goods = _cacheClientDB.GetValueFromHash<SeckillGoods>(SeckillService.KEY_PREFIX_GOODS, key);
                int stock = int.Parse(_cacheClientDB.GetValueFromHash(SeckillService.KEY_PREFIX_STOCK, goods.SkuId.ToString()));
                if (stock == 0)
                {
                    continue;
                }
                goods.Stock = stock;//库存以Stock的key为准
                list.Add(goods);
            }

            if (list.Count == 0)
            {
                var dateTime = DateTime.Now;
                // 在数据库中查询
                var listsku1 = _orangeContext.TbSeckillSku.Where(m => (m.StartTime <= dateTime && m.EndTime >= dateTime) && m.Enable == true);
                var listsku = listsku1.ToList();
                foreach (var goods in listsku)
                {
                    SeckillGoods seckillGoods = new SeckillGoods();
                    seckillGoods.Id = goods.Id;
                    seckillGoods.SkuId = goods.SkuId;
                    seckillGoods.Title = goods.Title;
                    seckillGoods.SeckillPrice = goods.SeckillPrice;
                    seckillGoods.Image = goods.Image;
                    seckillGoods.StartTime = goods.StartTime;
                    seckillGoods.EndTime = goods.EndTime;
                    seckillGoods.Enable = goods.Enable;
                    seckillGoods.Stock = _orangeContext.TbStock.FirstOrDefault(m => m.SkuId == goods.SkuId).Stock;
                    seckillGoods.Price = _orangeContext.TbSku.FirstOrDefault(m => m.Id == goods.SkuId).Price;
                    //添加缓存数据，等到数据库库存用完时清理，或者秒杀活动时间截止后清理(TODO)
                    _cacheClientDB.SetEntryInHash(SeckillService.KEY_PREFIX_GOODS, seckillGoods.SkuId.ToString(), seckillGoods);
                    _cacheClientDB.SetEntryInHash(SeckillService.KEY_PREFIX_STOCK, seckillGoods.SkuId.ToString(), goods.Stock);
                    list.Add(seckillGoods);
                }
            }

            return list;
        }

    }
}
