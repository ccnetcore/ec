using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Model.DTO;

namespace CC.ElectronicCommerce.Service
{
    public class GoodsService : IGoodsService
    {

        private OrangeContext _orangeContext;
        private CacheClientDB _cacheClientDB;

        public GoodsService(OrangeContext orangeContext, CacheClientDB cacheClientDB)
        {
            _orangeContext = orangeContext;
            _cacheClientDB = cacheClientDB;
        }

        public void AddGoods(TbSpu spu)
        {
            throw new NotImplementedException();
        }


        ////todo
        public void DecreaseStock(List<CartDto> cartDtos)
        {
            foreach (CartDto cartDto in cartDtos)
            {
                int count = _orangeContext.Database.ExecuteSqlRaw($"update tb_stock set stock = stock - {cartDto.num} where sku_id = {cartDto.skuId} and stock >= {cartDto.num}");
                if (count != 1)
                {
                    throw new Exception("扣减库存失败");
                }
            }
        }

        public void DeleteGoodsBySpuId(long spuId)
        {
            throw new NotImplementedException();
        }

        public void HandleSaleable(TbSpu spu)
        {
            throw new NotImplementedException();
        }



        public List<TbSku> QuerySkuBySpuId(long spuId)
        {
            List<TbSku> skuList = _orangeContext.TbSku.Where(m => m.SpuId == spuId).ToList();
            if (skuList.Count <= 0)
            {
                throw new Exception("查询的商品的SKU失败");
            }
            //查询库存
            foreach (TbSku sku1 in skuList)
            {
                sku1.Stock = _orangeContext.TbStock.Where(m => m.SkuId == sku1.Id).FirstOrDefault().Stock;
            }
            return skuList;
        }



        public List<long> QuerySpuIdsPage(int page, int pageSize)
        {
            return _orangeContext.TbSpu.Where(spu => spu.Id > 0)
                  .OrderBy(spu => spu.Id)
                  .Skip((page - 1) * pageSize)
                  .Take(pageSize)
                  .Select(spu => spu.Id)
                    .ToList();
        }

        public List<TbSku> QuerySkusByIds(List<long> ids)
        {
            List<TbSku> skus = _orangeContext.TbSku.Where(m => ids.Contains(m.Id)).ToList();
            if (skus.Count <= 0)
            {
                throw new Exception("查询");
            }
            //填充库存
            FillStock(ids, skus);
            return skus;
        }

        private void FillStock(List<long> ids, List<TbSku> skus)
        {
            //批量查询库存
            List<TbStock> stocks = _orangeContext.TbStock.Where(m => ids.Contains(m.SkuId)).ToList();
            if (stocks.Count <= 0)
            {
                throw new Exception("保存库存失败");
            }
            Dictionary<long, int> map = stocks.ToDictionary(s => s.SkuId, s => s.Stock);
            //首先将库存转换为map，key为sku的ID
            //遍历skus，并填充库存
            foreach (var sku in skus)
            {
                sku.Stock = map[sku.Id];
            }
        }


        public PageResult<TbSpu> QuerySpuByPage(int page, int rows, string key, bool? saleable)
        {
            var list = _orangeContext.TbSpu.AsQueryable();
            if (!string.IsNullOrEmpty(key))
            {
                list = list.Where(m => m.Title.Contains(key));
            }
            if (saleable != null)
            {
                list = list.Where(m => m.Saleable == saleable);
            }
            //默认以上一次更新时间排序
            list = list.OrderByDescending(m => m.LastUpdateTime);

            //只查询未删除的商品 
            list = list.Where(m => m.Valid == true);

            //查询
            List<TbSpu> spuList = list.ToList();

            if (spuList.Count <= 0)
            {
                throw new Exception("查询的商品不存在");
            }
            //对查询结果中的分类名和品牌名进行处理
            HandleCategoryAndBrand(spuList);
            return new PageResult<TbSpu>(spuList.Count, spuList);
        }

        /**
		 * 处理商品分类名和品牌名
		 *
		 * @param spuList
		 */
        private void HandleCategoryAndBrand(List<TbSpu> spuList)
        {
            foreach (TbSpu spu in spuList)
            {
                //根据spu中的分类ids查询分类名
                var ids = new List<string>() { spu.Cid1.ToString(), spu.Cid2.ToString(), spu.Cid3.ToString() };
                List<string> nameList = _orangeContext.TbCategory.Where(m => ids.Contains(m.Id.ToString())).Select(m => m.Name).ToList();
                //对分类名进行处理
                spu.Cname = string.Join('/', nameList);
                //查询品牌
                spu.Bname = _orangeContext.TbBrand.Where(m => m.Id == spu.BrandId).FirstOrDefault()?.Name;
            }
        }


        public TbSpu QuerySpuBySpuId(long spuId)
        {
            //根据spuId查询spu
            TbSpu spu = _orangeContext.TbSpu.Where(m => m.Id == spuId).FirstOrDefault();
            //查询spuDetail
            TbSpuDetail detail = QuerySpuDetailBySpuId(spuId);
            //查询skus
            List<TbSku> skus = QuerySkuBySpuId(spuId);
            spu.SpuDetail = detail;
            spu.Skus = skus;

            return spu;
        }

        public TbSpuDetail QuerySpuDetailBySpuId(long spuId)
        {
            TbSpuDetail spuDetail = _orangeContext.TbSpuDetail.Where(m => m.SpuId == spuId).FirstOrDefault();
            if (spuDetail == null)
            {
                throw new Exception("查询的商品不存在");
            }
            return spuDetail;
        }

