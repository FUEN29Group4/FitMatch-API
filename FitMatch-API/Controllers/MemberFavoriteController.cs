using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;
using static FitMatch_API.Controllers.MemberFavoriteController;

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











        //============== 商品收藏 start ==========
        [HttpPost("FavoriteProduct")]
        public async Task<IActionResult> InsertFavoriteTrainer([FromBody] FavoriteProduct favoriteProduct)
        {//接收教練收藏
            if (favoriteProduct == null)
            {
                return BadRequest("找不到data");
            }
            string sql = @"insert into MemberFavorite(MemberID,ProductID) values (@MemberId,@ProductId);";

            var parameters = new
            {
                MemberId = favoriteProduct.MemberId,
                ProductId = favoriteProduct.ProductId,
            };
            try
            {
                int rowsAffected = await _context.ExecuteAsync(sql, parameters);

                if (rowsAffected > 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "商品收藏成功"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"内部服务器错误: {ex.Message}"
                });
            }

            return BadRequest("商品無法加入收藏");
        }
        // 接收的模型
        public class FavoriteProduct
        {
            public int MemberId { get; set; }
            public int ProductId { get; set; }
        }







        [HttpGet("GetFavoriteProduct/{id}")]
        public async Task<IActionResult> GetFavoriteProduct(int id)
        {//查詢收藏
            try
            {
                // Your SQL query to retrieve all records for the given MemberId
                string sqlQuery = "SELECT * FROM MemberFavorite WHERE MemberId = @MemberId AND (TrainerId IS NOT NULL OR ProductId IS NOT NULL)";

                // Execute the query using Dapper
                var favoriteRecords = await _context.QueryAsync<SimpleMemberFavorite>(sqlQuery, new { MemberId = id });

                if (favoriteRecords != null && favoriteRecords.Any())
                {
                    var response = new
                    {
                        status = "success",
                        message = "商品收藏查詢成功",
                        data = favoriteRecords
                    };

                    return Ok(favoriteRecords);
                }
                else
                {
                    var response = new
                    {
                        status = "error",
                        message = "找不到具有TrainerId或ProductId的紀錄"
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    status = "error",
                    message = $"內部服務器錯誤: {ex.Message}"
                };
                return StatusCode(500, response);
            }
        }
        //自訂模型 因為原本模型太多東西了
        public class SimpleMemberFavorite
        {
            public int MemberFavoriteId { get; set; }
            public int? MemberId { get; set; }
            public int? TrainerId { get; set; }
            public int? ProductId { get; set; }
        }





        //============== 商品收藏 end  ==========













        // 讀取所有MemberFavorite列表資料 => ok
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
            //尋找會員資料
            const string sql1 = @"
                SELECT DISTINCT
                    m.MemberID AS mMemberId,
                    c.MemberID AS cMemberId,
                    c.MemberName,
                    c.Email
                FROM 
                    MemberFavorite AS m
                LEFT JOIN 
                    [Member] AS c ON m.MemberID = c.MemberID
                WHERE 
                    m.MemberID = @MemberId;
                ";
                        
            //尋找會員年度花費
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
            //尋找收藏教練
            const string sql2 = @"
                SELECT 
                    m.MemberID,
                    m.TrainerID,
                    t.TrainerID,
                    t.TrainerName,
                    t.Photo,
                    t.Introduce
                FROM 
                    MemberFavorite AS m
                LEFT JOIN 
                    Trainers AS t ON m.TrainerID = t.TrainerID 
                WHERE 
                    m.MemberID = @MemberId AND m.TrainerID IS NOT NULL;
                ";
            //尋找收藏商品
            const string sql4 = @"
                SELECT 
                    m.MemberID,
                    m.ProductID,
                    p.ProductID,
                    p.ProductName,
                    p.ProductDescription,
                    p.Photo
                FROM 
                    MemberFavorite AS m
                LEFT JOIN 
                    Product AS p ON m.ProductID = p.ProductID
                WHERE 
                    m.MemberID = @MemberId AND m.ProductID IS NOT NULL;
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

            //找會員年度花費總金額
            var totalOrderAmountResult = await _context.QueryFirstOrDefaultAsync<dynamic>(sql3, parameters);


           


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

            if ((memberFavorites1 == null || !memberFavorites1.Any()) && (memberFavorites2 == null || !memberFavorites2.Any()) && totalOrderAmountResult == null && (memberFavorites4 == null || !memberFavorites4.Any()))
            {
                return NotFound("No data found");
            }

            // 打包資料
            return Ok(new
            {
                FavoritesWithMembers = memberFavorites1,
                FavoritesWithTrainersAndProducts1 = memberFavorites2,
                FavoritesWithTrainersAndProducts2 = memberFavorites4,
                TotalOrderAmount = totalOrderAmountResult?.TotalOrderAmount ?? 0 // 使用 null propagation 檢查是否為 null，如果為 null 則返回 0
            });

        }



        //刪除教練
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


        //刪除商品
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
    }
}
