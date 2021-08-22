using CC.ElectronicCommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Interface
{
	public interface ISeckillService
	{
		/**
         * 发送秒杀信息到队列当中接口方法
         * @param seckillDTO
         */
		void sendMessage(SeckillDTO seckillDTO);

		/**
         * 根据用户id查询秒杀订单接口方法
         * @param userId
         * @return
         */
		long? checkSeckillOrder(long goodsId, long userId);


		/**
         * 创建秒杀地址接口方法
         * @param goodsId
         * @param id
         * @return
         */
		string createPath(long goodsId, long id);

		/**
         * 验证秒杀地址接口方法
         * @param goodsId
         * @param id
         * @param path
         * @return
         */
		bool checkSeckillPath(long goodsId, long id, string path);

        /**
         * 创建验证码
         */
        byte[] createVerifyCode(UserInfo user, string goodsId);

		/**
         * 校验验证码
         */
		bool checkVerifyCode(UserInfo user, long goodsId, string verifyCode);

		/**
         * 查询可以秒杀的商品信息
         * @return
         */
		List<SeckillGoods> querySecKillList();

		/**
         * 从缓存中获取秒杀的商品详情
         * @param goodsId
         * @return
         */
		SeckillGoods queryGoodsInfoFormCache(long goodsId);
	}

}
