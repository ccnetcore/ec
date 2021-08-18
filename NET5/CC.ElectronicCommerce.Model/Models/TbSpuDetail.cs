using System;
using System.Collections.Generic;

namespace CC.ElectronicCommerce.Model
{
    public partial class TbSpuDetail
    {
        public long SpuId { get; set; }
        public string Description { get; set; }
        public string GenericSpec { get; set; }
        public string SpecialSpec { get; set; }
        public string PackingList { get; set; }
        public string AfterService { get; set; }
    }
}
