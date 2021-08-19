using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Model.DTO;


namespace CC.ElectronicCommerce.Interface
{
    public interface IOrderService
    {
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="orderDto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        long CreateOrder(OrderDto orderDto, UserInfo user);
        /// <summary>
        /// 更新订单的支付状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderPayStatus"></param>
        void UpdateOrderStatus(long orderId, int orderPayStatus);

        /// <summary>
        /// 获取订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        int? GetOrderStatus(long orderId);
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool CloseOrder(long orderId);

        /// <summary>
        /// 根据订单号，查询订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        TbOrder QueryById(long orderId);

        /// <summary>
        /// 分页查询订单信息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        PageResult<TbOrder> QueryOrderByPage(int page, int rows);


    }
}
