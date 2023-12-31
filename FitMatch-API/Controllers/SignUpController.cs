﻿using FitMatch_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Linq;


namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly IDbConnection _db;

        public SignUpController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _db.Open();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(SignUpModel signUpModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model");
            }

            // 檢查兩個資料表裡面有沒有重複的E-mail
            var duplicateEmailCheckSql = @"SELECT COUNT(*) FROM Trainers WHERE Email = @Email UNION ALL SELECT COUNT(*) FROM Member WHERE Email = @Email";
            var duplicateCount = await _db.QueryAsync<int>(duplicateEmailCheckSql, new { Email = signUpModel.Email });
            if (duplicateCount.Any(count => count > 0))
            {
                return BadRequest("Email 已經被註冊過了");
            }

            // Generate a salt
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string saltString = Convert.ToBase64String(salt);

            // Generate the hash
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: signUpModel.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Update signUpModel.Password with hashedPassword before saving
            signUpModel.Password = hashedPassword;
            signUpModel.Salt = saltString;


            try
            {
                if (signUpModel.AccountType == "教練")
                {
                    DateTime birthDate = DateTime.Parse(signUpModel.Birth);  //轉換日期
                    signUpModel.Birth = birthDate.ToShortDateString();

                    const string trainerSql = @"INSERT INTO Trainers(TrainerName, Gender, Birth, Phone, Address, Email, Password, Salt, Approved, CreatedAt) 
                                    VALUES(@Name, @Gender, @Birth, @Phone, @Address, @Email, @Password, @Salt, 0, GETUTCDATE())";

                    await _db.ExecuteAsync(trainerSql, signUpModel);
                    return Ok(new { message = "註冊成功，身份：教練" });
                }
                else if (signUpModel.AccountType == "會員")
                {
                    DateTime birthDate = DateTime.Parse(signUpModel.Birth);  //轉換日期
                    signUpModel.Birth = birthDate.ToShortDateString();

                    const string memberSql = @"INSERT INTO Member(MemberName, Gender, Birth, Phone, Address, Email, Password, Salt, CreatedAt) VALUES(@Name, @Gender, @Birth, @Phone, @Address, @Email, @Password, @Salt, GETUTCDATE())";

                    await _db.ExecuteAsync(memberSql, signUpModel);
                    return Ok(new { message = "註冊成功，身份：會員" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return BadRequest("註冊失敗");
            }
            return BadRequest("註冊失敗");
        }

    }
}
