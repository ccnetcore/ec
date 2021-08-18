using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model
{
	public class Cart
	{  
		//商品id
		public long skuId { get; set; }

		//商品标题
		public string title { get; set; }

		//购买数量
		public int num { get; set; }

		//商品图片
		public string image { get; set; }

		//加入购物车时商品的价格
		public long price { get; set; }

		//商品的规格参数
		public string ownSpec { get; set; }
	}
}
