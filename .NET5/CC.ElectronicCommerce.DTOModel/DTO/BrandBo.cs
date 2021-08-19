using System;
using System.Collections.Generic;
using System.Text;

namespace CC.ElectronicCommerce.Model.DTO
{

    public class BrandBo
    {
        private long id { get; set; }
        private string name { get; set; }
        private string image { get; set; }
        private List<long> cids { get; set; }
        private char letter { get; set; }

    }
}
