using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Model.DTO;
using System;
using System.Collections.Generic;
using System.Text;


namespace CC.ElectronicCommerce.Interface
{
    public interface IBrandService
    {
        PageResult<TbBrand> QueryBrandByPageAndSort(int page, int rows, string sortBy, bool desc, string key);

        void SaveBrand(TbBrand brand, List<long> cids);

        List<TbCategory> QueryCategoryByBid(long bid);

        void UpdateBrand(BrandBo brandbo);

        void DeleteBrand(long bid);

        List<TbBrand> QueryBrandByCid(long cid);

        TbBrand QueryBrandByBid(long id);

        List<TbBrand> QueryBrandByIds(List<long> ids);

    }
}
