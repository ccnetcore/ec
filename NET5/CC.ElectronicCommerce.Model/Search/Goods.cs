using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model.Search
{
    [ElasticsearchType(IdProperty = "id")]//主键声明，且主键必须是属性
    public class Goods
    {
        public long id { get; set; } //SpuId

        public string all;  //所有需要被搜索的信息，包括品牌，分类，标题
        public string subtitle;  //子标题
        public long brandId;
        public long cid1;
        public long cid2;
        public long cid3;
        public DateTime? createTime;
        public HashSet<double> price = new HashSet<double>();  //是所有sku的价格集合。方便根据价格进行筛选过滤

        public string skus;  //sku信息的json结构数据
        public Dictionary<string, object> specs = new Dictionary<string, object>();  //可搜索的规格参数，key是参数名，值是参数值
    }
}
