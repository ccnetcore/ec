using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Common.QueueModel;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.OrderProcessor
{
    public class CancelOrderWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CancelOrderWorker> _logger;
        private readonly RabbitMQInvoker _RabbitMQInvoker;
        private readonly IOrderService _IOrderService = null;
        private readonly CacheClientDB _cacheClientDB;

        public CancelOrderWorker(ILogger<CancelOrderWorker> logger, RabbitMQInvoker rabbitMQInvoker, IConfiguration configuration, IOrderService orderService, CacheClientDB cacheClientDB)
        {
            this._logger = logger;
            this._RabbitMQInvoker = rabbitMQInvoker;
            this._configuration = configuration;
            this._IOrderService = orderService;
            this._cacheClientDB = cacheClientDB;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RabbitMQConsumerModel rabbitMQConsumerModel = new RabbitMQConsumerModel()
            {
                ExchangeName = RabbitMQExchangeQueueName.OrderCreate_Delay_Exchange,
                QueueName = RabbitMQExchangeQueueName.OrderCreate_Delay_Queue_CancelOrder
            };
            this._RabbitMQInvoker.RegistReciveAction(rabbitMQConsumerModel, message =>
            {
                try
                {

                    OrderCreateQueueModel orderCreateQueueModel = JsonConvert.DeserializeObject<OrderCreateQueueModel>(message);
                    var orderStatus = this._IOrderService.GetOrderStatus(orderCreateQueueModel.OrderId);
                    if (orderStatus != null && orderStatus.Value == (int)OrderStatusEnum.INIT)
                    {
                        bool bResult = this._IOrderService.CloseOrder(orderCreateQueueModel.OrderId);
                        //如果是秒杀订单，关闭订单需增加Redis库存
                        if (orderCreateQueueModel.OrderType == OrderCreateQueueModel.OrderTypeEnum.Seckill)
                        {
                            ////考虑下秒杀已结束的恢复  TODO
                            //this._cacheClientDB.IncrementValueInHash(SeckillService.KEY_PREFIX_STOCK, orderCreateQueueModel.SkuIdList[0].ToString(), 1);
                        }
                        return bResult;
                    }
                    else
                    {
                        this._logger.LogWarning($"{nameof(CancelOrderWorker)}.CancelOrder complate message={message}, 未修改状态，当时状态为{orderStatus}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogModel logModel = new LogModel()
                    {
                        OriginalClassName = this.GetType().FullName,
                        OriginalMethodName = nameof(ExecuteAsync),
                        Remark = "定时作业错误日志"
                    };
                    this._logger.LogError(ex, $"{nameof(CancelOrderWorker)}.CancelOrder failed message={message}, Exception:{ex.Message}", JsonConvert.SerializeObject(logModel));
                    return false;
                }
            });
            await Task.CompletedTask;
        }
    }
}
