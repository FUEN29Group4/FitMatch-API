var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//這段讓我的前端地址連的到
builder.Services.AddCors(options =>
{
    //builder => builder.WithOrigins("http://127.0.0.1:5500/")
    options.AddPolicy("AllowMyOrigin",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
var app = builder.Build();
// 配置中間件
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowMyOrigin"); // 確保這一行在 UseRouting 和 UseEndpoints 之前
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
//app.MapControllers();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});



app.Run();