        public void UpdateGoods(TbSpu spu)
        {
            throw new NotImplementedException();
        }
        //////////////////////////////// 秒杀//////////////////////////////
        ///// <summary>
        ///// 添加秒杀商品
        ///// </summary>
        ///// <param name="seckillParameter"></param>
        public void AddSeckillGoods(SeckillParameter seckillParameter)
        {
            //1.根据sku_id查询商品
            TbSku sku = _orangeContext.TbSku.First(sku => sku.Id == seckillParameter.Id);
            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = "yyyy-MM-dd HH:mm:ss";


            IDbContextTransaction trans = null;
            try
            {
                trans = this._orangeContext.Database.BeginTransaction();

                //2.插入到秒杀商品表中
                TbSeckillSku seckillGoods = new TbSeckillSku()
                {
                    Enable = true,
                    StartTime = Convert.ToDateTime(seckillParameter.StartTime, dtFormat),
                    EndTime = Convert.ToDateTime(seckillParameter.EndTime, dtFormat),
                    Image = sku.Images,
                    SkuId = sku.Id,
                    Title = sku.Title,
                    SeckillPrice = sku.Price * seckillParameter.Discount,
                    Stock = seckillParameter.Count
                };
                _orangeContext.TbSeckillSku.Add(seckillGoods);

                //3.更新对应的库存信息，tb_stock
                TbStock stock = _orangeContext.TbStock.First(s => s.SkuId == sku.Id);

                if (stock == null || stock.Stock - seckillGoods.Stock < 0)
                {
                    throw new Exception("参与秒杀的库存不足");
                }

                if (_orangeContext.SaveChanges() > 0)
                {
                    stock.SeckillStock = stock.SeckillStock != null ? stock.SeckillStock + seckillParameter.Count : seckillParameter.Count;
                    stock.SeckillTotal = stock.SeckillTotal != null ? stock.SeckillTotal + seckillParameter.Count : seckillParameter.Count;
                    stock.Stock = stock.Stock - seckillParameter.Count;
                    _orangeContext.TbStock.Update(stock);
                    _orangeContext.SaveChanges();
                }
                else
                {
                    throw new Exception("添加秒杀商品失败");
                }

                //4.更新redis中的秒杀库存(预热到redis中)---goods展示/stock控制库存的
                //UpdateSeckillStock();
                {
                    SeckillGoods goods = new SeckillGoods();
                    goods.CurrentTime = seckillGoods.EndTime;
                    goods.Enable = seckillGoods.Enable;
                    goods.Id = seckillGoods.Id;
                    goods.Image = seckillGoods.Image;
                    goods.SeckillPrice = seckillGoods.SeckillPrice;
                    goods.SkuId = seckillGoods.SkuId;
                    goods.Stock = stock.SeckillStock;
                    goods.Title = seckillGoods.Title;
                    goods.SeckillTotal = stock.SeckillTotal;
                    this._cacheClientDB.SetEntryInHash(SeckillService.KEY_PREFIX_GOODS, seckillGoods.SkuId.ToString(), JsonConvert.SerializeObject(goods));
                    this._cacheClientDB.SetEntryInHash(SeckillService.KEY_PREFIX_STOCK, seckillGoods.SkuId.ToString(), stock.SeckillStock);
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (trans != null)
                    trans.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 更新秒杀商品数量
        /// </summary>
        public void UpdateSeckillStock()
        {
            //1.查询可以秒杀的商品
            List<SeckillGoods> seckillGoods = QuerySeckillGoods();
            if (seckillGoods == null || seckillGoods.Count == 0)
            {
                throw new Exception("没有找到可秒杀的商品");
            }


            // 如果秒杀商品存在就直接删除
            if (_cacheClientDB.GetHashKeys(SeckillService.KEY_PREFIX_STOCK) != null)
            {
                _cacheClientDB.Remove(SeckillService.KEY_PREFIX_STOCK);
            }

            //// TODO 应该把剩余库存更新到秒杀库存表
            //seckillGoods.ForEach(goods => _cacheClientDB.SetEntryInHash(SeckillService.KEY_PREFIX_STOCK, goods.SkuId.ToString(), goods.Stock.ToString()));

            seckillGoods.ForEach(goods => _cacheClientDB.SetEntryInHash(SeckillService.KEY_PREFIX_STOCK, goods.SkuId.ToString(), JsonConvert.SerializeObject(goods)));
        }

        /// <summary>
        /// 查询秒杀商品
        /// </summary>
        /// <returns></returns>
        public List<SeckillGoods> QuerySeckillGoods()
        {
            var list = _orangeContext.TbSeckillSku.AsParallel();
            // 可以秒杀 
            list = list.Where(m => m.Enable == true);
            List<TbSeckillSku> tbSeckillSkus = list.ToList();
            List<SeckillGoods> seckillGoods = new List<SeckillGoods>();
            foreach (var item in tbSeckillSkus)
            {
                var stock = _orangeContext.TbStock.Where(m => m.SkuId == item.SkuId).FirstOrDefault();
                SeckillGoods goods = new SeckillGoods();
                goods.CurrentTime = item.EndTime;
                goods.Enable = item.Enable;
                goods.Id = item.Id;
                goods.Image = item.Image;
                goods.SeckillPrice = item.SeckillPrice;
                goods.SkuId = item.SkuId;
                goods.Stock = stock.SeckillStock;
                goods.Title = item.Title;
                goods.SeckillTotal = stock.SeckillTotal;

                seckillGoods.Add(goods);
            }


            return seckillGoods;
        }
    }
}
