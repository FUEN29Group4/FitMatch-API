using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassTypeController : ControllerBase
    {
        private readonly IDbConnection _db;
        public ClassTypeController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            const string sql = @"SELECT * FROM ClassTypes";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var ClassTypes = multi.Read<ClassType>().ToList();
                if (ClassTypes == null)
                {
                    return NotFound("No data found");
                }
                return Ok(ClassTypes);
            }
        }
    }
}
