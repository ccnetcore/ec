using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace  CC.ElectronicCommerce.Service
{
	public class CartService : ICartService
	{
		private CacheClientDB _cacheClientDB;
		public CartService(CacheClientDB cacheClientDB)
		{
			_cacheClientDB = cacheClientDB;
		}
		private static readonly string KEY_PREFIX = "yt:cart:uid:";
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cart"></param>
		/// <param name="loginUser"></param>
		public void AddCart(Cart cart, UserInfo loginUser)
		{
			//获取用户信息
			string key = KEY_PREFIX + loginUser.id;
			//获取商品ID
			string hashKey = cart.skuId.ToString();
			//获取数量
			int num = cart.num;
			//获取hash操作的对象
			var hashOps = _cacheClientDB.GetHashKeys(key);
			if (hashOps.Contains(hashKey))
			{
				cart = _cacheClientDB.GetValueFromHash<Cart>(key, hashKey);
				cart.num = num + cart.num;
			}
			_cacheClientDB.SetEntryInHash(key, hashKey, cart);
		}
		public void AddCarts(List<Cart> carts, UserInfo loginUser)
		{
			foreach (Cart cart in carts)
			{
				AddCart(cart, loginUser);
			}
		}

		public void DeleteCart(long id, UserInfo loginUser)
		{
			string key = KEY_PREFIX + loginUser.id;
			var hashOps = _cacheClientDB.GetHashKeys(key);
			if (!hashOps.Contains(id.ToString()))
			{
				//该商品不存在
				throw new Exception("购物车商品不存在, 用户：" + loginUser.id + ", 商品：" + id);
			}
			//删除商品
			_cacheClientDB.RemoveEntryFromHash(key, id.ToString());
		}

		public void DeleteCarts(List<long> ids, long userId)
		{
			string key = KEY_PREFIX + userId;
			var hashOps = _cacheClientDB.GetHashKeys(key);
			foreach (var item in ids)
			{
				if (hashOps.Contains(item.ToString()))
				{
					//删除商品
					_cacheClientDB.RemoveEntryFromHash(key, item.ToString());
				}
			}
		}

		public List<Cart> ListCart(UserInfo loginUser)
		{  //获取该用户Redis中的key
			string key = KEY_PREFIX + loginUser.id;
			if (!_cacheClientDB.ContainsKey(key))
			{ //Redis中没有给用户信息
				return null;
			}
			var hashOps = _cacheClientDB.GetHashValues(key);
			if (hashOps == null || hashOps.Count <= 0)
			{
				//购物车中无数据
			}
			List<Cart> carts = new List<Cart>();
			foreach (var item in hashOps)
			{
				carts.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Cart>(item));
			}
			return carts;
		}

		public void UpdateNum(long id, int num, UserInfo loginUser)
		{
			//获取该用户Redis中的key
			string key = KEY_PREFIX + loginUser.id;
			var hashOps = _cacheClientDB.GetHashKeys(key);
			if (!hashOps.Contains(id.ToString()))
			{
				//该商品不存在
				throw new Exception("购物车商品不存在, 用户：" + loginUser.id + ", 商品：" + id);
			}
			//查询购物车商品
			var cart = _cacheClientDB.GetValueFromHash<Cart>(key, id.ToString());
			//修改数量
			cart.num = num;
			// 回写Redis
			_cacheClientDB.SetEntryInHash(key, id.ToString(), cart);
		}
	}
}
