using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {

        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _context;//宣吿類別級變數，串資料庫
        private readonly ILogger<ReviewsController> _logger; //宣吿類別級變數，串登入資訊

        //定義_context
        public ReviewsController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }


        // GET: api/<ReviewsController>
  //      [HttpGet]
  //      public async Task<IActionResult> GetALLMemberFavorite()
  //      {
  //          const string sql = @"SELECT * FROM [Reviews]";

  //          using (var multi = await _context.QueryMultipleAsync(sql))
  //          {
  //              var Reviewss = multi.Read<Reviews>().ToList();
  //              // 基本驗證，確保資料存在
  //              if (Reviewss == null)
  //              {
  //                  return NotFound("No data found");
  //              }
  //              return Ok(Reviewss);
  //          }
  //      }

  //      SELECT
  //  r.ReviewID,
  //  r.MemberID,
  //  r.ProductID,
  //  r.ClassID,
  //  r.Rating,
  //  r.Comment,
  //  r.ReviewDateTime,
  //  r.TrainerID,
  //  r.ClassTypeID,

  //  m.MemberID,
  //  m.MemberName,

  //  p.ProductID,
  //  p.ProductName,

  //  c.ClassID,
  //  c.CourseStatus,

  //  t.TrainerID,
  //  t.TrainerName,

  //  ct.ClassTypeID,
  //  ct.ClassName

  //FROM Reviews as r

  //LEFT JOIN[Member] as m ON r.MemberID = m.MemberID

  //LEFT JOIN Product as p ON r.ProductID = p.ProductID

  //LEFT JOIN Class as c ON r.ClassID = c.ClassID

  //LEFT JOIN Trainers as t ON r.TrainerID = t.TrainerID

  //LEFT JOIN ClassTypes as ct ON r.ClassTypeID = ct.ClassTypeID


  //WHERE r.MemberID = 3;



        // GET api/<ReviewsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ReviewsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ReviewsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ReviewsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
