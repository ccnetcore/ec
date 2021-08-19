using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model
{
	public enum OrderStatusEnum
	{
		  
		//"初始化，未付款"
		INIT = 1,
		//"已付款，未发货"
		PAY_UP = 2,
		//"已发货，未确认"
		DELIVERED = 3,
		//已确认,未评价
		CONFIRMED = 4,
		//"已关闭"
		CLOSED = 5,
		//"已评价，交易结束"
		RATED = 6

		 
}
}
