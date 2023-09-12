using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json; //要記得加這個轉json的namespace
using FitMatch_API.Models;
using Dapper;
using Microsoft.AspNetCore.Cors.Infrastructure;






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

    //    //（json）字串轉物件
    //    public static T Get<T>(this ISession session, string key)
    //    {
    //        var value = session.GetString(key);
    //        return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
    //    }
    //}


    [Route("api/[controller]")]
    public class OrderAPIController : Controller
    {


        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _context;//宣吿類別級變數，串資料庫
        private readonly ILogger<OrderAPIController> _logger; //宣吿類別級變數，串登入資訊

        //定義_context
        public OrderAPIController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        ////定義_context & _logger
        //public OrderAPIController(IDbConnection context, ILogger<OrderAPIController> logger)
        //{
        //    _context = context;
        //    _logger = logger;
        //}



        //======開始製作方法function=======


        //會員訪客判斷條件
        //private bool IsUserLoggedIn()
        //{
        //    if (HttpContext.Session.GetInt32("MemberId") == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //從session取得memberId回傳
        //[HttpGet]
        //public ActionResult<int> GetMemberId()
        //{
        //    try
        //    {
        //        var memberId = HttpContext.Session.GetInt32("MemberId");
        //        if (memberId == null)
        //        {
        //            return NotFound("找不到會員編號");
        //        }
        //        return Ok(memberId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        return StatusCode(500, "Internal server error");
        //    }
        //}


        // GET: api/values
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

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



        // GET api/values/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

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

        //====================================test====================================
        // 商品列表
        [HttpGet("/api/Order/Index")]
        public IActionResult Index()
        {
            // 调用后端API获取商品列表
            // 这里可以使用HttpClient来调用API

            List<OrderViewModel> products = new List<OrderViewModel>
        {
            new OrderViewModel { ProductId = 1, ProductName = "商品1", Price = 100 },
            new OrderViewModel { ProductId = 2, ProductName = "商品2", Price = 200 },
            // 添加更多商品
        };

            return Ok("也許成功?");
        }


        // 加入购物车
        [HttpPost("/api/Order/AddToCart")]
        public IActionResult AddToCart(int productId)
        {
            // 在前端使用JavaScript将商品添加到本地存储购物车
            // 这里省略前端JavaScript代码
            return RedirectToAction("Index");
        }

        // 查看购物车
        [HttpGet("/api/Order/ViewCart")]
        public IActionResult ViewCart()
        {
            // 从本地存储获取购物车数据
            // 这里省略前端JavaScript代码
            return View();
        }


        // 更新购物车
        [HttpPut("/api/Order/UpdateCart")]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            // 在前端使用JavaScript更新购物车
            // 这里省略前端JavaScript代码
            return RedirectToAction("ViewCart");
        }

        // 结账
        [HttpPost("/api/OrderCheckout")]
        public IActionResult Checkout()
        {
            // 将购物车数据发送到后端API并创建订单
            // 这里可以使用HttpClient来调用API
            // 前端在提交订单后清空本地存储的购物车数据

            return RedirectToAction("Index");
        }



        //====================================test====================================



        //[HttpPost]
        //public async Task<IActionResult> Checkout([FromBody] List<OrderViewModel> orderItems)
        //{
        //    try
        //    {
        //        // 创建一个新订单
        //        var newOrder = new OrderViewModel
        //        {
        //            MemberId = orderItems[0].MemberId,
        //            OrderId = orderItems.Count,
        //            OrderDetailId = orderItems.Count,
        //            ProductId = orderItems.Count,
        //            OrderTime = DateTime.Now,
        //            Quantity = orderItems.Count,
        //            ProductName = string.Join(", ", orderItems.Select(item => item.ProductName)), // 將商品名稱串聯為字串
        //            Price = orderItems.Count,
        //            TotalPrice = orderItems.Sum(item => item.Price * item.Quantity) // 計算總價

        //            // 其他订单属性的赋值
        //        };

        //        //// 将订单项添加到新订单中
        //        //foreach (var orderItem in orderItems)
        //        //{
        //        //    var product = await _context.Products.FindAsync(orderItem.ProductId);

        //        //    if (product != null)
        //        //    {
        //        //        // 创建订单详情
        //        //        var orderDetail = new OrderDetail
        //        //        {
        //        //            ProductId = product.Id, // 假设你有一个 Id 属性来表示产品的唯一标识
        //        //            Quantity = orderItem.Quantity,
        //        //            OrderId = newOrder.OrderId,
        //        //            OrderDetailId = newOrder.OrderDetailId


        //        //            // 其他订单详情属性的赋值
        //        //        };

        //        //        // 将订单详情添加到新订单中
        //        //        newOrder.OrderDetailId.Add(orderDetail);

        //        //        _context.Carts.Add(cart);
        //        //        _context.SaveChanges();
        //        //    }
        //        //}

        //        //// 保存订单到数据库
        //        //_context.Orders.Add(newOrder);
        //        //await _context.SaveChangesAsync();

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









        // POST api/values
        // C: 創立資料
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        // U: 更新編輯資料
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        // D: 刪除資料
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

