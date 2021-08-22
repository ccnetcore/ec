using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model
{
    public class SeckillParameter
    {
        /// <summary>
        /// 要秒杀的sku id
        /// </summary>
        public long Id { set; get; }

        /**
         * 秒杀开始时间
         */
        public string StartTime { set; get; }

        /**
         * 秒杀结束时间
         */
        public string EndTime { set; get; }

        /**
         * 参与秒杀的商品数量
         */
        public int Count { set; get; }

        /**
         * 折扣后的金额 0.33
         */
        public double Discount { set; get; }
    }
}
