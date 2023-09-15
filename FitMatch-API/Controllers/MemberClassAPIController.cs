using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberClassAPIController : ControllerBase
    {
        //   Member/MemberClass
        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _context;                                //宣吿類別級變數，串資料庫
        private readonly ILogger<MemberClassAPIController> _logger;             //宣吿類別級變數，串登入資訊

        //定義_context
        public MemberClassAPIController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // R: 讀取所有MemberClassAPI列表資料 => ok
        //   Member/MemberClass
        [HttpGet]
        public async Task<IActionResult> GetALLMemberClassAPI()
        {
            const string sql = @"SELECT * FROM [CLASS]";

            using (var multi = await _context.QueryMultipleAsync(sql))
            {
                var MemberClassAPIs = multi.Read<MemberClassAPI>().ToList();
                // 基本驗證，確保資料存在
                if (MemberClassAPIs == null)
                {
                    return NotFound("No data found");
                }
                return Ok(MemberClassAPIs);
            }
        }

        //   Member/MemberClass
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberClassAPI(int id)
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
        c.ClassID,
        c.MemberID,
        c.TrainerID,
		c.GymID,
		c.CourseStatus,
		c.BuildTime,

        t.TrainerID,
        t.TrainerName,

	   g.GymId,
	   g.GymName,
	   g.[Address]

    FROM Class as c
    LEFT JOIN Trainers as t ON c.TrainerID = t.TrainerID
	LEFT JOIN Gyms as g ON   c.GymID = g.GymID
    WHERE c.MemberID = @MemberId;
    ";

            var parameters = new { MemberId = id };

            var MemberClassAPIs1 = await _context.QueryAsync<MemberClassAPI, Member, Order, MemberClassAPI>(
                    sql1,
                    (memberClassapi, member, order) =>
                    {
                        if (member != null && member.MemberId != null)
                        {
                            memberClassapi.Members.Add(member);
                        }

                        if (order != null && order.MemberId != null)
                        {
                            memberClassapi.Orders.Add(order);
                        }

                        return memberClassapi;
                    },
                    param: parameters,
                    splitOn: "MemberId,MemberId"
                );

            var MemberClassAPIs2 = await _context.QueryAsync<Class, Trainer, Gym, Class>(
                sql2,
                (memberClassapi, trainer,gym) =>
                {
                    if (trainer != null && trainer.TrainerId != null)
                    {
                        memberClassapi.Trainers.Add(trainer);
                    }

                    if (gym != null && gym.GymId != null)
                    {
                        memberClassapi.Gyms.Add(gym);
                    }


                    return memberClassapi;
                },
                param: parameters,
                splitOn: "TrainerId,GymId"
            );

            if ((MemberClassAPIs1 == null || !MemberClassAPIs1.Any()) && (MemberClassAPIs2 == null || !MemberClassAPIs2.Any()))
            {
                return NotFound("No data found");
            }

            // 此處可以根據您的需求來決定如何組合或返回查詢結果。
            return Ok(new { ClassAPIWithMembers = MemberClassAPIs1, ClassAPIWithTrainersAndClassTyoeAndGyms = MemberClassAPIs2 });
        }
    }
}
