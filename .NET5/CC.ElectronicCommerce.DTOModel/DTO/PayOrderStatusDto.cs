using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model.DTO
{
    /// <summary>
    /// 支付回调订单时
    /// </summary>
    public class PayOrderStatusDto
    {
        public long OrderId { get; set; }
        /// <summary>
        /// 0 未支付  2已支付
        /// </summary>
        public int PayStatus { get; set; }
    }
}
