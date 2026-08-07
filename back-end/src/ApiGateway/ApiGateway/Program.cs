using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using NSwag.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Authentication (JWT)
//var jwtSettings = builder.Configuration.GetSection("JwtSettings");
//var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = false; // Nên để true khi lên Production
//    options.SaveToken = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
//        ValidateIssuer = true,
//        ValidIssuer = jwtSettings["Issuer"],
//        ValidateAudience = true,
//        ValidAudience = jwtSettings["Audience"],
//        ValidateLifetime = true,
//        ClockSkew = TimeSpan.Zero
//    };
//});

//builder.Services.AddAuthorization();

// 2. Cấu hình YARP Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 3. Cấu hình CORS (Quan trọng vì Frontend sẽ gọi vào Gateway)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Trong môi trường Dev. Production cần chặn chính xác hơn
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Đọc danh sách các Swagger URL từ appsettings
var swaggerUrls = builder.Configuration.GetSection("SwaggerServices").Get<Dictionary<string, string>>();

builder.Services.AddSwaggerDocument(config =>
{
    config.DocumentName = "GatewayAPI";
    config.Title = "API Gateway";
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll"); // CORS phải gọi trước Authentication
app.UseOpenApi(); // serve swagger.json do NSwag generate (nếu bạn muốn gộp cả gateway spec)
app.UseSwaggerUi(options =>
{
    options.DocumentTitle = "API Gateway";
    options.SwaggerRoutes.Clear();
    if (swaggerUrls != null)
    {
        foreach (var service in swaggerUrls)
        {
            options.SwaggerRoutes.Add(new SwaggerUiRoute($"{service.Key} Service", service.Value));
        }
    }

    // Fix lỗi NSwag tự nối PathBase vào URL tuyệt đối
    options.TransformToExternalPath = (internalUiRoute, request) =>
    {
        // URL tương đối (ví dụ khi bạn chuyển sang proxy qua YARP) -> xử lý bình thường
        return request.PathBase + internalUiRoute;
    };
});

//app.UseAuthentication();
//app.UseAuthorization();

// Map Proxy
app.MapReverseProxy();

app.Run();