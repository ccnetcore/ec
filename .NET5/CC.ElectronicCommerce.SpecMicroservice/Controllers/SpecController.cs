using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.SpecMicroservice.Controllers
{
	[Route("api/item/[controller]")]
	[ApiController]
	public class SpecController : ControllerBase
	{
		private ISpecService _specService;
		public SpecController(ISpecService specService)
		{
			_specService = specService;
		}

		[Route("groups/{cid}")]
		[HttpGet]
		public List<TbSpecGroup> QuerySpecGroupByCid(long cid)
		{
			return _specService.QuerySpecGroupByCid(cid);
		}


		[Route("params")]
		[HttpGet]
		public List<TbSpecParam> QuerySpecParams(long gid, long cid, bool searching, bool generic
)
		{
			return _specService.QuerySpecParams(gid, cid, searching, generic);
		}


		[Route("{cid}")]
		[HttpGet]
		public List<TbSpecGroup> QuerySpecsByCid(long cid)
		{
			return _specService.QuerySpecsByCid(cid);
		}

		/**
	* 增加商品规格组
	*
	* @param specGroup
	* @return
	*/

		[Route("group")]
		[HttpPost]
		public void SaveSpecGroup([FromBody] TbSpecGroup specGroup)
		{
			_specService.SaveSpecGroup(specGroup);

		}

		/**
		 * 删除商品规格组
		 *
		 * @param id
		 * @return
		 */

		[Route("group/{id}")]
		[HttpDelete]
		public void DeleteSpecGroup(long id)
		{

			_specService.DeleteSpecGroup(id);

		}

		/**
		 * 更新商品规格组
		 *
		 * @param specGroup
		 * @return
		 */
		[Route("group")]
		[HttpPut]
		public void UpdateSpecGroup([FromBody] TbSpecGroup specGroup)
		{
			_specService.UpdateSpecGroup(specGroup);
		}

		/**
		 * 增加商品规格参数
		 *
		 * @param specParam
		 * @return
		 */
		[Route("param")]
		[HttpPost]
		public void saveSpecParam([FromBody] TbSpecParam specParam)
		{
			_specService.SaveSpecParam(specParam);
		}

		/**
		 * 删除商品规格参数
		 *
		 * @param id
		 * @return
		 */
		[Route("param/{id}")]
		[HttpDelete]
		public void DeleteSpecParam(long id)
		{
			_specService.DeleteSpecParam(id);

		}

		/**
		 * 更新商品规格参数
		 *
		 * @param specParam
		 * @return
		 */
		[Route("param")]
		[HttpPut]
		public void updateSpecParam([FromBody] TbSpecParam specParam)
		{
			_specService.UpdateSpecParam(specParam);
		}
	}
}
