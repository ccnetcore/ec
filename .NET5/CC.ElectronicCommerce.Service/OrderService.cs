using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using DotNetCore.CAP;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Model.DTO;
using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common;
using CC.ElectronicCommerce.Common.QueueModel;

namespace Zhaoxi.MSACommerce.Service
{
    public class OrderService : IOrderService
    {
        private const string KEY_PAY_PREFIX = "order:pay:url:";


        private readonly IGoodsService _goodsService;
        private readonly OrangeContext _orangeContext;
        private readonly ILogger<OrderService> _logger;
        private readonly CacheClientDB _cacheClientDB;
        private readonly RabbitMQInvoker _RabbitMQInvoker = null;
        private readonly ICapPublisher _iCapPublisher;
        public OrderService(IGoodsService goodsService, OrangeContext orangeContext, ILogger<OrderService> logger, CacheClientDB cacheClientDB, RabbitMQInvoker rabbitMQInvoker, ICapPublisher capPublisher)
        {
            this._goodsService = goodsService;
            this._orangeContext = orangeContext;
            this._logger = logger;
            this._cacheClientDB = cacheClientDB;
            this._RabbitMQInvoker = rabbitMQInvoker;
            this._iCapPublisher = capPublisher;
        }
        public long CreateOrder(OrderDto orderDto, UserInfo userInfo)
        {
            #region 插入订单

            #region 数据准备
            //生成订单ID，采用自己的算法生成订单ID
            long orderId = SnowflakeHelper.Next();
            //填充order
            TbOrder order = new TbOrder();
            order.CreateTime = DateTime.Now;
            order.OrderId = orderId;
            order.PaymentType = orderDto.paymentType;
            order.PostFee = 0L; // TODO调用物流信息，根据地址计算邮费
            order.UserId = userInfo.id.ToString(); //设置用户信息
            order.BuyerNick = userInfo.username;
            order.BuyerRate = false; //TODO 卖家为留言
                                     //收货人地址信息，应该从数据库中物流信息中获取，这里使用的是假的数据
            AddressDTO addressDTO = AddressClient.FindById(orderDto.addressId);
            if (addressDTO == null)
            {
                // 商品不存在，抛出异常
                throw new Exception("收货地址不存在");
            }
            order.Receiver = addressDTO.name;
            order.ReceiverAddress = addressDTO.address;
            order.ReceiverCity = addressDTO.city;
            order.ReceiverDistrict = addressDTO.district;
            order.ReceiverMobile = addressDTO.phone;
            order.ReceiverZip = addressDTO.zipCode;
            order.ReceiverState = addressDTO.state;
            //付款金额相关，首先把orderDto转化成map，其中key为skuId,值为购物车中该sku的购买数量
            Dictionary<long, int> skuNumMap = orderDto.carts.ToDictionary(m => m.skuId, m => m.num);
            //查询商品信息，根据skuIds批量查询sku详情
            List<TbSku> skus = _goodsService.QuerySkusByIds(skuNumMap.Keys.ToList());
            if (skus.Count <= 0)
            {
                throw new Exception("查询的商品信息不存在");
            }
            Double totalPay = 0.0;
            //填充orderDetail
            List<TbOrderDetail> orderDetails = new List<TbOrderDetail>();
            //遍历skus，填充orderDetail
            foreach (TbSku sku in skus)
            {
                // 获取购买商品数量
                int num = skuNumMap[sku.Id];
                // 计算金额
                totalPay += num * sku.Price;
                TbOrderDetail orderDetail = new TbOrderDetail();
                orderDetail.OrderId = orderId;
                orderDetail.OwnSpec = sku.OwnSpec;
                orderDetail.SkuId = sku.Id;
                orderDetail.Title = sku.Title;
                orderDetail.Num = num;
                orderDetail.Price = sku.Price;
                // 获取商品展示第一张图片
                orderDetail.Image = sku.Images.Split(',')[0];
                orderDetails.Add(orderDetail);
            }
            order.ActualPay = (long)(totalPay + order.PostFee);  //todo 还要减去优惠金额
            order.TotalPay = (long)totalPay;
            //保存order
            _orangeContext.TbOrder.Add(order);

            //保存detail
            _orangeContext.TbOrderDetail.AddRange(orderDetails);
            //填充orderStatus
            TbOrderStatus orderStatus = new TbOrderStatus();
            orderStatus.OrderId = orderId;
            orderStatus.Status = (int)OrderStatusEnum.INIT;
            orderStatus.CreateTime = DateTime.Now;
            //保存orderStatus
            _orangeContext.TbOrderStatus.AddRange(orderStatus);
            #endregion

            IDbContextTransaction trans = null;
            try
            {
                //减库存（1、下订单减库存，2、支付完成后减库存）
                // TODO 需要处理强一致分布式事务
                //_goodsService.DecreaseStock(orderDto.carts);

                #region 本地数据库
                //trans = this._orangeContext.Database.BeginTransaction();
                //foreach (CartDto cartDto in orderDto.carts)
                //{
                //    int count = _orangeContext.Database.ExecuteSqlRaw($"update tb_stock set stock = stock - {cartDto.num} where sku_id = {cartDto.skuId} and stock >= {cartDto.num}");
                //    if (count != 1)
                //    {
                //        throw new Exception("扣减库存失败");
                //    }
                //}
                //	log.info("生成订单，订单编号：{}，用户id：{}", orderId, user.getId());
                //var isok = _orangeContext.SaveChanges() > 0;
                //trans.Commit();
                #endregion

                #region 数据库拆分后--分布式事务
                trans = this._orangeContext.Database.BeginTransaction(this._iCapPublisher, autoCommit: false);
                this._iCapPublisher.Publish(name: RabbitMQExchangeQueueName.Order_Stock_Decrease,
                    contentObj: new OrderCartDto()
                    {
                        Carts = orderDto.carts,
                        OrderId = order.OrderId
                    }, headers: null);
                this._orangeContext.SaveChanges();
                foreach (var skuIdNum in skuNumMap)
                {
                    string key = $"{CommonConfigConstant.StockRedisKeyPrefix}{skuIdNum.Key}";
                    if (!this._cacheClientDB.ContainsKey(key))
                    {
                        throw new Exception("库存在Redis不存在,需要初始化");
                    }
                    else if (this._cacheClientDB.DecrementValueBy(key, skuIdNum.Value) < 0)
                    {
                        this._cacheClientDB.IncrementValueBy(key, skuIdNum.Value);//订单失败恢复回去
                        throw new Exception("库存在Redis不足,需要检查");
                    }
                    ////需要多项同时减少，支持多key，需要Lua TODO
                }
                trans.Commit();
                Console.WriteLine("数据库业务数据已经插入,操作完成");
                #endregion

                #region 异步清理购物车--模拟异常
                {
                    ////删除购物车中已经下单的商品数据, 采用异步mq的方式通知购物车系统删除已购买的商品，传送商品ID和用户ID---模拟操作失败
                    //Dictionary<string, object> map = new Dictionary<string, object>();
                    //map.Add("skuIds", skuNumMap.Keys);
                    //map.Add("userId", user.id);
                }
                #endregion
            }
            catch (Exception ex)
            {
                if (trans != null)
                {
                    Console.WriteLine(ex);
                    trans.Rollback();
                }
                throw;
            }
            finally
            {
                trans.Dispose();
            }
            #endregion

            #region 异步清理购物车+延时取消任务+确认订单支付状态任务
            {
                //删除购物车中已经下单的商品数据, 采用异步mq的方式通知购物车系统删除已购买的商品，传送商品ID和用户ID---模拟操作失败
                try
                {
                    var orderCreateQueueModel = new OrderCreateQueueModel()
                    {
                        OrderId = order.OrderId,
                        UserId = userInfo.id,
                        SkuIdList = skuNumMap.Keys.ToList(),
                        TryTime = 0,
                        OrderType = OrderCreateQueueModel.OrderTypeEnum.Normal
                    };
                    string message = JsonConvert.SerializeObject(orderCreateQueueModel);
                    //发布清理购物车任务
                    this._RabbitMQInvoker.Send(new RabbitMQConsumerModel() { ExchangeName = RabbitMQExchangeQueueName.OrderCreate_Exchange, QueueName = RabbitMQExchangeQueueName.OrderCreate_Queue_CleanCart },message);

                    //this._RabbitMQInvoker.SendDelay(RabbitMQExchangeQueueName.OrderCreate_Delay_Exchange, message, 60 * 30);
                    //发布延时关闭订单任务
                    this._RabbitMQInvoker.SendDelay(RabbitMQExchangeQueueName.OrderCreate_Delay_Exchange, message, 30);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    this._logger.LogError($"发送异步购物车清理消息失败，OrderId={order.OrderId}，UserId={userInfo.id}");
                }
            }
            #endregion

            return orderId;
        }

