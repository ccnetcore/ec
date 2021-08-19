﻿using CC.ElectronicCommerce.Common;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Model.DTO;
using CC.ElectronicCommerce.StockInterface;
using CC.ElectronicCommerce.StockModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CC.ElectronicCommerce.StockServiceService
{
    public class StockService : IStockService
    {
        private OrangeStockContext _orangeStockContext;
        private readonly CacheClientDB _cacheClientDB;

        public StockService(OrangeStockContext orangeStockContext, CacheClientDB cacheClientDB)
        {
            this._orangeStockContext = orangeStockContext;
            this._cacheClientDB = cacheClientDB;
        }

        /// <summary>
        /// 增加库存
        /// </summary>
        /// <param name="spu"></param>
        public void IncreaseStock()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 恢复库存，以订单为单位
        /// </summary>
        /// <param name="spu"></param>
        public void ResumeStock(List<CartDto> cartDtos, long orderId)
        {
            IDbContextTransaction trans = null;
            try
            {
                trans = this._orangeStockContext.Database.BeginTransaction();
                if (!this._orangeStockContext.Set<TbStockLog>().Any(l => l.OrderId == orderId && l.StockStatus == (int)TbStockLogEnum.StockStatus.Normal))
                {
                    Console.WriteLine("订单需要恢复的库存流水不存在");//可以细致处理
                    return;
                }

                foreach (CartDto cartDto in cartDtos)
                {
                    int count = _orangeStockContext.Database.ExecuteSqlRaw($"update tb_stock set stock = stock + {cartDto.num} where sku_id = {cartDto.skuId}");
                    if (count != 1)
                    {
                        throw new Exception("恢复库存失败");
                    }
                    int countLog = _orangeStockContext.Database.ExecuteSqlRaw($"update tb_stock_log set stock_status ={(int)TbStockLogEnum.StockStatus.Backoff} where sku_id = {cartDto.skuId} and order_id={orderId} and stock_status={(int)TbStockLogEnum.StockStatus.Normal}");
                    if (countLog != 1)
                    {
                        throw new Exception("恢复库存失败");
                    }
                }
                trans.Commit();

                foreach (CartDto cartDto in cartDtos)
                {
                    #region 增加Redis库存
                    string key = $"{CommonConfigConstant.StockRedisKeyPrefix}{cartDto.skuId}";
                    this._cacheClientDB.IncrementValueBy(key, cartDto.num);
                    #endregion
                }

            }
            catch (Exception ex)
            {
                if (trans != null)
                {
                    Console.WriteLine(ex);
                    trans.Rollback();
                }
                throw;
            }
            finally
            {
                trans.Dispose();
            }
        }

        /// <summary>
        /// 减少库存
        /// </summary>
        /// <param name="cartDtos"></param>
        public void DecreaseStock(List<CartDto> cartDtos, long orderId)
        {
            IDbContextTransaction trans = null;
            try
            {

                trans = this._orangeStockContext.Database.BeginTransaction();
                foreach (CartDto cartDto in cartDtos)
                {
                    int count = _orangeStockContext.Database.ExecuteSqlRaw($"update tb_stock set stock = stock - {cartDto.num} where sku_id = {cartDto.skuId} and stock >= {cartDto.num}");
                    if (count != 1)
                    {
                        throw new Exception("扣减库存失败");
                    }
                    TbStockLog tbStockLog = new TbStockLog()
                    {
                        SeckillNum = 0,
                        SkuId = cartDto.skuId,
                        OrderId = orderId,
                        OrderNum = cartDto.num,
                        StockStatus = (int)TbStockLogEnum.StockStatus.Normal,
                        StockType = (int)TbStockLogEnum.StockType.Decrease
                    };
                    this._orangeStockContext.Set<TbStockLog>().Add(tbStockLog);
                }
                this._orangeStockContext.SaveChanges();
                trans.Commit();
            }
            catch (Exception ex)
            {
                if (trans != null)
                {
                    Console.WriteLine(ex);
                    trans.Rollback();
                }
                throw;
            }
            finally
            {
                trans.Dispose();
            }
        }
    }
}
