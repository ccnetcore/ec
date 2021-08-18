using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Common.QueueModel;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
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

namespace CC.ElectronicCommerce.ElasticSearchProcessor
{
    public class WarmupESIndexWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WarmupESIndexWorker> _logger;
        private readonly RabbitMQInvoker _RabbitMQInvoker;
        private readonly IElasticSearchService _IElasticSearchService;
        //private readonly IGoodsService _IGoodsService = null;
        private readonly ISearchService _ISearchService = null;
        private readonly IOptionsMonitor<ElasticSearchOptions> _ElasticSearchOptions = null;

        public WarmupESIndexWorker(ILogger<WarmupESIndexWorker> logger, RabbitMQInvoker rabbitMQInvoker, IConfiguration configuration, IElasticSearchService elasticSearchService,/* IGoodsService goodsService, */ISearchService searchService, IOptionsMonitor<ElasticSearchOptions> optionsMonitor)
        {
            this._logger = logger;
            this._RabbitMQInvoker = rabbitMQInvoker;
            this._configuration = configuration;
            this._IElasticSearchService = elasticSearchService;
            //this._IGoodsService = goodsService;
            this._ISearchService = searchService;
            this._ElasticSearchOptions = optionsMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RabbitMQConsumerModel rabbitMQConsumerModel = new RabbitMQConsumerModel()
            {
                ExchangeName = RabbitMQExchangeQueueName.SKUWarmup_Exchange,
                QueueName = RabbitMQExchangeQueueName.SKUWarmup_Queue_ESIndex
            };
            HttpClient _HttpClient = new HttpClient();
            this._RabbitMQInvoker.RegistReciveAction(rabbitMQConsumerModel, message =>
            {
                SKUWarmupQueueModel skuWarmupQueueModel = JsonConvert.DeserializeObject<SKUWarmupQueueModel>(message);
                #region 先删除Index---新建Index---再建立全部数据索引
                {
                    try
                    {
                        this._IElasticSearchService.DropIndex(this._ElasticSearchOptions.CurrentValue.IndexName);
                        this._ISearchService.ImpDataBySpu();

                        this._logger.LogInformation($"{nameof(WarmupESIndexWorker)}.InitAll succeed");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        var logModel = new LogModel()
                        {
                            OriginalClassName = this.GetType().FullName,
                            OriginalMethodName = nameof(ExecuteAsync),
                            Remark = "定时作业错误日志"
                        };
                        this._logger.LogError(ex, $"{nameof(WarmupESIndexWorker)}.Warmup ESIndex failed message={message}, Exception:{ex.Message}", JsonConvert.SerializeObject(logModel));
                        return false;
                    }
                }
                #endregion
            });
            await Task.CompletedTask;
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
    }
}