        /// <summary>
        /// 更新订单的支付状态
        /// 专门为支付回调更新状态---检测状态发异常通知
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderPayStatus"></param>
        public void UpdateOrderStatus(long orderId, int orderPayStatus)
        {
            TbOrderStatus updateOrderStatus = this._orangeContext.TbOrderStatus.First(o => o.OrderId == orderId);
            updateOrderStatus.Status = orderPayStatus;
            updateOrderStatus.OrderId = orderId;
            updateOrderStatus.PaymentTime = DateTime.Now;
            this._orangeContext.SaveChanges();
        }


        public int? GetOrderStatus(long orderId)
        {
            var orderStatus = this._orangeContext.TbOrderStatus.Find(orderId);
            if (orderStatus == null)
                throw new Exception($"orderId={orderId}的orderStatus 为null");
            return orderStatus.Status;
        }

        public bool CloseOrder(long orderId)
        {
            var orderStatus = this._orangeContext.TbOrderStatus.Find(orderId);
            if (orderStatus == null)
                throw new Exception($"orderId={orderId}的orderStatus 为null");

            orderStatus.Status = (int)OrderStatusEnum.CLOSED;
            orderStatus.CloseTime = DateTime.Now;

            var skuIdList = this._orangeContext.TbOrderDetail.Where(od => od.OrderId == orderId).Select(od => new
            {
                skuId = od.SkuId,
                num = od.Num
            }).ToList();

            IDbContextTransaction trans = null;
            try
            {
                #region 本地数据库
                //trans = this._orangeContext.Database.BeginTransaction();
                //foreach (var item in skuIdList)
                //{
                //    int count = _orangeContext.Database.ExecuteSqlRaw($"update tb_stock set stock = stock + {item.num} where sku_id = {item.skuId} and stock >= {item.num}");
                //    if (count != 1)
                //    {
                //        throw new Exception("恢复库存失败");
                //    }
                //}
                //this._orangeContext.SaveChanges();

                //trans.Commit();
                #endregion

                #region 数据库拆分后--分布式事务
                trans = this._orangeContext.Database.BeginTransaction(this._iCapPublisher, autoCommit: false);
                this._iCapPublisher.Publish(name: RabbitMQExchangeQueueName.Order_Stock_Resume,
                    contentObj: new OrderCartDto()
                    {
                        Carts = skuIdList.Select(sku => new CartDto()
                        {
                            skuId = sku.skuId,
                            num = sku.num
                        }).ToList(),
                        OrderId = orderId
                    }, headers: null);
                this._orangeContext.SaveChanges();
                Console.WriteLine("数据库业务数据已经插入,操作完成");
                trans.Commit();
                #endregion
            }
            catch (Exception ex)
            {
                if (trans != null)
                {
                    Console.WriteLine(ex);
                    trans.Rollback();
                }
                throw;
            }
            finally
            {
                trans.Dispose();
            }
            return true;
        }


