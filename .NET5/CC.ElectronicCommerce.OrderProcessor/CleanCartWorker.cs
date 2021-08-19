using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Common.QueueModel;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    /// <summary>
    /// 清理购物车里面的商品
    /// </summary>
    public class CleanCartWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CleanCartWorker> _logger;
        private readonly RabbitMQInvoker _RabbitMQInvoker;
        private readonly ICartService _ICartService = null;

        public CleanCartWorker(ILogger<CleanCartWorker> logger, RabbitMQInvoker rabbitMQInvoker, IConfiguration configuration, ICartService cartService)
        {
            this._logger = logger;
            this._RabbitMQInvoker = rabbitMQInvoker;
            this._configuration = configuration;
            this._ICartService = cartService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RabbitMQConsumerModel rabbitMQConsumerModel = new RabbitMQConsumerModel()
            {
                ExchangeName = RabbitMQExchangeQueueName.OrderCreate_Exchange,
                QueueName = RabbitMQExchangeQueueName.OrderCreate_Queue_CleanCart
            };
            HttpClient _HttpClient = new HttpClient();
            this._RabbitMQInvoker.RegistReciveAction(rabbitMQConsumerModel, message =>
            {
                try
                {
                    OrderCreateQueueModel orderCreateQueueModel = JsonConvert.DeserializeObject<OrderCreateQueueModel>(message);
                    this._ICartService.DeleteCarts(orderCreateQueueModel.SkuIdList, orderCreateQueueModel.UserId);

                    this._logger.LogInformation($"{nameof(CleanCartWorker)}.Clean Cart succeed {message}");
                    return true;
                }
                catch (Exception ex)
                {
                    LogModel logModel = new LogModel()
                    {
                        OriginalClassName = this.GetType().FullName,
                        OriginalMethodName = nameof(ExecuteAsync),
                        Remark = "定时作业错误日志"
                    };
                    this._logger.LogError(ex, $"{nameof(CleanCartWorker)}.Init ESIndex failed message={message}, Exception:{ex.Message}", JsonConvert.SerializeObject(logModel));
                    return false;
                }
            });
            await Task.CompletedTask;
        }
    }
}
