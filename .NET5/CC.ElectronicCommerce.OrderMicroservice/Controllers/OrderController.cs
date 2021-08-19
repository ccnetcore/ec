using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Model.DTO;
using CC.ElectronicCommerce.WebCore;
using CC.ElectronicCommerce.WebCore.FilterExtend;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.OrderMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        /**
		  * 创建订单
		  *
		  * @param orderDto
		  * @return
		  */
        [HttpPost]
        [Route("/api/order/create")]
        [TypeFilter(typeof(CustomAction2CommitFilterAttribute))]//避免重复提交
        public Result CreateOrder( OrderDto orderDto)
        {
            UserInfo user = base.HttpContext.GetCurrentUserInfo();// sign 签名保证数据不会被篡改
            var orderId = this._orderService.CreateOrder(orderDto, user);
            return Result.Success("下单成功").SetData(new { status = 300, orderId = orderId });
        }


        /**
		 * 根据订单ID查询订单详情
		 *
		 * @param orderId
		 * @return
		 */
        [Route("/api/order/{id}")]
        [HttpGet]
        public Result QueryOrderById(long id)
        {
            var order = _orderService.QueryById(id);

            return Result.Success("查询成功").SetData(order);
         
        }

       

        /**
		 * 分页查询所有订单
		 *
		 * @param page
		 * @param rows
		 * @return
		 */
        [Route("/api/order/list")]
        [HttpGet]
        public Result QueryOrderByPage(int page, int rows)
        {
            PageResult<TbOrder> result = _orderService.QueryOrderByPage(page, rows);
            return Result.Success("查询成功").SetData(result);
        }
    }
}
