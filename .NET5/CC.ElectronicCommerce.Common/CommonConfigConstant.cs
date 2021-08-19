using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Common
{
    public class CommonConfigConstant
    {
        /// <summary>
        /// 保存库存数据的前缀,用的地方直接拼装skuid
        /// $"{CommonConfigConstant.StockRedisKeyPrefix}{stock.SkuId}"
        /// </summary>
        public const string StockRedisKeyPrefix = "Stock_Redis_Key_Prefix_";
    }
}
