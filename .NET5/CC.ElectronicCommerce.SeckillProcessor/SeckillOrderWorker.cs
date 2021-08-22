using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Common.QueueModel;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Service;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Zhaoxi.MSACormmerce.SeckillProcessor
{
    public class SeckillOrderWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SeckillOrderWorker> _logger;
        private readonly RabbitMQInvoker _RabbitMQInvoker;
        private readonly IOrderService _IOrderService = null;
        private readonly OrangeContext _orangeContext;
        private readonly CacheClientDB _cacheClientDB;

        public SeckillOrderWorker(ILogger<SeckillOrderWorker> logger, RabbitMQInvoker rabbitMQInvoker, IConfiguration configuration, OrangeContext orangeContext, CacheClientDB cacheClientDB)
        {
            this._logger = logger;
            this._RabbitMQInvoker = rabbitMQInvoker;
            this._configuration = configuration;
            this._orangeContext = orangeContext;
            this._cacheClientDB = cacheClientDB;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RabbitMQConsumerModel rabbitMQConsumerModel = new RabbitMQConsumerModel()
            {
                ExchangeName = RabbitMQExchangeQueueName.Seckill_Exchange,
                QueueName = RabbitMQExchangeQueueName.Seckill_Order_Queue
            };
            this._RabbitMQInvoker.RegistReciveAction(rabbitMQConsumerModel, message =>
            {
                try
                {

                    SeckillDTO seckillDTO = JsonConvert.DeserializeObject<SeckillDTO>(message);
                    UserInfo userInfo = seckillDTO.userInfo;
                    var seckillGood = seckillDTO.seckillGoods;
                    TbSeckillSku seckillSku = new TbSeckillSku()
                    {
                        Enable = seckillGood.Enable,
                        CurrentTime = seckillGood.CurrentTime,
                        EndTime = seckillGood.EndTime,
                        Id = seckillGood.Id,
                        Image = seckillGood.Image,
                        Price = seckillGood.Price,
                        SeckillPrice = seckillGood.SeckillPrice,
                        SeckillTotal = seckillGood.SeckillTotal,
                        SkuId = seckillGood.SkuId,
                        StartTime = seckillGood.StartTime,
                        Stock = seckillGood.Stock,
                        Title = seckillGood.Title
                    };

                    //-----------------------------------------------【标记】测试环境将这个关闭
                    //#region 先确认用户有没有秒杀订单记录--防重放攻击
                    //if (this._orangeContext.TbSeckillOrder.Count(m => m.SkuId == seckillSku.SkuId && m.UserId == userInfo.id) > 0)
                    //{

                    //    return true;//直接结束
                    //}
                    //#endregion

                    #region 库存不充足--更新Enable 以及  Redis结束
                    TbStock stock = this._orangeContext.TbStock.FirstOrDefault(m => m.SkuId == seckillSku.SkuId);
                    if (stock.SeckillStock <= 0)
                    {
                        //如果库存不足的话修改秒杀商品的enable字段
                        TbSeckillSku tbSeckillSkuDB = _orangeContext.TbSeckillSku.First(m => m.Id == seckillSku.Id);
                        if (seckillSku.Enable)
                        {
                            seckillGood.Enable = false;
                            _orangeContext.SaveChanges();
                        }
                        return true;
                    }
                    #endregion


                    #region 自动生成订单
                    IDbContextTransaction trans = null;
                    try
                    {
                        trans = this._orangeContext.Database.BeginTransaction();

                        //构造order对象
                        TbOrder order = new TbOrder();
                        order.PaymentType = 1;
                        order.TotalPay = long.Parse(seckillSku.SeckillPrice.ToString());
                        order.ActualPay = long.Parse(seckillSku.SeckillPrice.ToString());
                        order.PostFee = 0L;
                        // 实际项目应该查询用户的默认收货地址
                        order.Receiver = "gerry";
                        order.ReceiverMobile = "18986164761";
                        order.ReceiverCity = "武汉";
                        order.ReceiverDistrict = "洪山区";
                        order.ReceiverState = "武汉";
                        order.ReceiverAddress = "武汉市汉阳区人信大厦1502";
                        order.ReceiverZip = "000000000";
                        order.InvoiceType = 0;
                        order.SourceType = 2;

                        TbOrderDetail orderDetail = new TbOrderDetail();
                        orderDetail.SkuId = seckillSku.SkuId;
                        orderDetail.Num = 1;
                        orderDetail.Title = seckillSku.Title;
                        orderDetail.Image = seckillSku.Image;
                        //todo
                        orderDetail.Price = seckillSku.SeckillPrice;
                        orderDetail.OwnSpec = _orangeContext.TbSku.Where(m => m.Id == seckillSku.SkuId).FirstOrDefault().OwnSpec;
                        order.orderDetails.Add(orderDetail); ;

                        //3.1 生成orderId
                        long orderId = SnowflakeHelper.Next();
                        //3.2 初始化数据
                        order.BuyerNick = userInfo.username;
                        order.BuyerRate = false;
                        order.CreateTime = DateTime.Now;
                        order.OrderId = orderId;
                        order.UserId = userInfo.id.ToString();
                        //3.3 保存数据
                        _orangeContext.TbOrder.Add(order);
                        _orangeContext.SaveChanges();

                        //3.4 保存订单状态
                        TbOrderStatus orderStatus = new TbOrderStatus();
                        orderStatus.OrderId = orderId;
                        orderStatus.CreateTime = order.CreateTime;
                        //初始状态未未付款：1
                        orderStatus.Status = 1;
                        //3.5 保存数据
                        _orangeContext.TbOrderStatus.Add(orderStatus);
                        _orangeContext.SaveChanges();

                        //3.6 在订单详情中添加orderId
                        foreach (var od in order.orderDetails)
                        {
                            od.OrderId = orderId;
                        }

                        //3.7 保存订单详情，使用批量插入功能 
                        _orangeContext.TbOrderDetail.AddRange(order.orderDetails);
                        _orangeContext.SaveChanges();
                        //3.8 修改库存
                        foreach (var ord in order.orderDetails)
                        {
                            TbStock stock1 = _orangeContext.TbStock.First(m => m.SkuId == ord.SkuId);
                            stock1.SeckillStock = stock1.SeckillStock - ord.Num;
                            
                            _orangeContext.TbStock.Update(stock1);
                            _orangeContext.SaveChanges();
                            //新建秒杀订单
                            TbSeckillOrder seckillOrder = new TbSeckillOrder();
                            seckillOrder.OrderId = orderId;
                            seckillOrder.SkuId = ord.SkuId;
                            seckillOrder.UserId = userInfo.id;
                            _orangeContext.TbSeckillOrder.Add(seckillOrder);
                            _orangeContext.SaveChanges();

                            #region 数据库的库存为空了，关闭秒杀
                            if (stock1.SeckillStock == 0)
                            {
                                TbSeckillSku tbSeckillSkuDB = _orangeContext.TbSeckillSku.First(m => m.Id == seckillSku.Id);
                                if (seckillSku.Enable)
                                {
                                    seckillGood.Enable = false;
                                    _orangeContext.SaveChanges();
                                }
                                this._cacheClientDB.RemoveEntryFromHash(SeckillService.KEY_PREFIX_GOODS, tbSeckillSkuDB.SkuId.ToString());//清除秒杀商品缓存--结束了
                                this._cacheClientDB.RemoveEntryFromHash(SeckillService.KEY_PREFIX_STOCK, tbSeckillSkuDB.SkuId.ToString());//清除秒杀库存缓存--结束了
                            }
                            #endregion
                        }
                        trans.Commit();
                        #endregion


                        #region 延时取消任务+确认订单支付状态任务
                        {
                            try
                            {
                                var orderCreateQueueModel = new OrderCreateQueueModel()
                                {
                                    OrderId = order.OrderId,
                                    UserId = userInfo.id,
                                    SkuIdList = new List<long>() { seckillSku.SkuId },
                                    TryTime = 0,
                                    OrderType = OrderCreateQueueModel.OrderTypeEnum.Seckill
                                };

                                string messageTarget = JsonConvert.SerializeObject(orderCreateQueueModel);
                                //发布刷新订单支付状态---但不需要清除购物车---TODO
                                //this._RabbitMQInvoker.Send(RabbitMQExchangeQueueName.OrderCreate_Exchange, messageTarget);

                                //this._RabbitMQInvoker.SendDelay(RabbitMQExchangeQueueName.OrderCreate_Delay_Exchange, message, 60 * 30);
                                //发布延时关闭订单任务,加个字段重用起来
                                this._RabbitMQInvoker.SendDelay(RabbitMQExchangeQueueName.OrderCreate_Delay_Exchange, messageTarget, 60 * 5);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                this._logger.LogError($"发送异步购物车清理消息失败，OrderId={order.OrderId}，UserId={userInfo.id}");
                            }
                        }
                        #endregion


                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine("----------------------");
                        if (trans != null)
                            trans.Rollback();
                        return false;
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    LogModel logModel = new LogModel()
                    {
                        OriginalClassName = this.GetType().FullName,
                        OriginalMethodName = nameof(ExecuteAsync),
                        Remark = "秒杀业务异步处理错误日志"
                    };
                    this._logger.LogError(ex, $"{nameof(SeckillOrderWorker)}.CancelOrder failed message={message}, Exception:{ex.Message}", JsonConvert.SerializeObject(logModel));
                    return false;
                }
            });
            await Task.CompletedTask;
        }
    }
}
