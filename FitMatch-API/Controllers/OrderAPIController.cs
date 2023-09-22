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
using static FitMatch_API.Models.MemberFavorite;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;





// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
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



        // 彤: 讀取所有Order列表資料 => ok
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

        // 彤: 讀取特定的Order資料 => ok
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

                // #return Ok(order);
                return Ok(order);
            }
        }


        // 彤: # 讀取特定的OrderViewModel資料 => 
        //[HttpGet("orderviewmodel")]
        //public async Task<IActionResult> GetOrderViewModel(int id)
        //{
        //    const string sql = @"SELECT * FROM [Order] WHERE OrderId = @OrderId";
        //    var parameters = new { OrderId = id };

        //    using (var multi = await _context.QueryMultipleAsync(sql, parameters))
        //    {
        //        var order = multi.Read<OrderViewModel>().FirstOrDefault();

        //        // 基本驗證，確保資料存在
        //        if (order == null)
        //        {
        //            return NotFound("No data found");
        //        }

        //        // #return Ok(order);
        //        return Ok(order);
        //    }
        //}


        //##讀取特定的OrderViewModel資料：Dapper 測試中～～～～～
        [HttpGet("orderviewmodel")]
        public async Task<IActionResult> GetOrderViewModel(int id)
        {
            const string sql = @"
            SELECT 
                o.OrderId,
                o.MemberId,
                o.TotalPrice,
                o.OrderTime,
                
                odi.Quantity,
               
                m.MemberName,
                m.Phone,
                m.Address,
                m.Email

            FROM [Order] AS o
            LEFT JOIN OrderDetail AS odi ON o.OrderId = odi.OrderId
            
            LEFT JOIN Member AS m ON o.MemberId = m.MemberId
            WHERE o.OrderId = @OrderId;";

            var parameters = new { OrderId = id };
           


            using (var multi = await _context.QueryMultipleAsync(sql, parameters))
            {

                
                var order = multi.Read<OrderViewModel>().FirstOrDefault();

                if (order == null)
                {
                    return NotFound("No data found");
                }

                return Ok(order);
            }
        }





        //彤： 建立訂單  => ok 
        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderViewModel orderViewModel)
        {
            // 在这里处理接收到的订单信息
            // orderViewModel 包含了订单相关的数据，例如MemberId、TotalPrice、PaymentMethod、ShippingMethod等
            // 驗證數據
            if (orderViewModel == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }
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

        }









        //＝＝＝＝＝＝＝＝ 以上給欣彤 ＝＝＝＝＝ 以下給承蔚 ＝＝＝＝＝＝＝＝＝


        //查會員訂單
        [HttpGet("Order")]
        public async Task<IActionResult> GetMemberOrder(int id)
        {

            if (id <= 0)
            {
                return BadRequest("Invalid ID provided.");
            }

            //會員訂單
            const string sqlOrder = @"
              SELECT
                   o.OrderID,
                   o.MemberID AS oMemberId,
                   o.TotalPrice,
                   o.OrderTime,
                   o.PaymentMethod,
                   o.ShippingMethod,
                   o.PayTime,

                   m.MemberID AS mMemberId,
                   m.MemberName
                FROM [Order] AS o
            
                LEFT JOIN Member AS m ON o.MemberID = m.MemberID
            
                WHERE o.MemberID = @MemberId;
            ";

            var parameters = new { MemberId = id };

            //會員訂單
            var MemberOrder = await _context.QueryAsync<Order, Member, Order>(
               sqlOrder,
               (memberorder, member) =>
               {
                   if (member != null && member.MemberId != null)
                   {
                       memberorder.Members.Add(member);
                   }
                   return memberorder;
               },
               param: parameters,
               splitOn: "oMemberId,mMemberId"
           );

            if ((MemberOrder == null || !MemberOrder.Any()))
            {
                return NotFound("No data found");
            }

            // 打包資料
            return Ok(new
            {
                OrderWithMember = MemberOrder
            });

        }

        //查會員訂單明細
        [HttpGet("OrderDetail")]
        public async Task<IActionResult> GetMemberOrderDetail(int oid)
        {
            //查訂單明細
            const string sqlOrderDetail = @"
              SELECT
                  od.OrderID,
                  od.OrdetailId,
                  od.ProductId,
                  od.Quantity,
                  

                  p.ProductId,
                  p.Productname,
                  p.Price,
                  p.Photo

                   FROM OrderDetail AS od

                 LEFT JOIN Product AS p ON od.ProductId = p.ProductId

                 WHERE od.OrderID = @OrderID;
                ";
            var parameters = new { OrderID = oid };

            //查訂單明細
            var MemberOrderDetail = await _context.QueryAsync<Order, Product, Order>(
               sqlOrderDetail,
               (orderdetail, product) =>
               {
                   if (product != null && product.ProductId != null)
                   {
                       orderdetail.Products.Add(product);
                   }
                   return orderdetail;
               },
               param: parameters,
               splitOn: "OrdetailId"
           );

            if ((MemberOrderDetail == null || !MemberOrderDetail.Any()))
            {
                return NotFound("No data found");
            }

            // 打包資料
            return Ok(new
            {
                OrderDetailWithMember = MemberOrderDetail
            });

        }



        //======= 建立訂單 test  =======
        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderViewModel orderViewModel)
        {
            // 在这里处理接收到的订单信息
            // orderViewModel 包含了订单相关的数据，例如MemberId、TotalPrice、PaymentMethod、ShippingMethod等
            // 驗證數據
            if (orderViewModel == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }
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
            
        }
        [HttpGet("orderviewmodel")]
        public async Task<IActionResult> GetOrderViewModel(int id)
        {
            const string sql = @"
        SELECT 
            o.OrderId,
            o.MemberId,
            o.TotalPrice,
            o.OrderTime,
            o.PaymentMethod,
            o.ShippingMethod,
            o.PayTime,
            o.Status,
            odi.OrderDetailId,
            odi.CartItems,
            odi.ProductId,
            odi.Quantity,
            odi.ProductName,
            odi.ProductDescription,
            odi.Price,
            odi.TypeId,
            odi.ProductInventory,
            odi.Approved,
            odi.Photo,
            t.TypeName,
            m.MemberName,
            m.Phone,
            m.Address,
            m.Email
        FROM [Order] AS o
        LEFT JOIN OrderDetail AS odi ON o.OrderId = odi.OrderId
        LEFT JOIN ProductType AS t ON odi.TypeId = t.TypeId
        LEFT JOIN Member AS m ON o.MemberId = m.MemberId
        WHERE o.OrderId = @OrderId;";

            var parameters = new { OrderId = id };

            using (var connection = new SqlConnection("YourConnectionStringHere"))
            {
                connection.Open();
                var multi = await connection.QueryMultipleAsync(sql, parameters, commandType: CommandType.Text);

                var order = multi.Read<OrderViewModel>().FirstOrDefault();

                if (order == null)
                {
                    return NotFound("No data found");
                }

                return Ok(order);
            }
        }
    }
}






























