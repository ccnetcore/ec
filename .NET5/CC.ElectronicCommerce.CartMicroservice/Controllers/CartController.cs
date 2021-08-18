using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CC.ElectronicCommerce.WebCore;

namespace CC.ElectronicCommerce.CartMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        /**
		 * 添加商品到购物车
		 *
		 * @param cart
		 * @return
		 */
        [Route("add")]
        [Authorize]
        [HttpPost]
        public Result AddCart( Cart cart)
        {
            UserInfo user = base.HttpContext.GetCurrentUserInfo();
            _cartService.AddCart(cart, user);
            return Result.Success("添加成功");
        }

        /**
         * 批量添加商品到购物车
         *
         */
        [Authorize]
        [Route("batch")]
        [HttpPost]
        public Result AddCartBatch( List<Cart> carts)
        {
            UserInfo user = base.HttpContext.GetCurrentUserInfo();
            _cartService.AddCarts(carts, user);
            return Result.Success("添加成功");
        }


        /**
         * 从购物车中删除商品
         *
         * @param id
         * @return
         */
        [Authorize]
        [Route("{id}")]
        [HttpDelete]
        public Result DeleteCart(long id)
        {
            UserInfo user = base.HttpContext.GetCurrentUserInfo();
            _cartService.DeleteCart(id, user);
            return Result.Success("删除成功");
        }



        /**
		 * 更新购物车中商品的数量
		 *
		 * @param id  商品ID
		 * @param num 修改后的商品数量
		 * @return
		 */

        [HttpPut]
        [Authorize]
        [Route("update")]
        public Result UpdateNum( long id, int num)
        {
            UserInfo user = base.HttpContext.GetCurrentUserInfo();
            _cartService.UpdateNum(id, num, user);
            return Result.Success("更新成功");
        }


        /**
		 * 查询购物车
		 *
		 * @return
		 */
        [Route("list")]
        [HttpGet]
        [Authorize]
        public Result ListCart()
        {
            UserInfo user = base.HttpContext.GetCurrentUserInfo();
            var cartList = _cartService.ListCart(user);
            return Result.Success("查询成功").SetData(cartList);

        }
    }
}
