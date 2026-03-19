using Microsoft.EntityFrameworkCore;
using Cover.Api.Data;
using Cover.Api.Middleware;
using Cover.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IBalanceService, BalanceService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Cover API"));
app.UseCors();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    bool isPublic = path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase)
                 || path.StartsWith("/api/setup", StringComparison.OrdinalIgnoreCase)
                 || !path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
    if (!isPublic)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        var db = context.RequestServices.GetRequiredService<AppDbContext>();
        var creds = await db.AppCredentials.FirstOrDefaultAsync();
        if (token is null || creds?.SessionToken != token)
        {
            context.Response.StatusCode = 401;
            return;
        }
    }
    await next();
});

app.MapControllers();
app.Run();
