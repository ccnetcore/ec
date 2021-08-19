using CC.ElectronicCommerce.Common;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.StockInterface;
using CC.ElectronicCommerce.StockModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace   CC.ElectronicCommerce.StockServiceService
{
    public class StockManagerService : IStockManagerService
    {
        private OrangeStockContext _orangeStockContext;
        private readonly CacheClientDB _cacheClientDB;

        public StockManagerService(OrangeStockContext orangeStockContext, CacheClientDB cacheClientDB)
        {
            this._orangeStockContext = orangeStockContext;
            this._cacheClientDB = cacheClientDB;
        }

        #region InitRedisStock
        private static readonly object InitRedisStock_Lock = new object();
        private static bool Is_InitRedisStock = false;

        /// <summary>
        /// 初始化全局的库存
        /// 锁定+单例----TODO 分布式锁
        /// </summary>
        public void InitRedisStock()
        {
            if (!Is_InitRedisStock)
            {
                lock (InitRedisStock_Lock)
                {
                    if (!Is_InitRedisStock)
                    {
                        this.InitRedisStockCore();
                    }
                }
            }
        }

        private void InitRedisStockCore()
        {
            int index = 1;
            int size = 100;
            int originalSize = size;
            while (size == originalSize)
            {
                List<TbStock> stockList = this._orangeStockContext.TbStock.Where(s => s.SkuId > 0)
                                                                            .OrderBy(s => s.SkuId)
                                                                            .Skip((index - 1) * size)
                                                                            .Take(size)
                                                                            .ToList();
                foreach (var stock in stockList)
                {
                    string key = $"{CommonConfigConstant.StockRedisKeyPrefix}{stock.SkuId}";
                    if (!this._cacheClientDB.ContainsKey(key))
                    {
                        this._cacheClientDB.Add<long>(key, stock.Stock);
                    }
                }
                index++;
                size = stockList.Count;
            }
        }
        #endregion

        public void ForceInitRedisStockBySkuId(long skuId)
        {
            var stock = this._orangeStockContext.TbStock.First(s => s.SkuId == skuId);
            string key = $"{CommonConfigConstant.StockRedisKeyPrefix}{stock.SkuId}";
            this._cacheClientDB.Set<long>(key, stock.Stock);
        }

    }
}
