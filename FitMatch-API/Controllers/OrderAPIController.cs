using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FitMatch_API.Models;
using Dapper;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;

//Debug類要加的命名空間
using System.Diagnostics;





// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{


    //=========做一個session，記錄一些暫存的資料（登入/購物車）=========
    //public static class SessionExtensions
    //{
    //    //物件轉字串（json）
    //    public static void Set<T>(this ISession session, string key, T value)
    //    {
    //        session.SetString(key, JsonSerializer.Serialize(value));
    //    }

    //（json）字串轉物件
    //    public static T Get<T>(this ISession session, string key)
    //    {
    //        var value = session.GetString(key);
    //        return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
    //    }
    //}


    [Route("api/[controller]")]
    public partial class OrderAPIController : Controller
    {


        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _context;//宣吿類別級變數，串資料庫
        private readonly ILogger<OrderAPIController> _logger; //宣吿類別級變數，串登入資訊

        //定義_context
        public OrderAPIController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // R: 讀取所有Order列表資料 => ok
        [HttpGet]
        public async Task<IActionResult> GetALLOrder()
        {
            const string sql = @"SELECT * FROM [Order]";

            using (var multi = await _context.QueryMultipleAsync(sql))
            {
                var orders = multi.Read<Order>().ToList();
                // 基本驗證，確保資料存在
                if (orders == null)
                {
                    return NotFound("No data found");
                }
                return Ok(orders);
            }
        }

        // R: 讀取特定的Order資料 => ok
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            const string sql = @"SELECT * FROM [Order] WHERE OrderId = @OrderId";
            var parameters = new { OrderId = id };

            using (var multi = await _context.QueryMultipleAsync(sql, parameters))
            {
                var order = multi.Read<Order>().FirstOrDefault();
                // 基本驗證，確保資料存在
                if (order == null)
                {
                    return NotFound("No data found");
                }
                return Ok(order);
            }
        }






        //======= 建立訂單 test  =======
        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderViewModel orderViewModel)
        {
            // 在这里处理接收到的订单信息
            // orderViewModel 包含了订单相关的数据，例如MemberId、TotalPrice、PaymentMethod、ShippingMethod等

            // 1. 创建新订单
            var newOrder = new Order
            {
                MemberId = orderViewModel.MemberId,
                TotalPrice = orderViewModel.TotalPrice,
                OrderTime = DateTime.Now, // 您可以根据需要设置订单时间
                PaymentMethod = orderViewModel.PaymentMethod,
                ShippingMethod = orderViewModel.ShippingMethod
            };
            // 使用Dapper執行插入操作，並獲取訂單編號
            int orderId = _context.ExecuteScalar<int>(@"INSERT INTO [Order] (MemberID, TotalPrice, OrderTime, PaymentMethod, ShippingMethod)
        VALUES (@MemberId, @TotalPrice, @OrderTime, @PaymentMethod, @ShippingMethod);
        SELECT SCOPE_IDENTITY();", newOrder);

            // 2. 將訂單明細寫入數據庫
            foreach (var item in orderViewModel.CartItems) // 注意這裡使用CartItems而不是OrderDetailIds
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity // 購物車中的數量
                };

                // 使用Dapper執行插入操作
                _context.Execute(@"INSERT INTO OrderDetail (OrderId, ProductId, Quantity)
                            VALUES (@OrderId, @ProductId, @Quantity)", orderDetail);
            }

            // 返回成功或其他適當的響應
            return Ok(new { OrderId = orderId });
            //// 2. 创建訂單明細
            //var newOrderDetail = new OrderDetail
            //{
            //    OrderDetailId = orderViewModel.OrderDetailId,
            //    OrderId = orderViewModel.OrderId,
            //    ProductId = orderViewModel.ProductId,
            //    Quantity = orderViewModel.Quantity,

            //};

            //// 使用Dapper执行插入操作，并获取订单编号
            //int orderId = _context.ExecuteScalar<int>(@"INSERT INTO [Order] (MemberID, TotalPrice, OrderTime, PaymentMethod, ShippingMethod)
            //  VALUES (@MemberId, @TotalPrice, @OrderTime, @PaymentMethod, @ShippingMethod);
            //  SELECT SCOPE_IDENTITY();", newOrder);

            //// 2. 将订单明细写入数据库
            //foreach (var item in orderViewModel.OrderDetailIds)
            //{
            //    var orderDetail = new OrderDetail
            //    {
            //        OrderId = orderId,
            //        ProductId = item.ProductId,
            //        Quantity = item.Quantity
            //    };

            //    // 使用Dapper执行插入操作
            //    _context.Execute(@"INSERT INTO OrderDetail (OrderId, ProductId, Quantity)
            //                        VALUES (@OrderId, @ProductId, @Quantity)", orderDetail);
            //}

            //// 返回成功或其他适当的响应
            //return Ok(new { OrderId = orderId }); // 返回订单编号或其他信息
        }
    }
}

//======= 建立訂單 OrderId   Start =======
//[HttpPost("create-order")]
//public IActionResult CreateOrder1([FromBody] OrderViewModel orderViewModel)
//{
//    try
//    {
//        // 从JWT令牌中获取会员ID
//        var memberIdClaim = User.Claims.FirstOrDefault(c => c.Type == "MemberId");

//        if (memberIdClaim != null && int.TryParse(memberIdClaim.Value, out int memberId))
//        {
//            //// 从localStorage中获取购物车内容
//            //var cartItems = GetCartItemsFromLocalStorage();



//            if (!memberId = null)
//            {
//                // 创建订单并保存到数据库
//                var newOrderDetail = new OrderDetail
//                {
//                    OrderDetailId = orderViewModel.OrderDetailId,
//                    OrderId = orderViewModel.OrderId,
//                    ProductId = orderViewModel.ProductId,
//                    Quantity = orderViewModel.Quantity,