        //private string RefreshOrderPayStatus_RedisKeyPrefix = "Check_Order_Pay_Status";
        ///// <summary>
        ///// 基于Redis检查是否需要更新支付状态，
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <returns></returns>
        //public bool CheckIsNeedRefreshOrderPayStatusRedis(long orderId)
        //{
        //    string key = $"{RefreshOrderPayStatus_RedisKeyPrefix}_{orderId}";

        //    if (this._cacheClientDB.ContainsKey(key))//检查是否存在key，存在则不需要更新了
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
        ///// <summary>
        ///// 设置不需要刷新了
        ///// </summary>
        ///// <param name="orderId"></param>
        //public void SetIsNeedRefreshOrderPayStatusRedis(long orderId)
        //{
        //    string key = $"{RefreshOrderPayStatus_RedisKeyPrefix}_{orderId}";
        //    this._cacheClientDB.Set<string>(key, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), DateTime.Now.AddHours(1));
        //}


        ///// <summary>
        ///// 根据微信支付返回的数据更新数据库
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <param name="wxPayData"></param>
        ///// <returns></returns>
        //public bool UpdatePayStatus(long orderId, WxPayData wxPayData)
        //{
        //    #region 先检测数据
        //    long totalFee = long.Parse(wxPayData.GetValue("total_fee").ToString());//订单金额
        //    string transactionId = wxPayData.GetValue("transaction_id").ToString();//商户订单号
        //    string bankType = wxPayData.GetValue("bank_type").ToString();//银行类型

