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
    m.MemberID AS mMemberId,
    c.MemberID AS cMemberId,
    c.MemberName,
    c.Email
FROM MemberFavorite as m
LEFT JOIN [Member] as c ON m.MemberID = c.MemberID
WHERE m.MemberID = @MemberId;
    ";

            const string sql2 = @"
SELECT
    m.MemberID,
    m.TrainerID,

    t.TrainerID,
    t.TrainerName,
    t.Photo,
    t.Introduce

FROM MemberFavorite as m
LEFT JOIN Trainers as t ON m.TrainerID = t.TrainerID 
WHERE m.MemberID = @MemberId AND m.TrainerID IS NOT NULL;
    ";

            const string sql4 = @"
        SELECT
            m.MemberID,
            m.ProductID,


            p.ProductID,
            p.ProductName,
            p.ProductDescription,
            p.Photo

        FROM MemberFavorite as m

        LEFT JOIN Product as p ON m.ProductID = p.ProductID
        WHERE m.MemberID = @MemberId  AND m.ProductID IS NOT NULL;
    ";


            const string sql3 = @"
        SELECT 
            MemberID,
            SUM(TotalPrice) AS TotalOrderAmount
        FROM 
            [Order]
        WHERE 
            MemberID = @MemberId
        GROUP BY 
            MemberID;
    ";



            var parameters = new { MemberId = id };

            //找會員資訊
            var memberFavorites1 = await _context.QueryAsync<MemberFavorite, Member, MemberFavorite>(
                 sql1,
                 (memberFavorite, member) =>
                 {
                     if (member != null && member.MemberId != null)
                     {
                         memberFavorite.Members.Add(member);
                     }


                     return memberFavorite;
                 },
                 param: parameters,
                 splitOn: "mMemberId,cMemberId"
             );

            //找教練資訊
            var memberFavorites2 = await _context.QueryAsync<MemberFavorite, Trainer, MemberFavorite>(
                sql2,
                (memberFavorite, trainer) =>
                {
                    if (trainer != null && trainer.TrainerId != null)
                    {
                        memberFavorite.Trainers.Add(trainer);
                    }


                    return memberFavorite;
                },
                param: parameters,
                splitOn: "TrainerId"
            );
            //找商品資訊
            var memberFavorites4 = await _context.QueryAsync<MemberFavorite, Product, MemberFavorite>(
               sql4,
               (memberFavorite, product) =>
               {
                 

                   if (product != null && product.ProductId != null)
                   {
                       memberFavorite.Products.Add(product);
                   }

                   return memberFavorite;
               },
               param: parameters,
               splitOn: "ProductId"
           );

            //找會員年度總金額
            var totalOrderAmountResult = await _context.QueryFirstOrDefaultAsync<dynamic>(sql3, parameters);

            if ((memberFavorites1 == null || !memberFavorites1.Any()) && (memberFavorites2 == null || !memberFavorites2.Any()) && totalOrderAmountResult == null && (memberFavorites4 == null || !memberFavorites4.Any()))
            {
                return NotFound("No data found");
            }

            return Ok(new
            {
                FavoritesWithMembers = memberFavorites1,
                FavoritesWithTrainersAndProducts1 = memberFavorites2,
                FavoritesWithTrainersAndProducts2 = memberFavorites4,
                TotalOrderAmount = totalOrderAmountResult?.TotalOrderAmount ?? 0 // 使用 null propagation 檢查是否為 null，如果為 null 則返回 0
            });

        }


        //刪除
        // DELETE api/<MemberFavoriteController>/5
        // DELETE api/<MemberFavoriteController>/memberId/product/productId
        [HttpDelete("{memberId}/product/{productId}")]
        public async Task<IActionResult> DeleteProductFavorite(int memberId, int productId)
        {
            // Define SQL statement to delete data with specific MemberID and ProductID
            const string sql = @"DELETE FROM MemberFavorite WHERE MemberID = @MemberID AND ProductID = @ProductID";

            // Execute the SQL statement
            var affectedRows = await _context.ExecuteAsync(sql, new { MemberID = memberId, ProductID = productId });

            // Check if any row was deleted
            if (affectedRows > 0)
            {
                return Ok($"Deleted {affectedRows} record(s).");
            }
            else
            {
                return NotFound($"No record found with MemberID: {memberId} and ProductID: {productId}");
            }
        }

        // DELETE api/<MemberFavoriteController>/memberId/trainer/trainerId
        [HttpDelete("{memberId}/trainer/{trainerId}")]
        public async Task<IActionResult> DeleteTrainerFavorite(int memberId, int trainerId)
        {
            // Define SQL statement to delete data with specific MemberID and TrainerID
            const string sql = @"DELETE FROM MemberFavorite WHERE MemberID = @MemberID AND TrainerID = @TrainerID";

            // Execute the SQL statement
            var affectedRows = await _context.ExecuteAsync(sql, new { MemberID = memberId, TrainerID = trainerId });

            // Check if any row was deleted
            if (affectedRows > 0)
            {
                return Ok($"Deleted {affectedRows} record(s).");
            }
            else
            {
                return NotFound($"No record found with MemberID: {memberId} and TrainerID: {trainerId}");
            }
        }

    }
}
