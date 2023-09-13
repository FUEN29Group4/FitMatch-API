var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//�o�q���ڪ��e�ݦa�}�s����
builder.Services.AddCors(options =>
{
    //builder => builder.WithOrigins("http://127.0.0.1:5500/")
    options.AddPolicy("AllowMyOrigin",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
var app = builder.Build();
// �t�m������
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowMyOrigin"); // �T�O�o�@��b UseRouting �M UseEndpoints ���e
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
//app.MapControllers();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});



app.Run();
