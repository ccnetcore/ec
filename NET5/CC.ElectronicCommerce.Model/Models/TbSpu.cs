using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace CC.ElectronicCommerce.Model
{
	public partial class TbSpu
	{
		public long Id { get; set; }
		public string Title { get; set; }
		public string SubTitle { get; set; }
		public long Cid1 { get; set; }
		public long Cid2 { get; set; }
		public long Cid3 { get; set; }
		public long BrandId { get; set; }
		public bool? Saleable { get; set; }
		public bool? Valid { get; set; }
		public DateTime? CreateTime { get; set; }
		public DateTime? LastUpdateTime { get; set; }

		[NotMapped]
		public string Cname { get; set; }

		[NotMapped]
		public string Bname { get; set; }

		[NotMapped]
		public TbSpuDetail SpuDetail { get; set; }

		[NotMapped]
		public List<TbSku> Skus { get; set; }

		[NotMapped]
		public string Skusstr { get; set; }
	}
}
