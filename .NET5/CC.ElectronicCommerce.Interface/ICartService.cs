using CC.ElectronicCommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Interface
{
	public interface ICartService
	{
		/**
     * 添加到购物车
     * @param cart
     */
		void AddCart(Cart cart, UserInfo user);

		/**
         * 批量添加商品到购物
         * @param carts
         * @param loginUser
         */
		void AddCarts(List<Cart> carts, UserInfo loginUser);

		/**
         * 查询购物车
         * @return
         */
		List<Cart> ListCart(UserInfo user);

		/**
         * 根据id更新商品数量
         * @param id
         * @param num
         */
		void UpdateNum(long id, int num, UserInfo user);

		/**
         * 删除购物车商品
         * @param id
         */
		void DeleteCart(long id, UserInfo user);

		/**
         * 批量删除购物车商品
         * @param ids
         * @param userId
         */
		void DeleteCarts(List<long> ids, long userId);
	}
}
