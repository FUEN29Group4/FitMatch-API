using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberFavoriteController : ControllerBase
    {

        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _context;//宣吿類別級變數，串資料庫
        private readonly ILogger<MemberFavoriteController> _logger; //宣吿類別級變數，串登入資訊

        //定義_context
        public MemberFavoriteController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // R: 讀取所有MemberFavorite列表資料 => ok
        // GET: api/<MemberFavoriteController>
        [HttpGet]
        public async Task<IActionResult> GetALLMemberFavorite()
        {
            const string sql = @"SELECT * FROM [MemberFavorite]";

            using (var multi = await _context.QueryMultipleAsync(sql))
            {
                var MemberFavorites = multi.Read<MemberFavorite>().ToList();
                // 基本驗證，確保資料存在
                if (MemberFavorites == null)
                {
                    return NotFound("No data found");
                }
                return Ok(MemberFavorites);
            }
        }

        //查找所有與特定MemberID相關的MemberFavorite、Trainers和Product的資料。
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberFavorites(int id)
        {
            const string sql = @"
SELECT
m.MemberID,
m.TrainerID,
m.ProductID,

t.TrainerID,
t.TrainerName,
t.Photo,
t.Introduce,

p.ProductID,
p.ProductName,
p.ProductDescription

FROM MemberFavorite as m
LEFT JOIN Trainers as t ON m.TrainerID = t.TrainerID
LEFT JOIN Product as p ON m.ProductID = p.ProductID
WHERE m.MemberID = @MemberId";

            var parameters = new { MemberId = id };
            //            這裡的檢查：

            //確保只有當trainer不為NULL且其TrainerID也不為NULL時，才將它加入Trainers列表。
            //確保只有當product不為NULL且其ProductId也不為NULL時，才將它加入Products列表。
            var memberFavorites = await _context.QueryAsync<MemberFavorite, Trainer, Product, MemberFavorite>(
    sql,
    (memberFavorite, trainer, product) =>
    {
        if (trainer != null && trainer.TrainerId != null)
        {
            memberFavorite.Trainers.Add(trainer);
        }

        if (product != null && product.ProductId != null)
        {
            memberFavorite.Products.Add(product);
        }

        return memberFavorite;
    },
                param: parameters,
                splitOn: "TrainerId,ProductId");

            // Basic validation to ensure data exists
            if (memberFavorites == null || !memberFavorites.Any())
            {
                return NotFound("No data found");
            }

            return Ok(memberFavorites);
        }







        //        //查找所有與特定MemberID相關的MemberFavorite、Trainers和Product的資料。
        //        [HttpGet("{id}")]
        //        public async Task<IActionResult> GetMemberFavorites(int id)
        //        {
        //            const string sql = @"

        //SELECT
        //    m.MemberID,
        //    m.TrainerID,
        //    m.ProductID,

        //    t.TrainerID,
        //    t.TrainerName,
        //    t.Photo,
        //    t.Introduce,

        //    p.ProductID,
        //    p.ProductName,
        //    p.ProductDescription,

        //    c.MemberID,
        //    c.MemberName,
        //    c.Email,

        //    o.MemberID,
        //    o.TotalPrice

        //FROM MemberFavorite as m
        //LEFT JOIN Trainers as t ON m.TrainerID = t.TrainerID
        ////LEFT JOIN Product as p ON m.ProductID = p.ProductID
        ////LEFT JOIN Member as c ON m.MemberID = c.MemberID
        ////LEFT JOIN [Order] as o ON m.MemberID = o.MemberID

        //WHERE m.MemberID = @MemberId
        //";

        //            var parameters = new { MemberId = id };
        ////            這裡的檢查：

        ////確保只有當trainer不為NULL且其TrainerID也不為NULL時，才將它加入Trainers列表。
        ////確保只有當product不為NULL且其ProductId也不為NULL時，才將它加入Products列表。
        //            var memberFavorites = await _context.QueryAsync<MemberFavorite, Trainer, Product, Member, Order, MemberFavorite >(
        //    sql,
        //    (memberFavorite, trainer, product, Member, Order) =>
        //    {
        //        if (trainer != null && trainer.TrainerId != null)
        //        {
        //            memberFavorite.Trainers.Add(trainer);
        //        }

        //        if (product != null && product.ProductId != null)
        //        {
        //            memberFavorite.Products.Add(product);
        //        }

        //        //if (Member != null && Member.MemberId != null)
        //        //{
        //        //    memberFavorite.Members.Add(Member);
        //        //}

        //        //if (Order != null && Order.MemberId != null)
        //        //{
        //        //    memberFavorite.Orders.Add(Order);
        //        //}

        //        return memberFavorite;
        //    },
        //                param: parameters,
        //                splitOn: "TrainerId,ProductId");

        //            // Basic validation to ensure data exists
        //            if (memberFavorites == null || !memberFavorites.Any())
        //            {
        //                return NotFound("No data found");
        //            }

        //            return Ok(memberFavorites);
        //        }







        //// POST api/<MemberFavoriteController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<MemberFavoriteController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<MemberFavoriteController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
