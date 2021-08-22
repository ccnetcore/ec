using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model
{
	public class SeckillDTO
	{
		public SeckillDTO(UserInfo UserInfo, SeckillGoods SeckillGoods)
		{
			userInfo = UserInfo;
			seckillGoods = SeckillGoods;
		}
		/**
     * 用户信息
     */
		public UserInfo userInfo { get; set; }

		/**
         * 秒杀商品
         */
		public SeckillGoods seckillGoods { get; set; }
	}
}