//                };

//                var newOrder = new Order
//                {
//                    OrderId = orderViewModel.OrderId,
//                    MemberId = memberId,
//                    TotalPrice = orderViewModel.TotalPrice,
//                    OrderTime = DateTime.Now, // 您可以根据需要设置订单时间
//                    PaymentMethod = orderViewModel.PaymentMethod,
//                    ShippingMethod = orderViewModel.ShippingMethod,
//                    PayTime = orderViewModel.PayTime
//                };


//                _context.Order.Add(newOrder);
//                _context.SaveChanges();

//                // 清空localStorage中的购物车内容
//                ClearCartInLocalStorage();

//                return Ok(new { OrderId = newOrder.OrderId });
//            }


//            else
//            {
//                return BadRequest("购物车为空，无法创建订单。");
//            }
//        }
//        else
//        {
//            return Unauthorized("未授权的访问，用户未登录或身份验证失败。");
//        }
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, "Internal server error");
//    }
//}

//// 从localStorage中获取购物车内容
//private List<CartItem> GetCartItemsFromLocalStorage()
//{

//    // 从localStorage中获取购物车内容并将其转换为CartItem对象的列表
//    // 根据您的localStorage数据结构编写适当的逻辑来解析购物车内容
//    List<CartItem> cartItems = new List<CartItem>();

//    // 例如，假设您的localStorage中的数据结构如下所示：
//    // key: "cartItem1", value: "Product1|100|2"
//    // key: "cartItem2", value: "Product2|50|1"

//    // 您可以遍历localStorage中的所有键，将其解析为CartItem对象，并添加到列表中

//    for (int i = 0; i < localStorage.length; i++)
//    {
//        string key = localStorage.key(i);
//        string value = localStorage.getItem(key);

//        // 解析购物车项的逻辑，将其添加到cartItems列表中
//        // 例如，根据上述的假设数据结构，您可以使用Split方法进行解析
//        string[] parts = value.Split('|');

//        if (parts.Length == 3)
//        {
//            string productName = parts[0];
//            decimal totalprice = decimal.Parse(parts[1]);
//            int quantity = int.Parse(parts[2]);

//            cartItems.Add(new CartItem { ProductName = productName, TotalPrice = totalprice, Quantity = quantity });
//        }
//    }

//    return cartItems;
//}

//// 清空localStorage中的购物车内容
//private void ClearCartInLocalStorage()
//{
//    // 根据您的localStorage数据结构编写适当的逻辑来清空购物车内容
//    // 遍历localStorage中的所有键，并使用removeItem方法将其删除
//    for (int i = 0; i < localStorage.length; i++)
//    {
//        string key = localStorage.key(i);
//        localStorage.removeItem(key);
//    }
//}



//======= 建立OrderId test  end =======















//[HttpPost]
//public async Task<IActionResult> Checkout([FromBody] List<OrderViewModel> orderItems)
//{
//    try
//    {
//        创建一个新订单
//       var newOrder = new OrderViewModel
//       {
//           MemberId = orderItems[0].MemberId,
//           OrderId = orderItems.Count,
//           OrderDetailId = orderItems.Count,
//           ProductId = orderItems.Count,
//           OrderTime = DateTime.Now,
//           Quantity = orderItems.Count,
//           ProductName = string.Join(", ", orderItems.Select(item => item.ProductName)), // 將商品名稱串聯為字串
//           Price = orderItems.Count,
//           TotalPrice = orderItems.Sum(item => item.Price * item.Quantity) // 計算總價


//            其他订单属性的赋值
//       };

//        // 将订单项添加到新订单中
//        foreach (var orderItem in orderItems)
//        {
//            var product = await _context.Products.FindAsync(orderItem.ProductId);

//            if (product != null)
//            {
//                // 创建订单详情
//                var orderDetail = new OrderDetail
//                {
//                    ProductId = product.Id, // 假设你有一个 Id 属性来表示产品的唯一标识
//                    Quantity = orderItem.Quantity,
//                    OrderId = newOrder.OrderId,
//                    OrderDetailId = newOrder.OrderDetailId


//                    // 其他订单详情属性的赋值
//                };

//                // 将订单详情添加到新订单中
//                newOrder.OrderDetailId.Add(orderDetail);

//                _context.Carts.Add(cart);
//                _context.SaveChanges();
//            }
//        }

//        // 保存订单到数据库
//        _context.Orders.Add(newOrder);
//        await _context.SaveChangesAsync();

//        return Ok(new { message = "订单已成功创建" });
//    }

//    catch (Exception ex)
//    {
//        return BadRequest(new { message = "创建订单时出错：" + ex.Message });
//    }
//}























////讀取購物車所有資料
//[HttpGet("/api/OrderAPI/GetCart")]
//public async Task<ActionResult<CartViewModel>> GetCart()
//{
//    try
//    {
//        var memberId = HttpContext.Session.GetInt32("MemberId");
//        var cart = await Read<Cart>().ToList();

//        if (cart == null)
//        {
//            return NotFound("找不到此購物車");
//        }

//        var cartItemViewModels = cart.CartItems.Select(item => new CartItemViewModel
//        {
//            ProductId = (int)item.ProductId,
//            ProductName = item.Product?.ProductName,
//            UnitPrice = item.Product.UnitPrice,
//            Quantity = (int)item.Quantity
//        }).ToList();

//        var cartViewModel = new CartViewModel
//        {
//            CartId = cart.CartId,
//            Items = cartItemViewModels
//        };

//        return Ok(cartViewModel);
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "取得購物車時發生錯誤。");
//        return StatusCode(500, "內部伺服器錯誤");
//    }
//}





