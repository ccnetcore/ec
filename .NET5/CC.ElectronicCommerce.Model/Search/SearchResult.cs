using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Model.Search
{
    public class SearchResult<Goods> : PageResult<Goods>
    {
        public List<TbBrand> brands = new List<TbBrand>();
        public List<TbCategory> categories = new List<TbCategory>();
        //规格参数过滤条件
        public List<Dictionary<string, object>> specs = new List<Dictionary<string, object>>();
        public SearchResult(long total,
                       int totalPage,
                       List<Goods> items,
                       List<TbCategory> categories,
                       List<TbBrand> brands,
                       List<Dictionary<string, object>> specs) : base
            (total, items)
        {

            this.categories = categories;
            this.brands = brands;
            this.specs = specs;
        }

    }
}
