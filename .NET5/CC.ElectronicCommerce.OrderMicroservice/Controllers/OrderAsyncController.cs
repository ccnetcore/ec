using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Model.DTO;
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

namespace CC.ElectronicCommerce.OrderMicroservice.Controllers
{
    /// <summary>
    /// 分布式事务
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAsyncController : ControllerBase
    {
        #region Identity
        private readonly IOrderService _IOrderService;
        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly OrangeContext _OrangeContext;
        private readonly ILogger<OrderAsyncController> _Logger;
        public OrderAsyncController(IConfiguration configuration, OrangeContext orangeContext, ILogger<OrderAsyncController> logger, ICapPublisher capPublisher, IOrderService stockService)
        {
            this._iCapPublisher = capPublisher;
            this._iConfiguration = configuration;
            this._OrangeContext = orangeContext;
            this._Logger = logger;
            this._IOrderService = stockService;
        }
        #endregion


        #region 异步更新订单状态
        [NonAction]
        [CapSubscribe(RabbitMQExchangeQueueName.Pay_Order_UpdateStatus)]
        public void UpdateOrderStatus(PayOrderStatusDto payOrderStatusDto, [FromCap] CapHeader header)
        {
            try
            {
                Console.WriteLine($@"{DateTime.Now} UpdateOrderStatus invoked, Info: {Newtonsoft.Json.JsonConvert.SerializeObject(payOrderStatusDto)}");
                using (var trans = this._OrangeContext.Database.BeginTransaction(this._iCapPublisher, autoCommit: false))
                {
                    this._IOrderService.UpdateOrderStatus(payOrderStatusDto.OrderId, payOrderStatusDto.PayStatus);
                    this._OrangeContext.SaveChanges();
                    trans.Commit();
                }
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
