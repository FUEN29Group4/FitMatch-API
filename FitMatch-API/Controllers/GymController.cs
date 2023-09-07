using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GymController : ControllerBase
    {
        private readonly IDbConnection _db;

        public GymController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            const string sql = @"SELECT * FROM Gyms";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var gyms = multi.Read<Gym>().ToList();
                // 基本驗證，確保資料存在
                if (gyms == null)
                {
                    return NotFound("No data found");
                }
                return Ok(gyms);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGyms(string gymName = null, string city = null, string district = null)
        {
            StringBuilder sql = new StringBuilder("SELECT * FROM Gyms WHERE 1=1 ");
            var parameters = new DynamicParameters();

            // 篩選場館名稱
            if (!string.IsNullOrEmpty(gymName))
            {
                sql.Append("AND GymName LIKE @GymName ");
                parameters.Add("GymName", "%" + gymName + "%");
            }

            // 篩選縣市
            if (!string.IsNullOrEmpty(city))
            {
                sql.Append("AND Address LIKE @City ");
                parameters.Add("City", "%" + city + "市%");
            }

            // 篩選區域
            if (!string.IsNullOrEmpty(district))
            {
                sql.Append("AND Address LIKE @District ");
                parameters.Add("District", "%" + district + "區%");
            }

            using (var multi = await _db.QueryMultipleAsync(sql.ToString(), parameters))
            {
                var gyms = multi.Read<Gym>().ToList();

                if (gyms == null || !gyms.Any())
                {
                    return NotFound("No gyms found matching the criteria");
                }
                return Ok(gyms);
            }
        }


        // 其他方法（例如：新增、修改、刪除等）在這裡繼續添加
    }
}
