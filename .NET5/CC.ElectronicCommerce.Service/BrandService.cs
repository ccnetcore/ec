using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Model.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CC.ElectronicCommerce.Service
{
	public class BrandService : IBrandService
	{
		private OrangeContext _orangeContext; 
		public BrandService(OrangeContext orangeContext)
		{
			_orangeContext = orangeContext;
		}

		public void DeleteBrand(long bid)
		{
			throw new NotImplementedException();
		}

		public TbBrand QueryBrandByBid(long id)
		{
			TbBrand b1 = _orangeContext.TbBrand.Where(m => m.Id == id).FirstOrDefault();
			if (b1 == null)
			{
				throw new Exception("查询品牌不存在");
			}
			return b1;
		}

		public List<TbBrand> QueryBrandByCid(long cid)
		{
			var brandList = _orangeContext.TbBrand.FromSqlRaw($"select * from tb_brand where id in (select brand_id from tb_category_brand where category_id = {cid})").ToList();
			if (brandList.Count <= 0)
			{
				throw new Exception("没有找到分类下的品牌");
			}
			return brandList;
		}

		public List<TbBrand> QueryBrandByIds(List<long> ids)
		{
			List<TbBrand> brands = _orangeContext.TbBrand.Where(m => ids.Contains(m.Id)).ToList();
			if (brands.Count <= 0)
			{
				throw new Exception("查询品牌不存在");
			}
			return brands;
		}

		public PageResult<TbBrand> QueryBrandByPageAndSort(int page, int rows, string sortBy, bool desc, string key)
		{
			var list = _orangeContext.TbBrand.AsQueryable();

			if (!string.IsNullOrEmpty(key))
			{
				list = list.Where(m => m.Name.Contains(key) || m.Letter == key);
			}
			if (!string.IsNullOrEmpty(sortBy))
			{
				if (desc)
				{
					list.OrderByDescending(m => m.Letter);
				}
			}

			var total = list.Count();
			var tbBrands = list.Take(10).ToList();
			if (tbBrands.Count() <= 0)
			{
				throw new Exception("查询的品牌列表为空");
			}
			var data = new PageResult<TbBrand>(total, tbBrands);
			return data;
		}

		public List<TbCategory> QueryCategoryByBid(long bid)
		{
			throw new NotImplementedException();
		}

		public void SaveBrand(TbBrand brand, List<long> cids)
		{
			throw new NotImplementedException();
		}

		public void UpdateBrand(BrandBo brandbo)
		{
			throw new NotImplementedException();
		}
	}
}
