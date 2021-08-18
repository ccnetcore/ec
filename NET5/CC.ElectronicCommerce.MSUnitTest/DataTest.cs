
using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common.QueueModel;
using CC.ElectronicCommerce.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;


namespace CC.ElectronicCommerce.MSUnitTest
{
    [TestClass]
    public class DataTest
    {
        //    private ServiceProvider _ServiceProvider = null;

        //    [TestInitialize]
        //    public void Init()
        //    {
        //        Console.WriteLine("This is DataTest Init");
        //        IServiceCollection services = new ServiceCollection();


        //        //Optionע��

        //        this._ServiceProvider = services.BuildServiceProvider();
        //    }


        //    [TestMethod]
        //    public void TestStaticFileCreate()
        //    {
        //        try
        //        {
        //            var spuIdList = InitData.InitInsert(5);
        //            spuIdList.ForEach(id =>
        //            {
        //                Console.WriteLine($"����SpuId={id}");
        //            });

        //            RabbitMQInvoker rabbitMQInvoker = new RabbitMQInvoker("192.168.3.254");

        //            spuIdList.ForEach(id =>
        //            {
        //                rabbitMQInvoker.Send(RabbitMQExchangeQueueName.SKUCQRS_Exchange, JsonConvert.SerializeObject(new SPUCQRSQueueModel()
        //                {
        //                    SpuId = id,
        //                    CQRSType = (int)SPUCQRSQueueModelType.Insert
        //                }));
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }

        //    [TestMethod]
        //    public void TestStaticFileUpdate()
        //    {
        //        try
        //        {
        //            var spuIdList = InitData.InitInsert(1);
        //            RabbitMQInvoker rabbitMQInvoker = new RabbitMQInvoker("192.168.3.254");
        //            //������
        //            rabbitMQInvoker.Send(RabbitMQExchangeQueueName.SKUCQRS_Exchange, JsonConvert.SerializeObject(new SPUCQRSQueueModel()
        //            {
        //                SpuId = spuIdList[0],
        //                CQRSType = (int)SPUCQRSQueueModelType.Insert
        //            }));

        //            //Ȼ�����
        //            long spuId = InitData.InitUpdate(spuIdList[0]);
        //            spuIdList.ForEach(id =>
        //            {
        //                rabbitMQInvoker.Send(RabbitMQExchangeQueueName.SKUCQRS_Exchange, JsonConvert.SerializeObject(new SPUCQRSQueueModel()
        //                {
        //                    SpuId = id,
        //                    CQRSType = (int)SPUCQRSQueueModelType.Update
        //                }));
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }

        //    [TestMethod]
        //    public void TestStaticFileDelete()
        //    {
        //        try
        //        {
        //            var spuIdList = InitData.InitInsert(1);
        //            RabbitMQInvoker rabbitMQInvoker = new RabbitMQInvoker("192.168.3.254");
        //            //������
        //            rabbitMQInvoker.Send(RabbitMQExchangeQueueName.SKUCQRS_Exchange, JsonConvert.SerializeObject(new SPUCQRSQueueModel()
        //            {
        //                SpuId = spuIdList[0],
        //                CQRSType = (int)SPUCQRSQueueModelType.Insert
        //            }));

        //            //��ɾ��
        //            long spuId = InitData.InitDelete(spuIdList[0]);
        //            spuIdList.ForEach(id =>
        //            {
        //                rabbitMQInvoker.Send(RabbitMQExchangeQueueName.SKUCQRS_Exchange, JsonConvert.SerializeObject(new SPUCQRSQueueModel()
        //                {
        //                    SpuId = id,
        //                    CQRSType = (int)SPUCQRSQueueModelType.Delete
        //                }));
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        [TestMethod]
        public void TestStaticFileInit()
        {
            try
            {
                RabbitMQInvoker rabbitMQInvoker = new RabbitMQInvoker("192.168.2.128");
                rabbitMQInvoker.Send(new RabbitMQConsumerModel() { ExchangeName = RabbitMQExchangeQueueName.SKUWarmup_Exchange, QueueName = RabbitMQExchangeQueueName.SKUWarmup_Queue_StaticPage }, JsonConvert.SerializeObject(new SKUWarmupQueueModel()
                {
                    Warmup = true
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //[TestMethod]
        //public void TestWarmup()
        //{
        //    try
        //    {
        //        RabbitMQInvoker rabbitMQInvoker = new RabbitMQInvoker("192.168.2.128");
        //        rabbitMQInvoker.Send(new RabbitMQConsumerModel() { ExchangeName= RabbitMQExchangeQueueName.SKUWarmup_Exchange ,QueueName=RabbitMQExchangeQueueName.SKUWarmup_Queue_ESIndex }, JsonConvert.SerializeObject(new SKUWarmupQueueModel()
        //        {
        //            Warmup = true
        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}


        //[TestMethod]
        //public void TestOther()
        //{
        //    Console.WriteLine("**********************TestOther***********************");
        //    int i = 3;
        //    int k = 5;
        //    Assert.IsTrue(i < k);

        //    Assert.Equals(i + 2, k);

        //    throw new AssertFailedException();
        //}
        //[TestMethod]
        //public void TestELK()
        //{
        //    KafkaOptions kafkaOptions = new KafkaOptions()
        //    {
        //        TopicName = "kafkalog",
        //        BrokerList = "192.168.3.202:9092"
        //    };
        //    //ConfulentKafka confulentKafka = new ConfulentKafka(kafkaOptions);
        //    //confulentKafka.Produce(JsonConvert.SerializeObject(kafkaOptions)).Wait();
        //}
        //[TestMethod]
        //public void TestLog4Kafka()
        //{
        //    ILogger logger = new LoggerFactory()
        //                   .AddLog4Net()
        //                   .CreateLogger(nameof(DataTest));
        //    logger.LogInformation("����һ����ͨ��־");
        //    logger.LogDebug("����һ�� Debug ��־");
        //    logger.LogWarning("����һ��������־");
        //    logger.LogError("����һ��������־");
        //}

        //[TestMethod]
        //public void TestDelayQueue()
        //{
        //    try
        //    {
        //        RabbitMQInvoker rabbitMQInvoker = new RabbitMQInvoker("192.168.3.254");
        //        rabbitMQInvoker.SendDelay(RabbitMQExchangeQueueName.OrderCreate_Delay_Exchange, JsonConvert.SerializeObject(new OrderCreateQueueModel()
        //        {

        //        }), 15);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}
    }
}