using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Text;
using FitMatch_API.Hubs;

var builder = WebApplication.CreateBuilder(args);
// // 添加 SignalR 服務
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

          builder => builder.WithOrigins("https://localhost:7088")  // 替換為你的前端網站地址


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

app.UseAuthentication(); 
app.UseAuthorization();

// 加入 SignalR
app.MapHub<ChatHub>("/chatHub");



app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
