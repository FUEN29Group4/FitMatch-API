﻿using Dapper;
using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;


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
                    return Ok(new { Type = "Member", Data = member });
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
                    return Ok(new { Type = "Trainer", Data = trainer });
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

    }
}
