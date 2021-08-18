using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model.DTO
{
	public class AddressDTO
	{
		public long id;
		public string name;// 收件人姓名
		public string phone;// 电话
		public string state;// 省份
		public string city;// 城市
		public string district;// 区
		public string address;// 街道地址
		public string zipCode;// 邮编
		public bool isDefault;
	}
}
