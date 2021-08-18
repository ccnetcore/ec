using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CC.ElectronicCommerce.Service
{
	public class CategoryService : ICategoryService
	{
		private OrangeContext _orangeContext;
		public CategoryService(OrangeContext orangeContext)
		{
			_orangeContext = orangeContext;
		}
		public List<TbCategory> QueryCategoryByPid(long pid)
		{
			List<TbCategory> categoryList = _orangeContext.TbCategory.Where(m => m.ParentId == pid).ToList();
			if (categoryList.Count <= 0)
			{
				throw new Exception("查询分类不存在");
			}
			return categoryList;
		}


		public List<TbCategory> QueryCategoryByIds(List<long> ids)
		{
			return _orangeContext.TbCategory.Where(m => ids.Contains(m.Id)).ToList();
		}
		public List<TbCategory> QueryAllByCid3(long id)
		{
			TbCategory c3 = _orangeContext.TbCategory.Where(m => m.Id == id).FirstOrDefault();
			TbCategory c2 = _orangeContext.TbCategory.Where(m => m.Id == c3.ParentId).FirstOrDefault();
			TbCategory c1 = _orangeContext.TbCategory.Where(m => m.Id == c2.ParentId).FirstOrDefault();
			List<TbCategory> list = new List<TbCategory>() { c1, c2, c3 };
			//if (CollectionUtils.isEmpty(list))
			//{
			//	throw new Exception("分类不存在");
			//}
			return list;
		}
	}
}
