using System;
using System.Collections.Generic;

namespace CC.ElectronicCommerce.Model
{
    public partial class TbStock
    {
        public long SkuId { get; set; }
        public int? SeckillStock { get; set; }
        public int? SeckillTotal { get; set; }
        public int Stock { get; set; }
    }
}
