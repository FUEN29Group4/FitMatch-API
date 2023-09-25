using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Text;
using FitMatch_API.Hubs;
// ... ��L���n�� using �n��

var builder = WebApplication.CreateBuilder(args);
// �K�[ SignalR �A��
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

// �o�q���ڪ��e�ݦa�}�s����
builder.Services.AddCors(options =>
{
    //options.AddPolicy("AllowMyOrigin",
    //    builder => builder.AllowAnyOrigin()
    options.AddPolicy("CorsPolicy",

          builder => builder.WithOrigins("https://localhost:7088")  // �������A���e�ݺ����a�}


          .AllowAnyMethod()
            .AllowAnyHeader()
    .AllowCredentials()); // ���\SignalR����
});

var app = builder.Build();

// �t�m������
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

app.UseAuthentication(); // �T�O�o�@��b UseRouting ����A���b UseEndpoints ���e
app.UseAuthorization();

// �[�J SignalR
app.MapHub<ChatHub>("/chatHub");



app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
