using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private ICategoryService _categoryService;
        private IBrandService _brandService;
        public CategoryController(ICategoryService categoryService, IBrandService brandService)
        {
            _categoryService = categoryService;
            _brandService = brandService;

        }

        /**
    * 根据父类ID查询分类结果
    * @param pid
    * @return
    */
        [Route("list")]
        [HttpGet]
        public List<TbCategory> QueryCategoryByPid(long pid)
        {
            List<TbCategory> categoryList = _categoryService.QueryCategoryByPid(pid);
            return categoryList;
        }

        /**
		 * 根据品牌ID查询商品分类
		 *
		 * @param bid
		 * @return
		 */
        [Route("bid/{bid}")]
        [HttpGet]
        public List<TbCategory> QueryCategoryByBid(long bid)
        {
            return _brandService.QueryCategoryByBid(bid);
        }

        /**
		 * 根据商品分类Ids查询分类
		 * @param ids
		 * @return
		 */

        [Route("list/ids")]
        [HttpPost]
        public List<TbCategory> QueryCategoryByIds(List<long> ids)
        {
            return _categoryService.QueryCategoryByIds(ids);
        }

        /**
		 * 根据cid3查询三级分类
		 * @param id
		 * @return
		 */
        [Route("all/level/{id}")]
        [HttpGet]
        public Result QueryAllByCid3(long id)
        {
            List<TbCategory> categoryList = this._categoryService.QueryAllByCid3(id);
            return Result.Success().SetData(categoryList);
         
        }
    }
}
