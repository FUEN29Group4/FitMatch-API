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
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Mail;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IDbConnection _db;
        private readonly string _lineChannelId;
        private readonly string _lineChannelSecret;
        private readonly string _lineCallbackUrl;

        public LoginController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _lineChannelId = configuration["LineLogin:ChannelId"];
            _lineChannelSecret = configuration["LineLogin:ChannelSecret"];
            _lineCallbackUrl = configuration["LineLogin:CallbackUrl"];
        }

        [HttpGet("loginWithLine")]
        public IActionResult LoginWithLine()
        {
            var authUrl = $"https://access.line.me/oauth2/v2.1/authorize?response_type=code&client_id={_lineChannelId}&redirect_uri={_lineCallbackUrl}&state={Guid.NewGuid().ToString()}&scope=profile%20openid";
            return Redirect(authUrl);
        }

        [HttpGet("line-callback")]
        public async Task<IActionResult> LineCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Error: No code received from LINE.");
            }

            var tokenRequestData = new Dictionary<string, string>
    {
        {"grant_type", "authorization_code"},
        {"code", code},
        {"redirect_uri", _lineCallbackUrl},
        {"client_id", _lineChannelId},
        {"client_secret", _lineChannelSecret}
    };

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.line.me/oauth2/v2.1/token")
            {
                Content = new FormUrlEncodedContent(tokenRequestData)
            };

            var httpClient = new HttpClient();
            var tokenResponse = await httpClient.SendAsync(tokenRequest);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<dynamic>(tokenResponseContent);

            if (tokenData == null)
            {
                return BadRequest("Error: No token received from LINE.");
            }

            string accessToken = tokenData.access_token;

            var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.line.me/v2/profile")
            {
                Headers = { { "Authorization", $"Bearer {accessToken}" } }
            };

            var profileResponse = await httpClient.SendAsync(profileRequest);
            var profileResponseContent = await profileResponse.Content.ReadAsStringAsync();
            var profileData = JsonConvert.DeserializeObject<dynamic>(profileResponseContent);

            string lineUserId = profileData.userId;
            string displayName = profileData.displayName;
            string profilePictureUrl = profileData.pictureUrl;

            // 檢查LineUsers資料表中是否已存在此userId
            var sqlCheckUserExists = @"SELECT COUNT(*) FROM LineUsers WHERE LineUserId = @LineUserId";
            int userCount = await _db.ExecuteScalarAsync<int>(sqlCheckUserExists, new { LineUserId = lineUserId });

            if (userCount == 0)
            {
                // 用戶不存在，新增一條記錄
                var sqlInsertUser = @"INSERT INTO LineUsers (LineUserId, DisplayName, ProfilePictureUrl) VALUES (@LineUserId, @DisplayName, @ProfilePictureUrl)";
                await _db.ExecuteAsync(sqlInsertUser, new { LineUserId = lineUserId, DisplayName = displayName, ProfilePictureUrl = profilePictureUrl });
            }
            else
            {
                // 用戶已存在，可以根據需求更新登入日期或其他資訊
                var sqlUpdateLoginDate = @"UPDATE LineUsers SET LoginDate = GETDATE() WHERE LineUserId = @LineUserId";
                await _db.ExecuteAsync(sqlUpdateLoginDate, new { LineUserId = lineUserId });
            }

            return Redirect("https://localhost:7088/");  
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



        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var recaptchaResponse = this.Request.Headers["Recaptcha-Response"];
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                return BadRequest("reCAPTCHA 驗證失敗.");
            }

            // 驗證 reCAPTCHA
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret=6LdtAkYoAAAAADfRYGdV43K0yvESHMoV2e8A485_&response={recaptchaResponse}");
                var reCaptchaResult = JsonConvert.DeserializeObject<dynamic>(response);

                // 檢查驗證是否成功
                if (!(bool)reCaptchaResult.success)
                {
                    return BadRequest("reCAPTCHA 驗證失敗.");
                }

                // 檢查分數是否低於閾值
                double score = (double)reCaptchaResult.score;
                if (score < 0.5) // 你可以根據你的需要調整這個閾值
                {
                    return BadRequest("reCAPTCHA 驗證失敗: 懷疑你是機器人。");
                }
            }

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
                    // 發送 LINE Notify 訊息
                    string lineNotifyToken = "26Owfwc6FwrvY4ka0fqip7l4KC6zXT5KH5scLJ6JVlK";
                    await SendLineNotifyMessage("登入成功！", lineNotifyToken);

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

        [HttpPost("requestResetPassword")]
        public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordModel model)
        {

            // 查詢 Member 表
            var memberSql = @"SELECT * FROM Member WHERE Email = @Email";
            var memberParameters = new { Email = model.Email };

            var member = await _db.QuerySingleOrDefaultAsync<Member>(memberSql, memberParameters);

            // 查詢 Trainer 表
            var trainerSql = @"SELECT * FROM Trainers WHERE Email = @Email";
            var trainerParameters = new { Email = model.Email };

            var trainer = await _db.QuerySingleOrDefaultAsync<Trainer>(trainerSql, trainerParameters);

            if (member == null && trainer == null)
            {
                return BadRequest("該 Email 地址未註冊");
            }

            // 生成 JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("FitMatch123456789123456789123456789"); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, model.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var resetToken = tokenHandler.WriteToken(token);

            // 發送 Email（這個您需要自己實現）
            await SendResetPasswordEmail(model.Email, resetToken);

            return Ok();
        }


        private async Task SendResetPasswordEmail(string email, string token)
        {
            var resetLink = $"https://localhost:7088/ResetPassword/ResetPassword?token={token}";

            // 初始化 SmtpClient
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential("s90382s@gmail.com", "rher aggj hmej rgma");

                // 初始化 MailMessage
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress("YourGmailEmail@gmail.com", "FitMatch@gmail.com");
                    message.To.Add(email);
                    message.Subject = "密碼重置";
                    message.Body = $"請點選連結重置您的密碼: {resetLink}";

                    // 發送郵件
                    await smtp.SendMailAsync(message);
                }
            }
        }


        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmModel model)
        {
            var token = model.Token;
            var newPassword = model.NewPassword;

            // 驗證和解析 JWT 令牌
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("FitMatch123456789123456789123456789");
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            SecurityToken validatedToken;
            try
            {
                var claims = handler.ValidateToken(token, validations, out validatedToken);
                var email = claims.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest("Invalid token");
                }

                string updateSql = null;
                string userType = null;

                // Check the database for Member first
                var memberSql = @"SELECT * FROM Member WHERE Email = @Email";
                var memberParameters = new { Email = email };
                var member = await _db.QuerySingleOrDefaultAsync<Member>(memberSql, memberParameters);

                if (member != null)
                {
                    updateSql = @"UPDATE Member SET Password = @Password, Salt = @Salt WHERE Email = @Email";
                    userType = "Member";
                }
                else
                {
                    // If Member not found, check for Trainer
                    var trainerSql = @"SELECT * FROM Trainers WHERE Email = @Email";
                    var trainerParameters = new { Email = email };
                    var trainer = await _db.QuerySingleOrDefaultAsync<Trainer>(trainerSql, trainerParameters);

                    if (trainer != null)
                    {
                        updateSql = @"UPDATE Trainers SET Password = @Password, Salt = @Salt WHERE Email = @Email";
                        userType = "Trainer";
                    }
                }

                if (userType == null)
                {
                    return BadRequest("User not found");
                }

                // 生成新的哈希密碼和 Salt
                using (var rng = new RNGCryptoServiceProvider())
                {
                    byte[] salt = new byte[16];
                    rng.GetBytes(salt);
                    var newHashedPassword = HashPassword(newPassword, Convert.ToBase64String(salt));

                    // Update the database
                    var updateParameters = new { Password = newHashedPassword, Salt = Convert.ToBase64String(salt), Email = email };
                    var affectedRows = await _db.ExecuteAsync(updateSql, updateParameters);

                    if (affectedRows > 0)
                    {
                        return Ok(new { success = true, message = $"{userType} 密碼重置成功" });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = $"{userType} 密碼重置失敗" });
                    }
                }
            }
            catch
            {
                return BadRequest("Invalid token");
            }
        }



        private async Task SendLineNotifyMessage(string message, string lineNotifyToken)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("message", message)
        });

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {lineNotifyToken}");

                var response = await httpClient.PostAsync("https://notify-api.line.me/api/notify", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    // 處理回應，例如，寫入日誌
                }
                else
                {
                    // 處理錯誤，例如，寫入日誌
                }
            }
        }


    }
}
