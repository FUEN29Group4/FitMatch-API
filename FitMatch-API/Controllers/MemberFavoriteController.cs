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


        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberFavorites(int id)
        {
            const string sql1 = @"
       SELECT DISTINCT
           m.MemberID,

           c.MemberID,
           c.MemberName,
           c.Email,

           o.MemberID,
           o.TotalPrice
       FROM MemberFavorite as m
       LEFT JOIN [Member] as c ON m.MemberID = c.MemberID
       LEFT JOIN [Order] as o ON m.MemberID = o.MemberID
       WHERE m.MemberID =  @MemberId;
    ";

            const string sql2 = @"
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
        WHERE m.MemberID = @MemberId;
    ";

            var parameters = new { MemberId = id };

            var memberFavorites1 = await _context.QueryAsync<MemberFavorite, Member, Order, MemberFavorite>(
                 sql1,
                 (memberFavorite, member, order) =>
                 {
                     if (member != null && member.MemberId != null)
                     {
                         memberFavorite.Members.Add(member);
                     }

                     if (order != null && order.MemberId != null)
                     {
                         memberFavorite.Orders.Add(order);
                     }

                     return memberFavorite;
                 },
                 param: parameters,
                 splitOn: "MemberId,MemberId"
             );


            var memberFavorites2 = await _context.QueryAsync<MemberFavorite, Trainer, Product, MemberFavorite>(
                sql2,
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
                splitOn: "TrainerId,ProductId"
            );

            if ((memberFavorites1 == null || !memberFavorites1.Any()) && (memberFavorites2 == null || !memberFavorites2.Any()))
            {
                return NotFound("No data found");
            }

            // 此處可以根據您的需求來決定如何組合或返回查詢結果。
            return Ok(new { FavoritesWithMembers = memberFavorites1, FavoritesWithTrainersAndProducts = memberFavorites2 });
        }



        //// POST api/<MemberFavoriteController>
        //[HttpPost].
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
