using CC.ElectronicCommerce.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CC.ElectronicCommerce.Model
{
	public class AddressClient
	{
		public static readonly List<AddressDTO> addressList = new List<AddressDTO>()
		{new  AddressDTO(){
		id=1L,
		 address="武汉市洪山区凯乐桂圆 W号楼",
		   city="武汉",
			district="洪山区",
			name="gerry",
			phone="15855500000",
			state="武汉",
		zipCode="100010",
		isDefault=true
		},
		new  AddressDTO()
		{
				id=1L,
		 address="武汉市洪山区凯乐桂圆 W2号楼",
		   city="武汉",
			district="洪山区",
			name="gerry",
			phone="1234569877",
			state="武汉",
		zipCode="100010",
		isDefault=true
	}
};

		public static AddressDTO FindById(long id)
		{
			foreach (AddressDTO addressDTO in addressList)
			{
				if (addressDTO.id == id) return addressDTO;
			}
			return null;
		}
	}
}
