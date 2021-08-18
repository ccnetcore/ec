using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using CC.ElectronicCommerce.Common.IOCOptions;

namespace CC.ElectronicCommerce.Model
{
	public partial class OrangeContext : DbContext
	{
		private readonly IOptionsMonitor<MySqlConnOptions>  _optionsMonitor;
		private readonly string _connStr;

		public OrangeContext(IOptionsMonitor<MySqlConnOptions> optionsMonitor)
		{
			_optionsMonitor = optionsMonitor;
			_connStr = _optionsMonitor.CurrentValue.Url;
		}

		

		public OrangeContext(string connstr)
		{
			_connStr = connstr;
		}

		//public OrangeContext(DbContextOptions<OrangeContext> options)
		//	: base(options)
		//{
		//	//this.Database.l
		//}

		public virtual DbSet<Cid3> Cid3 { get; set; }
		public virtual DbSet<TbBrand> TbBrand { get; set; }
		public virtual DbSet<TbCategory> TbCategory { get; set; }
		public virtual DbSet<TbCategoryBrand> TbCategoryBrand { get; set; }
		public virtual DbSet<TbOrder> TbOrder { get; set; }
		public virtual DbSet<TbOrderDetail> TbOrderDetail { get; set; }
		public virtual DbSet<TbOrderStatus> TbOrderStatus { get; set; }
		//public virtual DbSet<TbPayLog> TbPayLog { get; set; }
		public virtual DbSet<TbSeckillOrder> TbSeckillOrder { get; set; }
		public virtual DbSet<TbSeckillSku> TbSeckillSku { get; set; }
		public virtual DbSet<TbSku> TbSku { get; set; }
		public virtual DbSet<TbSpecGroup> TbSpecGroup { get; set; }
		public virtual DbSet<TbSpecParam> TbSpecParam { get; set; }
		public virtual DbSet<TbSpu> TbSpu { get; set; }
		public virtual DbSet<TbSpuDetail> TbSpuDetail { get; set; }
        public virtual DbSet<TbStock> TbStock { get; set; }
        public virtual DbSet<TbUser> TbUser { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseMySql(this._connStr,new MySqlServerVersion(new Version(8, 0, 21)));

			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Cid3>(entity =>
			{
				entity.HasNoKey();

				entity.ToView("cid3");

				entity.Property(e => e.ParentId)
					.HasColumnName("parent_id")
					.HasColumnType("bigint(20)")
					.HasComment("父类目id,顶级类目填0");
			});

			modelBuilder.Entity<TbBrand>(entity =>
			{
				entity.ToTable("tb_brand");

				entity.HasComment("品牌表，一个品牌下有多个商品（spu），一对多关系");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("品牌id");

				entity.Property(e => e.Image)
				
					.HasColumnName("image")
					.HasColumnType("varchar(128)")
					.HasDefaultValueSql("''")
					.HasComment("品牌图片地址")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Letter)
					.HasColumnName("letter")
					.HasColumnType("char(1)")
					.HasDefaultValueSql("''")
					.HasComment("品牌的首字母")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasColumnName("name")
					.HasColumnType("varchar(32)")
					.HasComment("品牌名称")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			modelBuilder.Entity<TbCategory>(entity =>
			{
				entity.ToTable("tb_category");

				entity.HasComment("商品类目表，类目和商品(spu)是一对多关系，类目与品牌是多对多关系");

				entity.HasIndex(e => e.ParentId)
					.HasDatabaseName("key_parent_id");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("类目id");

				entity.Property(e => e.IsParent)
					.HasColumnName("is_parent")
					.HasComment("是否为父节点，0为否，1为是");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasColumnName("name")
					.HasColumnType("varchar(32)")
					.HasComment("类目名称")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.ParentId)
					.HasColumnName("parent_id")
					.HasColumnType("bigint(20)")
					.HasComment("父类目id,顶级类目填0");

				entity.Property(e => e.Sort)
					.HasColumnName("sort")
					.HasColumnType("int(4)")
					.HasComment("排序指数，越小越靠前");
			});

			modelBuilder.Entity<TbCategoryBrand>(entity =>
			{
				entity.HasKey(e => new { e.CategoryId, e.BrandId })
					.HasName("PRIMARY");

				entity.ToTable("tb_category_brand");

				entity.HasComment("商品分类和品牌的中间表，两者是多对多关系");

				entity.Property(e => e.CategoryId)
					.HasColumnName("category_id")
					.HasColumnType("bigint(20)")
					.HasComment("商品类目id");

				entity.Property(e => e.BrandId)
					.HasColumnName("brand_id")
					.HasColumnType("bigint(20)")
					.HasComment("品牌id");
			});

			modelBuilder.Entity<TbOrder>(entity =>
			{
				entity.HasKey(e => e.OrderId)
					.HasName("PRIMARY");

				entity.ToTable("tb_order");

				entity.HasIndex(e => e.BuyerNick)
					.HasDatabaseName("buyer_nick");

				entity.HasIndex(e => e.CreateTime)
					.HasDatabaseName("create_time");

				entity.Property(e => e.OrderId)
					.HasColumnName("order_id")
					.HasColumnType("bigint(20)")
					.HasComment("订单id");

				entity.Property(e => e.ActualPay)
					.HasColumnName("actual_pay")
					.HasColumnType("bigint(20)")
					.HasComment("实付金额。单位:分。如:20007，表示:200元7分");

				entity.Property(e => e.BuyerMessage)
					.HasColumnName("buyer_message")
					.HasColumnType("varchar(128)")
					.HasComment("买家留言")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.BuyerNick)
					.IsRequired()
					.HasColumnName("buyer_nick")
					.HasColumnType("varchar(32)")
					.HasComment("买家昵称")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.BuyerRate)
					.HasColumnName("buyer_rate")
					.HasComment("买家是否已经评价,0未评价，1已评价");

				entity.Property(e => e.CreateTime)
					.HasColumnName("create_time")
					.HasColumnType("datetime")
					.HasComment("订单创建时间");

				entity.Property(e => e.InvoiceType)
					.HasColumnName("invoice_type")
					.HasColumnType("int(1)")
					.HasDefaultValueSql("'0'")
					.HasComment("发票类型(0无发票1普通发票，2电子发票，3增值税发票)");

				entity.Property(e => e.PaymentType)
					.HasColumnName("payment_type")
					.HasColumnType("tinyint(1) unsigned zerofill")
					.HasComment("支付类型，1、在线支付，2、货到付款");

				entity.Property(e => e.PostFee)
					.HasColumnName("post_fee")
					.HasColumnType("bigint(20)")
					.HasComment("邮费。单位:分。如:20007，表示:200元7分");

				entity.Property(e => e.PromotionIds)
					.HasColumnName("promotion_ids")
					.HasColumnType("varchar(256)")
					.HasDefaultValueSql("''")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.Receiver)
					.HasColumnName("receiver")
					.HasColumnType("varchar(32)")
					.HasComment("收货人")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ReceiverAddress)
					.HasColumnName("receiver_address")
					.HasColumnType("varchar(256)")
					.HasDefaultValueSql("''")
					.HasComment("收获地址（街道、住址等详细地址）")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ReceiverCity)
					.HasColumnName("receiver_city")
					.HasColumnType("varchar(256)")
					.HasDefaultValueSql("''")
					.HasComment("收获地址（市）")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ReceiverDistrict)
					.HasColumnName("receiver_district")
					.HasColumnType("varchar(256)")
					.HasDefaultValueSql("''")
					.HasComment("收获地址（区/县）")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ReceiverMobile)
					.HasColumnName("receiver_mobile")
					.HasColumnType("varchar(11)")
					.HasComment("收货人手机")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ReceiverState)
					.HasColumnName("receiver_state")
					.HasColumnType("varchar(128)")
					.HasDefaultValueSql("''")
					.HasComment("收获地址（省）")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ReceiverZip)
					.HasColumnName("receiver_zip")
					.HasColumnType("varchar(16)")
					.HasComment("收货人邮编")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ShippingCode)
					.HasColumnName("shipping_code")
					.HasColumnType("varchar(20)")
					.HasComment("物流单号")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.ShippingName)
					.HasColumnName("shipping_name")
					.HasColumnType("varchar(20)")
					.HasComment("物流名称")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");

				entity.Property(e => e.SourceType)
					.HasColumnName("source_type")
					.HasColumnType("int(1)")
					.HasDefaultValueSql("'2'")
					.HasComment("订单来源：1:app端，2：pc端，3：M端，4：微信端，5：手机qq端");

				entity.Property(e => e.TotalPay)
					.HasColumnName("total_pay")
					.HasColumnType("bigint(20)")
					.HasComment("总金额，单位为分");

				entity.Property(e => e.UserId)
					.IsRequired()
					.HasColumnName("user_id")
					.HasColumnType("varchar(32)")
					.HasComment("用户id")
					.HasCharSet("utf8")
					.UseCollation("utf8_bin");
			});

			modelBuilder.Entity<TbOrderDetail>(entity =>
			{
				entity.ToTable("tb_order_detail");

				entity.HasComment("订单详情表");

				entity.HasIndex(e => e.OrderId)
					.HasDatabaseName("key_order_id");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("订单详情id ");

				entity.Property(e => e.Image)
					.HasColumnName("image")
					.HasColumnType("varchar(128)")
					.HasDefaultValueSql("''")
					.HasComment("商品图片")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Num)
					.HasColumnName("num")
					.HasColumnType("int(11)")
					.HasComment("购买数量");

				entity.Property(e => e.OrderId)
					.HasColumnName("order_id")
					.HasColumnType("bigint(20)")
					.HasComment("订单id");

				entity.Property(e => e.OwnSpec)
					.HasColumnName("own_spec")
					.HasColumnType("varchar(1024)")
					.HasDefaultValueSql("''")
					.HasComment("商品动态属性键值集")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Price)
					.HasColumnName("price")
					.HasColumnType("decimal(20,2)")
					.HasComment("价格,单位：分");

				entity.Property(e => e.SkuId)
					.HasColumnName("sku_id")
					.HasColumnType("bigint(20)")
					.HasComment("sku商品id");

				entity.Property(e => e.Title)
					.IsRequired()
					.HasColumnName("title")
					.HasColumnType("varchar(256)")
					.HasComment("商品标题")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			modelBuilder.Entity<TbOrderStatus>(entity =>
			{
				entity.HasKey(e => e.OrderId)
					.HasName("PRIMARY");

				entity.ToTable("tb_order_status");

				entity.HasComment("订单状态表");

				entity.HasIndex(e => e.Status)
					.HasDatabaseName("status");

				entity.Property(e => e.OrderId)
					.HasColumnName("order_id")
					.HasColumnType("bigint(20)")
					.HasComment("订单id");

				entity.Property(e => e.CloseTime)
					.HasColumnName("close_time")
					.HasColumnType("datetime")
					.HasComment("交易关闭时间");

				entity.Property(e => e.CommentTime)
					.HasColumnName("comment_time")
					.HasColumnType("datetime")
					.HasComment("评价时间");

				entity.Property(e => e.ConsignTime)
					.HasColumnName("consign_time")
					.HasColumnType("datetime")
					.HasComment("发货时间");

				entity.Property(e => e.CreateTime)
					.HasColumnName("create_time")
					.HasColumnType("datetime")
					.HasComment("订单创建时间");

				entity.Property(e => e.EndTime)
					.HasColumnName("end_time")
					.HasColumnType("datetime")
					.HasComment("交易完成时间");

				entity.Property(e => e.PaymentTime)
					.HasColumnName("payment_time")
					.HasColumnType("datetime")
					.HasComment("付款时间");

				entity.Property(e => e.Status)
					.HasColumnName("status")
					.HasColumnType("int(1)")
					.HasComment("状态：1、未付款 2、已付款,未发货 3、已发货,未确认 4、交易成功 5、交易关闭 6、已评价");
			});

			//modelBuilder.Entity<TbPayLog>(entity =>
			//{
			//	entity.HasKey(e => e.OrderId)
			//		.HasName("PRIMARY");

			//	entity.ToTable("tb_pay_log");

			//	entity.Property(e => e.OrderId)
			//		.HasColumnName("order_id")
			//		.HasColumnType("bigint(20)")
			//		.HasComment("订单号");

			//	entity.Property(e => e.BankType)
			//		.HasColumnName("bank_type")
			//		.HasColumnType("varchar(16)")
			//		.HasComment("银行类型")
			//		.HasCharSet("utf8")
			//		.UseCollation("utf8_general_ci");

			//	entity.Property(e => e.ClosedTime)
			//		.HasColumnName("closed_time")
			//		.HasColumnType("datetime")
			//		.HasComment("关闭时间");

			//	entity.Property(e => e.CreateTime)
			//		.HasColumnName("create_time")
			//		.HasColumnType("datetime")
			//		.HasComment("创建时间");

			//	entity.Property(e => e.PayTime)
			//		.HasColumnName("pay_time")
			//		.HasColumnType("datetime")
			//		.HasComment("支付时间");

			//	entity.Property(e => e.PayType)
			//		.HasColumnName("pay_type")
			//		.HasComment("支付方式，1 微信支付, 2 货到付款");

			//	entity.Property(e => e.RefundTime)
			//		.HasColumnName("refund_time")
			//		.HasColumnType("datetime")
			//		.HasComment("退款时间");

			//	entity.Property(e => e.Status)
			//		.HasColumnName("status")
			//		.HasComment("交易状态，1 未支付, 2已支付, 3 已退款, 4 支付错误, 5 已关闭");

			//	entity.Property(e => e.TotalFee)
			//		.HasColumnName("total_fee")
			//		.HasColumnType("bigint(20)")
			//		.HasComment("支付金额（分）");

			//	entity.Property(e => e.TransactionId)
			//		.HasColumnName("transaction_id")
			//		.HasColumnType("varchar(32)")
			//		.HasComment("微信交易号码")
			//		.HasCharSet("utf8")
			//		.UseCollation("utf8_general_ci");

			//	entity.Property(e => e.UserId)
			//		.HasColumnName("user_id")
			//		.HasColumnType("bigint(20)")
			//		.HasComment("用户ID");
			//});

			modelBuilder.Entity<TbSeckillOrder>(entity =>
			{
				entity.ToTable("tb_seckill_order");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("秒杀订单标识");

				entity.Property(e => e.OrderId)
					.HasColumnName("order_id")
					.HasColumnType("bigint(20)")
					.HasComment("秒杀订单号");

				entity.Property(e => e.SkuId)
					.HasColumnName("sku_id")
					.HasColumnType("bigint(20)")
					.HasComment("秒杀商品ID");

				entity.Property(e => e.UserId)
					.HasColumnName("user_id")
					.HasColumnType("bigint(20)")
					.HasComment("用户编号");
			});

			modelBuilder.Entity<TbSeckillSku>(entity =>
			{
				entity.ToTable("tb_seckill_sku");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("秒杀ID");

				entity.Property(e => e.Enable)
					.HasColumnName("enable")
					.HasComment("是否允许秒杀 1-允许，0-不允许");

				entity.Property(e => e.EndTime)
					.HasColumnName("end_time")
					.HasColumnType("datetime")
					.HasComment("秒杀结束时间");

				entity.Property(e => e.Image)
					.IsRequired()
					.HasColumnName("image")
					.HasColumnType("varchar(256)")
					.HasComment("秒杀商品图片")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.SeckillPrice)
					.HasColumnName("seckill_price")
					.HasColumnType("decimal(20,2)")
					.HasComment("秒杀价格");

				entity.Property(e => e.SkuId)
					.HasColumnName("sku_id")
					.HasColumnType("bigint(20)")
					.HasComment("秒杀的商品skuId");

				entity.Property(e => e.StartTime)
					.HasColumnName("start_time")
					.HasColumnType("datetime")
					.HasComment("秒杀开始时间");

				entity.Property(e => e.Title)
					.IsRequired()
					.HasColumnName("title")
					.HasColumnType("varchar(256)")
					.HasComment("秒杀商品标题")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			modelBuilder.Entity<TbSku>(entity =>
			{
				entity.ToTable("tb_sku");

				entity.HasComment("sku表,该表表示具体的商品实体,如黑色的 64g的iphone 8");

				entity.HasIndex(e => e.SpuId)
					.HasDatabaseName("key_spu_id");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("sku id");

				entity.Property(e => e.CreateTime)
					.HasColumnName("create_time")
					.HasColumnType("datetime")
					.HasComment("添加时间");

				entity.Property(e => e.Enable)
					.IsRequired()
					.HasColumnName("enable")
					.HasDefaultValueSql("'1'")
					.HasComment("是否有效，0无效，1有效");

				entity.Property(e => e.Images)
					.HasColumnName("images")
					.HasColumnType("varchar(1024)")
					.HasDefaultValueSql("''")
					.HasComment("商品的图片，多个图片以‘,’分割")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Indexes)
					.HasColumnName("indexes")
					.HasColumnType("varchar(32)")
					.HasDefaultValueSql("''")
					.HasComment("特有规格属性在spu属性模板中的对应下标组合")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.LastUpdateTime)
					.HasColumnName("last_update_time")
					.HasColumnType("datetime")
					.HasComment("最后修改时间");

				entity.Property(e => e.OwnSpec)
					.HasColumnName("own_spec")
					.HasColumnType("varchar(1024)")
					.HasDefaultValueSql("''")
					.HasComment("sku的特有规格参数键值对，json格式，反序列化时请使用linkedHashMap，保证有序")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Price)
					.HasColumnName("price")
					.HasColumnType("bigint(15)")
					.HasComment("销售价格，单位为分");

				entity.Property(e => e.SpuId)
					.HasColumnName("spu_id")
					.HasColumnType("bigint(20)")
					.HasComment("spu id");

				entity.Property(e => e.Title)
					.IsRequired()
					.HasColumnName("title")
					.HasColumnType("varchar(256)")
					.HasComment("商品标题")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			modelBuilder.Entity<TbSpecGroup>(entity =>
			{
				entity.ToTable("tb_spec_group");

				entity.HasComment("规格参数的分组表，每个商品分类下有多个规格参数组");

				entity.HasIndex(e => e.Cid)
					.HasDatabaseName("key_category");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("主键");

				entity.Property(e => e.Cid)
					.HasColumnName("cid")
					.HasColumnType("bigint(20)")
					.HasComment("商品分类id，一个分类下有多个规格组");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasColumnName("name")
					.HasColumnType("varchar(32)")
					.HasComment("规格组的名称")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			modelBuilder.Entity<TbSpecParam>(entity =>
			{
				entity.ToTable("tb_spec_param");

				entity.HasComment("规格参数组下的参数名");

				entity.HasIndex(e => e.Cid)
					.HasDatabaseName("key_category");

				entity.HasIndex(e => e.GroupId)
					.HasDatabaseName("key_group");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("主键");

				entity.Property(e => e.Cid)
					.HasColumnName("cid")
					.HasColumnType("bigint(20)")
					.HasComment("商品分类id");

				entity.Property(e => e.Generic)
					.HasColumnName("generic")
					.HasComment("是否是sku通用属性，true或false");

				entity.Property(e => e.GroupId)
					.HasColumnName("group_id")
					.HasColumnType("bigint(20)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasColumnName("name")
					.HasColumnType("varchar(256)")
					.HasComment("参数名")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Numeric)
					.HasColumnName("numeric")
					.HasComment("是否是数字类型参数，true或false");

				entity.Property(e => e.Searching)
					.HasColumnName("searching")
					.HasComment("是否用于搜索过滤，true或false");

				entity.Property(e => e.Segments)
					.HasColumnName("segments")
					.HasColumnType("varchar(1024)")
					.HasDefaultValueSql("''")
					.HasComment("数值类型参数，如果需要搜索，则添加分段间隔值，如CPU频率间隔：0.5-1.0")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Unit)
					.HasColumnName("unit")
					.HasColumnType("varchar(256)")
					.HasDefaultValueSql("''")
					.HasComment("数字类型参数的单位，非数字类型可以为空")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			modelBuilder.Entity<TbSpu>(entity =>
			{
				entity.ToTable("tb_spu");

				entity.HasComment("spu表，该表描述的是一个抽象性的商品，比如 iphone8");

				entity.Property(e => e.Id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)")
					.HasComment("spu id");

				entity.Property(e => e.BrandId)
					.HasColumnName("brand_id")
					.HasColumnType("bigint(20)")
					.HasComment("商品所属品牌id");

				entity.Property(e => e.Cid1)
					.HasColumnName("cid1")
					.HasColumnType("bigint(20)")
					.HasComment("1级类目id");

				entity.Property(e => e.Cid2)
					.HasColumnName("cid2")
					.HasColumnType("bigint(20)")
					.HasComment("2级类目id");

				entity.Property(e => e.Cid3)
					.HasColumnName("cid3")
					.HasColumnType("bigint(20)")
					.HasComment("3级类目id");

				entity.Property(e => e.CreateTime)
					.HasColumnName("create_time")
					.HasColumnType("datetime")
					.HasComment("添加时间");

				entity.Property(e => e.LastUpdateTime)
					.HasColumnName("last_update_time")
					.HasColumnType("datetime")
					.HasComment("最后修改时间");

				entity.Property(e => e.Saleable)
					.IsRequired()
					.HasColumnName("saleable")
					.HasDefaultValueSql("'1'")
					.HasComment("是否上架，0下架，1上架");

				entity.Property(e => e.SubTitle)
					.HasColumnName("sub_title")
					.HasColumnType("varchar(256)")
					.HasDefaultValueSql("''")
					.HasComment("子标题")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Title)
					.IsRequired()
					.HasColumnName("title")
					.HasColumnType("varchar(128)")
					.HasDefaultValueSql("''")
					.HasComment("标题")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Valid)
					.IsRequired()
					.HasColumnName("valid")
					.HasDefaultValueSql("'1'")
					.HasComment("是否有效，0已删除，1有效");
			});

			modelBuilder.Entity<TbSpuDetail>(entity =>
			{
				entity.HasKey(e => e.SpuId)
					.HasName("PRIMARY");

				entity.ToTable("tb_spu_detail");

				entity.Property(e => e.SpuId)
					.HasColumnName("spu_id")
					.HasColumnType("bigint(20)");

				entity.Property(e => e.AfterService)
					.HasColumnName("after_service")
					.HasColumnType("varchar(1024)")
					.HasDefaultValueSql("''")
					.HasComment("售后服务")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Description)
					.HasColumnName("description")
					.HasColumnType("text")
					.HasComment("商品描述信息")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.GenericSpec)
					.IsRequired()
					.HasColumnName("generic_spec")
					.HasColumnType("varchar(2048)")
					.HasDefaultValueSql("''")
					.HasComment("通用规格参数数据")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.PackingList)
					.HasColumnName("packing_list")
					.HasColumnType("varchar(1024)")
					.HasDefaultValueSql("''")
					.HasComment("包装清单")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.SpecialSpec)
					.IsRequired()
					.HasColumnName("special_spec")
					.HasColumnType("varchar(1024)")
					.HasComment("特有规格参数及可选值信息，json格式")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			modelBuilder.Entity<TbStock>(entity =>
			{
				entity.HasKey(e => e.SkuId)
					.HasName("PRIMARY");

				entity.ToTable("tb_stock");

				entity.HasComment("库存表，代表库存，秒杀库存等信息");

				entity.Property(e => e.SkuId)
					.HasColumnName("sku_id")
					.HasColumnType("bigint(20)")
					.HasComment("库存对应的商品sku id");

				entity.Property(e => e.SeckillStock)
					.HasColumnName("seckill_stock")
					.HasColumnType("int(9)")
					.HasDefaultValueSql("'0'")
					.HasComment("可秒杀库存");

				entity.Property(e => e.SeckillTotal)
					.HasColumnName("seckill_total")
					.HasColumnType("int(9)")
					.HasDefaultValueSql("'0'")
					.HasComment("秒杀总数量");

				entity.Property(e => e.Stock)
					.HasColumnName("stock")
					.HasColumnType("int(9)")
					.HasComment("库存数量");
			});

			modelBuilder.Entity<TbUser>(entity =>
			{
				entity.ToTable("tb_user");

				entity.HasComment("用户表");

				entity.HasIndex(e => e.Username)
					.HasDatabaseName("username")
					.IsUnique();

				entity.Property(e => e.id)
					.HasColumnName("id")
					.HasColumnType("bigint(20)");

				entity.Property(e => e.Created)
					.HasColumnName("created")
					.HasColumnType("datetime")
					.HasComment("创建时间");

				entity.Property(e => e.Password)
					.IsRequired()
					.HasColumnName("password")
					.HasColumnType("varchar(32)")
					.HasComment("密码，加密存储")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Phone)
					.HasColumnName("phone")
					.HasColumnType("varchar(11)")
					.HasComment("注册手机号")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Salt)
					.IsRequired()
					.HasColumnName("salt")
					.HasColumnType("varchar(32)")
					.HasComment("密码加密的salt值")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");

				entity.Property(e => e.Username)
					.IsRequired()
					.HasColumnName("username")
					.HasColumnType("varchar(32)")
					.HasComment("用户名")
					.HasCharSet("utf8")
					.UseCollation("utf8_general_ci");
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
