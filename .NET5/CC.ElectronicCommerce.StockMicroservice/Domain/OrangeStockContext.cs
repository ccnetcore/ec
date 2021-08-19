using System;
using CC.ElectronicCommerce.Common.IOCOptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace CC.ElectronicCommerce.StockModel
{
	public partial class OrangeStockContext : DbContext
	{
		private readonly IOptionsMonitor<MySqlConnOptions>  _optionsMonitor;
		private readonly string _connStr;

		public OrangeStockContext(IOptionsMonitor<MySqlConnOptions> optionsMonitor)
		{
			_optionsMonitor = optionsMonitor;
			_connStr = _optionsMonitor.CurrentValue.Url;
		}

		public OrangeStockContext(string connstr)
		{
			_connStr = connstr;
		}

		public virtual DbSet<TbStock> TbStock { get; set; }
		public virtual DbSet<TbStockLog> TbUser { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseMySql(this._connStr, new MySqlServerVersion(new Version(8, 0, 21)));
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
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

			modelBuilder.Entity<TbStockLog>(entity =>
			{
				entity.HasKey(e => new { e.SkuId, e.OrderId })
					.HasName("PRIMARY");

				entity.ToTable("tb_stock_log");

				entity.HasComment("库存表，代表库存，秒杀库存等信息");

				entity.Property(e => e.SkuId)
					.HasColumnName("sku_id")
					.HasColumnType("bigint(20)")
					.HasComment("库存对应的商品sku id");

				entity.Property(e => e.OrderId)
					.HasColumnName("order_id")
					.HasColumnType("int(20)")
					.HasComment("本次库存流水对应的订单id");

				entity.Property(e => e.OrderNum)
					.HasColumnName("order_num")
					.HasColumnType("int(9)")
					.HasDefaultValueSql("'0'")
					.HasComment("本次订单sku数量");

				entity.Property(e => e.SeckillNum)
					.HasColumnName("seckill_num")
					.HasColumnType("int(9)")
					.HasDefaultValueSql("'0'")
					.HasComment("本次秒杀sku个数");

				entity.Property(e => e.StockType)
					.HasColumnName("stock_type")
					.HasColumnType("int(9)")
					.HasDefaultValueSql("'0'")
					.HasComment("库存变更类型");

				entity.Property(e => e.StockStatus)
					.HasColumnName("stock_status")
					.HasColumnType("int(9)")
					.HasDefaultValueSql("'0'")
					.HasComment("流水状态");
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
