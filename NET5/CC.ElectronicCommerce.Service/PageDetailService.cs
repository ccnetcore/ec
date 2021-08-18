using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Service
{
	public class PageDetailService : IPageDetailService
	{
		private IGoodsService _goodsService;
		private IBrandService _brandService;
		private ICategoryService _categoryService;
		private ISpecService _specService;
		public PageDetailService(IGoodsService goodsService, IBrandService brandService, ICategoryService categoryService, ISpecService specService)
		{
			_goodsService = goodsService;
			_brandService = brandService;
			_categoryService = categoryService;
			_specService = specService;
		}
		public Dictionary<string, object> loadModel(long spuId)
		{
			Dictionary<string, object> model = new Dictionary<string, object>();
			TbSpu spu = _goodsService.QuerySpuBySpuId(spuId);

			//未上架，则不应该查询到商品详情信息，抛出异常
			if (spu.Saleable == null || spu.Saleable == false)
			{
				throw new Exception("查询了未上架的商品");
			}
			TbSpuDetail detail = spu.SpuDetail;
			List<TbSku> skus = spu.Skus;
			TbBrand brand = _brandService.QueryBrandByBid(spu.BrandId);
			//查询三级分类
			List<TbCategory> categories = _categoryService.QueryCategoryByIds(new List<long>() { spu.Cid1, spu.Cid2, spu.Cid3 });
			List<TbSpecGroup> specs = _specService.QuerySpecsByCid(spu.Cid3);
			model.Add("brand", brand);
			model.Add("categories", categories);
			model.Add("spu", spu);
			model.Add("skus", skus);
			model.Add("detail", detail);
			model.Add("specs", specs);
			return model;
		}
	}
}
