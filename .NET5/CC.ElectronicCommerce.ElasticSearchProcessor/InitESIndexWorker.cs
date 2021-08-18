using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Common.QueueModel;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model.Search;
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

namespace CC.ElectronicCommerce.ElasticSearchProcessor
{
    public class InitESIndexWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<InitESIndexWorker> _logger;
        private readonly RabbitMQInvoker _RabbitMQInvoker;
        private readonly IElasticSearchService _IElasticSearchService;
        private readonly IGoodsService _IGoodsService = null;
        private readonly ISearchService _ISearchService = null;

        public InitESIndexWorker(ILogger<InitESIndexWorker> logger, RabbitMQInvoker rabbitMQInvoker, IConfiguration configuration, IElasticSearchService elasticSearchService, IGoodsService goodsService, ISearchService searchService)
        {
            this._logger = logger;
            this._RabbitMQInvoker = rabbitMQInvoker;
            this._configuration = configuration;
            this._IElasticSearchService = elasticSearchService;
            this._IGoodsService = goodsService;
            this._ISearchService = searchService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RabbitMQConsumerModel rabbitMQConsumerModel = new RabbitMQConsumerModel()
            {
                ExchangeName = RabbitMQExchangeQueueName.SKUCQRS_Exchange,
                QueueName = RabbitMQExchangeQueueName.SKUCQRS_Queue_ESIndex
            };
            HttpClient _HttpClient = new HttpClient();
            this._RabbitMQInvoker.RegistReciveAction(rabbitMQConsumerModel, message =>
            {
                try
                {
                    SPUCQRSQueueModel spuCQRSQueueModel = JsonConvert.DeserializeObject<SPUCQRSQueueModel>(message);

                    switch (spuCQRSQueueModel.CQRSType)
                    {
                        case (int)SPUCQRSQueueModelType.Insert:
                        case (int)SPUCQRSQueueModelType.Update:
                            {
                                Goods goods = this._ISearchService.GetGoodsBySpuId(spuCQRSQueueModel.SpuId);
                                this._IElasticSearchService.InsertOrUpdata<Goods>(goods);
                                break;
                            }
                        case (int)SPUCQRSQueueModelType.Delete:
                            this._IElasticSearchService.Delete<Goods>(spuCQRSQueueModel.SpuId.ToString());
                            break;
                        default:
                            throw new Exception("wrong spuCQRSQueueModel.CQRSType");
                    }

                    this._logger.LogInformation($"{nameof(InitESIndexWorker)}.Init ESIndex succeed SpuId={spuCQRSQueueModel.SpuId}");
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
                    this._logger.LogError(ex, $"{nameof(InitESIndexWorker)}.Init ESIndex failed message={message}, Exception:{ex.Message}", JsonConvert.SerializeObject(logModel));
                    return false;
                }
            });
            await Task.CompletedTask;
        }
    }
}
