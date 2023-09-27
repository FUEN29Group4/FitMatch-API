using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using FitMatch_API.Models;
using static FitMatch_API.Controllers.ProductAPIController;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    public class ProductAPIController : Controller
    {


        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _context;//宣吿類別級變數，串資料庫
        private readonly ILogger<OrderAPIController> _logger; //宣吿類別級變數，串登入資訊

        //定義_context
        public ProductAPIController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }



        //讀取全部商品 => 給商城頁面使用 ok
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            const string sql = @"SELECT * FROM Product";

            using (var multi = await _context.QueryMultipleAsync(sql))
            {
                var products = multi.Read<Product>().ToList();
                // 基本驗證，確保資料存在
                if (products == null)
                {
                    return NotFound("No data found");
                }
                return Ok(products);
            }
        }



        //讀取特定商品 => 給商品詳細頁面使用 ok
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            const string sql = @"SELECT * FROM Product WHERE ProductId = @ProductId";

            

            using (var multi = await _context.QueryMultipleAsync(sql, new { ProductId = id }))
            {
                var product = multi.Read<Product>().FirstOrDefault();
                // 基本驗證，確保資料存在
                if (product == null)
                {
                    return NotFound("No data found");
                }
                return Ok(product);
            }
        }


    }
}

