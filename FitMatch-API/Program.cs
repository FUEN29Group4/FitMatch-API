using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Text;
using FitMatch_API.Hubs;
// ... 其他必要的 using 聲明

var builder = WebApplication.CreateBuilder(args);
// 添加 SignalR 服務
builder.Services.AddSignalR();


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add JWT Authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("FitMatch123456789123456789123456789"))
    };
});

// 這段讓我的前端地址連的到
builder.Services.AddCors(options =>
{
    //options.AddPolicy("AllowMyOrigin",
    //    builder => builder.AllowAnyOrigin()
    options.AddPolicy("CorsPolicy",
          builder => builder.WithOrigins("https://fitmatchclient4.azurewebsites.net")  // 替換為你的前端網站地址

          .AllowAnyMethod()
            .AllowAnyHeader()
    .AllowCredentials()); // 允許SignalR憑證
});

var app = builder.Build();

// 配置中間件
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors("AllowMyOrigin");
app.UseCors("CorsPolicy");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // 確保這一行在 UseRouting 之後，但在 UseEndpoints 之前
app.UseAuthorization();

// 加入 SignalR
app.MapHub<ChatHub>("/chatHub");



app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
