using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CC.ElectronicCommerce.Model
{
	public partial class TbSku
	{
		public long Id { get; set; }
		public long SpuId { get; set; }
		public string Title { get; set; }
		public string Images { get; set; }
		public long Price { get; set; }
		public string Indexes { get; set; }
		public string OwnSpec { get; set; }
		public bool? Enable { get; set; }
		public DateTime CreateTime { get; set; }
		public DateTime LastUpdateTime { get; set; }
		[NotMapped]
		public int Stock { get; set; }// 库存
	}
}
