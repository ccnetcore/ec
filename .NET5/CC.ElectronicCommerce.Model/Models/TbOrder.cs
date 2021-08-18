using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CC.ElectronicCommerce.Model
{
	public partial class TbOrder
	{
		public long OrderId { get; set; }
		public long TotalPay { get; set; }
		public long ActualPay { get; set; }
		public string PromotionIds { get; set; }
		public byte PaymentType { get; set; }
		public long PostFee { get; set; }
		public DateTime? CreateTime { get; set; }
		public string ShippingName { get; set; }
		public string ShippingCode { get; set; }
		public string UserId { get; set; }
		public string BuyerMessage { get; set; }
		public string BuyerNick { get; set; }
		public bool? BuyerRate { get; set; }
		public string ReceiverState { get; set; }
		public string ReceiverCity { get; set; }
		public string ReceiverDistrict { get; set; }
		public string ReceiverAddress { get; set; }
		public string ReceiverMobile { get; set; }
		public string ReceiverZip { get; set; }
		public string Receiver { get; set; }
		public int? InvoiceType { get; set; }
		public int? SourceType { get; set; }

		[NotMapped]
		public TbOrderStatus orderStatus = new TbOrderStatus();// 订单状态

		[NotMapped]
		public List<TbOrderDetail> orderDetails = new List<TbOrderDetail>(); // 订单中所有商品详情信息集合
	}
}
