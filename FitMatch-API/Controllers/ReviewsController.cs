using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using FitMatch_API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using System.Data;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IDbConnection _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // GET: api/<ReviewsController>
        [HttpGet]
        public async Task<IActionResult> GetALLReviews()
        {
            const string sql = @"SELECT * FROM [Reviews]";

            using (var multi = await _context.QueryMultipleAsync(sql))
            {
                var reviews = multi.Read<Review>().ToList();

                if (!reviews.Any())
                {
                    return NotFound("No data found");
                }

                return Ok(reviews);
            }
        }

        // GET api/<ReviewsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviews(int id)
        {
            const string sql = @"
                SELECT
                    r.ReviewID, r.MemberID, r.ProductID, r.ClassID, r.Rating, r.Comment, r.ReviewDateTime, r.TrainerID, r.ClassTypeID,
                    m.MemberID, m.MemberName,
                    p.ProductID, p.ProductName,
                    c.ClassID, c.CourseStatus,
                    t.TrainerID, t.TrainerName,
                    ct.ClassTypeID, ct.ClassName
                FROM Reviews as r
                LEFT JOIN [Member] as m ON r.MemberID = m.MemberID
                LEFT JOIN Product as p ON r.ProductID = p.ProductID
                LEFT JOIN Class as c ON r.ClassID = c.ClassID
                LEFT JOIN Trainers as t ON r.TrainerID = t.TrainerID
                LEFT JOIN ClassTypes as ct ON r.ClassTypeID = ct.ClassTypeID
                WHERE r.MemberID = @MemberId";

            var parameters = new { MemberId = id };

            var reviewsResult = await _context.QueryAsync<Review, Member, Product, Class, Trainer, ClassType, Review>(
                sql,
                (review, member, product, classObj, trainer, classType) =>
                {
                    if (member != null)
                        review.Members.Add(member);
                    if (product != null)
                        review.Products.Add(product);
                    if (classObj != null)
                        review.Classs.Add(classObj);
                    if (trainer != null)
                        review.Trainers.Add(trainer);
                    if (classType != null)
                        review.ClassTypes.Add(classType);

                    return review;
                },
                param: parameters,
                splitOn: "MemberId,ProductId,ClassId,TrainerId,ClassTypeId"
            );

            if (!reviewsResult.Any())
            {
                return NotFound("No data found");
            }

            return Ok(new { reviewsWithMembers = reviewsResult });
        }

        //// POST api/<ReviewsController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<ReviewsController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ReviewsController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
