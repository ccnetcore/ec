using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.GoodsMicroservice.Controllers
{
	[Route("api/item/[controller]")]
	[ApiController]
	public class GoodsController : ControllerBase
	{
		private IGoodsService _goodsService;
		public GoodsController(IGoodsService goodsService)
		{
			_goodsService = goodsService;
		}
		[Route("spu/page")]
		[HttpGet]
		public string QuerySpuByPage(int page, int rows, string key, bool saleable)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(_goodsService.QuerySpuByPage(page, rows, key, saleable));
		}

		/**
		 * 查询spu详情
		 * @param spuId
		 * @return
		 */

		[Route("spu/detail/{spuId}")]
		[HttpGet]
		public TbSpuDetail QuerySpuDetailBySpuId(long spuId)
		{
			return _goodsService.QuerySpuDetailBySpuId(spuId);
		}

		/**
		 * 根据spuId查询商品详情
		 * @param id
		 * @return
		 */
		[Route("sku/list")]
		[HttpGet]
		public List<TbSku> QuerySkuBySpuId(long id)
		{
			return _goodsService.QuerySkuBySpuId(id);

		}

		/**
		 * 根据sku ids查询sku
		 * @param ids
		 * @return
		 */
		[Route("sku/list/ids")]
		[HttpGet]
		public List<TbSku> QuerySkusByIds(List<long> ids)
		{
			return _goodsService.QuerySkusByIds(ids);
		}


		/**
		 * 删除商品
		 * @param spuId
		 * @return
		 */
		[Route("spu/spuId/{spuId}")]
		[HttpDelete]
		public void DeleteGoodsBySpuId(long spuId)
		{
			_goodsService.DeleteGoodsBySpuId(spuId);
		}


		/**
		 * 添加商品
		 * @param spu
		 * @return
		 */
		[Route("goods")]
		[HttpPost]

		public void addGoods([FromBody] TbSpu spu)
		{
			_goodsService.AddGoods(spu);
		}

		/**
		 * 更新商品
		 * @param spu
		 * @return
		 */
		[Route("goods")]
		[HttpPut]
		public void UpdateGoods([FromBody] TbSpu spu)
		{
			_goodsService.UpdateGoods(spu);
		}

		[Route("spu/saleable")]
		[HttpPut]
		public void handleSaleable([FromBody] TbSpu spu)
		{
			_goodsService.HandleSaleable(spu);


		}

		/**
		 * 根据spuId查询spu及skus
		 * @param spuId
		 * @return
		 */
		[Route("spu/{id}")]
		[HttpGet]
		public TbSpu QuerySpuBySpuId(long spuId)
		{
			return _goodsService.QuerySpuBySpuId(spuId);
		}

		/**
		 * 减库存
		 * @param cartDtos
		 * @return
		 */
		[Route("stock/decrease")]
		[HttpPost]
		public void DecreaseStock([FromBody] List<CartDto> cartDtos)
		{
			_goodsService.DecreaseStock(cartDtos);

		}

        //	////////////////////// 秒杀 ///////////////////////////
        //	/**
        //	 * 查询秒杀商品
        //	 * @return
        //	 */
        //	[Route("/seckill/list")]
        //	[HttpGet]
        //	public List<SeckillGoods> QuerySeckillGoods()
        //	{
        //		List<SeckillGoods> list = this._goodsService.QuerySeckillGoods();
        //		if (list == null || list.Count < 0)
        //		{
        //			//return ResponseEntity.status(HttpStatus.NOT_FOUND).build();
        //		}
        //		return list;
        //	}

        //	/**
        //	 * 添加秒杀商品
        //	 * @param seckillParameters
        //	 * @return
        //	 * @throws
        //	 */
        //	[Route("/seckill/add")]
        //	[HttpPost]
        //	public bool AddSeckillGoods([FromBody] List<SeckillParameter> seckillParameters)
        //	{
        //		if (seckillParameters != null && seckillParameters.Count > 0)
        //		{
        //			foreach (SeckillParameter seckillParameter in seckillParameters)
        //			{
        //				this._goodsService.AddSeckillGoods(seckillParameter);
        //			}
        //		}
        //		else
        //		{
        //			return false;
        //		}
        //		return true;
        //	}
    }
}
