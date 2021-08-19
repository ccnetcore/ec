using System;
using System.Collections.Generic;

namespace CC.ElectronicCommerce.StockModel
{
    public partial class TbStockLog
    {
        public long SkuId { get; set; }
        public long OrderId { get; set; }
        public int SeckillNum { get; set; }
        public int OrderNum { get; set; }
        public int StockType { get; set; }
        public int StockStatus { get; set; }
    }

    public class TbStockLogEnum
    {
        /// <summary>
        /// 库存操作类型0 新增   1减少
        /// </summary>
        public enum StockType
        {
            Increase = 0,
            Decrease = 1
        }
        /// <summary>
        /// 库存记录状态0 正常操作   1取消回退
        /// </summary>
        public enum StockStatus
        {
            Normal=0,
            Backoff=1
        }
    }
}
