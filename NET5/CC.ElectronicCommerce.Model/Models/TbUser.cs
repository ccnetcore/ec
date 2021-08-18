using System;
using System.Collections.Generic;

namespace CC.ElectronicCommerce.Model
{
    public partial class TbUser
    {
        public long id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public DateTime Created { get; set; }
        public string Salt { get; set; }
    }
}
