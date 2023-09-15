using Dapper;
using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IDbConnection _db;

        public LoginController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }


        private string GenerateJwtToken(int userId, string userType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("FitMatch123456789123456789123456789"); // 使用长、复杂和唯一的密钥
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("Id", userId.ToString()),
            new Claim("Type", userType)
        }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            const string sql = @"SELECT * FROM Member";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var Member = multi.Read<Member>().ToList();
                // 基本驗證，確保資料存在
                if (Member == null)
                {
                    return NotFound("No data found");
                }
                return Ok(Member);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(int id)
        {
            const string sql = @"SELECT MemberId,Email,Password FROM Member WHERE MemberId = @MemberId";
            var parameters = new { MemberId = id };

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var member = multi.Read<Member>().FirstOrDefault();
                // 基本驗證，確保資料存在
                if (member == null)
                {
                    return NotFound("No data found");
                }
                return Ok(member);
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {

            // 查詢 Member 表
            var memberSql = @"SELECT * FROM Member WHERE Email = @Email";
            var memberParameters = new { Email = loginModel.Email };

            var member = await _db.QuerySingleOrDefaultAsync<Member>(memberSql, memberParameters);

            if (member != null)
            {
                // 使用存儲的 Salt 和提交的 Password 生成新的哈希
                var hashedPassword = HashPassword(loginModel.Password, member.Salt);  // 假設你有這樣的函數

                // 比較新生成的哈希和存儲的哈希
                if (hashedPassword == member.Password)
                {
                    // 進行後續處理，例如設置 session 或發送 token
                    var token = GenerateJwtToken(member.MemberId, "Member");
                    return Ok(new { Token = token, Type = "Member", Data = member });
                }
            }

            // 查詢 Trainer 表
            var trainerSql = @"SELECT * FROM Trainers WHERE Email = @Email";
            var trainerParameters = new { Email = loginModel.Email };

            var trainer = await _db.QuerySingleOrDefaultAsync<Trainer>(trainerSql, trainerParameters);

            if (trainer != null)
            {
                // 使用存儲的 Salt 和提交的 Password 生成新的哈希
                var hashedPassword = HashPassword(loginModel.Password, trainer.Salt);  // 假設你有這樣的函數

                // 比較新生成的哈希和存儲的哈希
                if (hashedPassword == trainer.Password)
                {
                    // 進行後續處理，例如設置 session 或發送 token
                    var token = GenerateJwtToken(trainer.TrainerId, "Trainer");
                    return Ok(new { Token = token, Type = "Trainer", Data = trainer });
                }
            }

            return NotFound("Email 或 Password 錯誤");


        }

        private string HashPassword(string password, string saltString)
        {
            byte[] salt = Convert.FromBase64String(saltString);

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }

        [HttpGet("getUserInfo")]
        public async Task<IActionResult> GetUserInfo([FromHeader(Name = "Authorization")] string bearerToken)
        {
            if (string.IsNullOrEmpty(bearerToken) || !bearerToken.StartsWith("Bearer "))
            {
                return BadRequest("Invalid token");
            }

            var token = bearerToken.Substring("Bearer ".Length).Trim();

            // 驗證和解析 JWT 令牌（這裡只是一個簡單的示例，實際實施可能更複雜）
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("FitMatch123456789123456789123456789");
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var claims = handler.ValidateToken(token, validations, out var tokenSecure);
            var userId = int.Parse(claims.FindFirst("Id").Value);
            var userType = claims.FindFirst("Type").Value;

            // 根據 userType 和 userId 從數據庫中獲取用戶詳細信息
            if (userType == "Member")
            {
                var sql = @"SELECT * FROM Member WHERE MemberId = @MemberId";
                var parameters = new { MemberId = userId };
                var member = await _db.QuerySingleOrDefaultAsync<Member>(sql, parameters);
                return Ok(member);
            }
            else if (userType == "Trainer")
            {
                var sql = @"SELECT * FROM Trainers WHERE TrainerId = @TrainerId";
                var parameters = new { TrainerId = userId };
                var trainer = await _db.QuerySingleOrDefaultAsync<Trainer>(sql, parameters);
                return Ok(trainer);
            }

            return BadRequest("Invalid user type");
        }


    }
}