        //    TbOrder tbOrder = this.QueryById(orderId);
        //    if (1 != totalFee)//tbOrder.ActualPay---当下只是做了个假数据比较
        //    {
        //        _logger.LogError("【微信支付回调】支付回调返回数据不正确");
        //        throw new WxPayException("支付参数不正常");
        //    }
        //    #endregion

        //    #region 修改支付日志状态
        //    Console.WriteLine("修改支付日志状态");
        //    TbPayLog payLog = this._orangeContext.TbPayLog.First(l => l.OrderId == orderId);
        //    //未支付的订单才需要更改----重试机制
        //    if (payLog.Status == (int)PayStatusEnum.NOT_PAY)
        //    {
        //        payLog.OrderId = orderId;
        //        payLog.BankType = bankType;
        //        payLog.PayTime = DateTime.Now;
        //        payLog.TransactionId = transactionId;
        //        payLog.Status = (int)PayStatusEnum.SUCCESS;
        //    }
        //    #endregion

        //    #region 修改订单状态
        //    Console.WriteLine("修改订单状态");
        //    TbOrderStatus updateOrderStatus = this._orangeContext.TbOrderStatus.First(o => o.OrderId == orderId);

        //    updateOrderStatus.Status = (int)OrderStatusEnum.PAY_UP;
        //    updateOrderStatus.OrderId = orderId;
        //    updateOrderStatus.PaymentTime = DateTime.Now;
        //    #endregion
        //    // 提交所有更改
        //    _orangeContext.SaveChanges();

        //    return true;
        //}


        public TbOrder QueryById(long orderId)
        {
            TbOrder order = _orangeContext.TbOrder.Where(m => m.OrderId == orderId).FirstOrDefault();
            if (order == null)
            {
                throw new Exception("订单不存在");
            }
            List<TbOrderDetail> orderDetails = _orangeContext.TbOrderDetail.Where(m => m.OrderId == orderId).ToList();
            order.orderDetails = orderDetails;
            TbOrderStatus orderStatus = _orangeContext.TbOrderStatus.Where(m => m.OrderId == orderId).FirstOrDefault();
            order.orderStatus = orderStatus;
            return order;
        }

        public PageResult<TbOrder> QueryOrderByPage(int page, int rows)
        {
            //查询订单
            List<TbOrder> orders = _orangeContext.TbOrder.ToList(); ;
            foreach (var item in orders)
            {
                List<TbOrderDetail> orderDetails = _orangeContext.TbOrderDetail.Where(m => m.OrderId == item.OrderId).ToList();
                item.orderDetails = orderDetails;
                TbOrderStatus orderStatus = _orangeContext.TbOrderStatus.Where(m => m.OrderId == item.OrderId).FirstOrDefault();
                item.orderStatus = orderStatus;
            }
            return new PageResult<TbOrder>(orders.Count, orders);
        }



        ///// <summary>
        ///// 直接去微信拿状态，不管数据库，不管redis
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <returns></returns>
        //public WxPayData QueryOrderStateFromWechatByOrderId(long orderId)
        //{
        //    return this._payHelper.QueryOrderById(orderId);
        //}
    }
}