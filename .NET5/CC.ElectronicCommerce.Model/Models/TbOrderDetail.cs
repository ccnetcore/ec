using System;
using System.Collections.Generic;

namespace CC.ElectronicCommerce.Model
{
    public partial class TbOrderDetail
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long SkuId { get; set; }
        public int Num { get; set; }
        public string Title { get; set; }
        public string OwnSpec { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
    }
}
