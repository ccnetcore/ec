using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Model.DTO;
using CC.ElectronicCommerce.StockInterface;
using CC.ElectronicCommerce.StockModel;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.StockMicroservice.Controllers
{
    /// <summary>
    /// 分布式事务
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        #region Identity
        private readonly IStockService _iStockService;
        private readonly IStockManagerService _IStockManagerService;
        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly OrangeStockContext _OrangeStockContext;
        private readonly ILogger<StockController> _Logger;
        public StockController(IConfiguration configuration, OrangeStockContext userServiceDbContext, ILogger<StockController> logger, ICapPublisher capPublisher, IStockService stockService, IStockManagerService stockManagerService)
        {
            this._iCapPublisher = capPublisher;
            this._iConfiguration = configuration;
            this._OrangeStockContext = userServiceDbContext;
            this._Logger = logger;
            this._iStockService = stockService;
            this._IStockManagerService = stockManagerService;
        }
        #endregion

        [Route("test")]//测试发送队列，减少库存或者恢复库存
        [HttpGet]
        public Result Test(int index)
        {
            OrderCartDto orderCartDto = new OrderCartDto()
            {
                Carts = new List<CartDto>()
                {
                    new CartDto()
                    {
                        skuId=2600242,
                        num=10
                    },
                    new CartDto()
                    {
                        skuId=2600248,
                        num=10
                    }
                },
                OrderId = 1234567777
            };

            if (index == 1)
            {
                this._iCapPublisher.Publish(name: RabbitMQExchangeQueueName.Order_Stock_Decrease, contentObj: orderCartDto, headers: null);
            }
            else if (index == 2)
            {
                this._iCapPublisher.Publish(name: RabbitMQExchangeQueueName.Order_Stock_Resume, contentObj: orderCartDto, headers: null);
            }

            //this._iStockService.DecreaseStock(orderCartDto.carts, orderCartDto.OrderId);

            //this._iStockService.ResumeStock(orderCartDto.carts, orderCartDto.OrderId);

            return Result.Success();
        }

        
        [HttpGet]
        [Route("init/{skuId}")]
        public Result InitStock(long skuId)
        {
            this._IStockManagerService.ForceInitRedisStockBySkuId(skuId);
            return Result.Success("更新成功");
        }


        #region 下单减库存
        [NonAction]
        [CapSubscribe(RabbitMQExchangeQueueName.Order_Stock_Decrease)]
        public void DecreaseStockByOrder(OrderCartDto orderCartDto, [FromCap] CapHeader header)
        {
            try
            {
                Console.WriteLine($@"{DateTime.Now} DecreaseStockByOrder invoked, Info: {Newtonsoft.Json.JsonConvert.SerializeObject(orderCartDto)}");
                using (var trans = this._OrangeStockContext.Database.BeginTransaction(this._iCapPublisher, autoCommit: false))
                {
                    this._iStockService.DecreaseStock(orderCartDto.Carts, orderCartDto.OrderId);
                    this._iCapPublisher.Publish(name: RabbitMQExchangeQueueName.Stock_Logistics, contentObj: orderCartDto, headers: null);
                    this._OrangeStockContext.SaveChanges();
                    Console.WriteLine("数据库业务数据已经插入,操作完成");
                    trans.Commit();
                }
                this._Logger.LogWarning($"This is EFCoreTransaction Invoke");
            }
            catch (Exception ex)
            {
                Console.WriteLine("****************************************************");
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        #endregion

        #region 取消订单恢复库存
        [NonAction]
        [CapSubscribe(RabbitMQExchangeQueueName.Order_Stock_Resume)]
        public void ResumeStockByOrder(OrderCartDto orderCartDto, [FromCap] CapHeader header)
        {
            try
            {
                Console.WriteLine($@"{DateTime.Now} ResumeStockByOrder invoked, Info: {Newtonsoft.Json.JsonConvert.SerializeObject(orderCartDto)}");
                this._iStockService.ResumeStock(orderCartDto.Carts, orderCartDto.OrderId);
                Console.WriteLine("数据库业务数据已经插入,操作完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine("****************************************************");
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        #endregion


    }
}
