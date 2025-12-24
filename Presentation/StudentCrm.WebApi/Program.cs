using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Student.Domain.Entities;
using Student.Domain.HelperEntities;
using StudentCrm.Application.Profiles;
using StudentCrm.Persistence;
using StudentCrm.Persistence.Context;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StudentCrmDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// ? Identity BEFORE Build
builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredUniqueChars = 0;
    opt.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<StudentCrmDbContext>()
.AddDefaultTokenProviders();

// ? CORS BEFORE BuildAppUser
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsapp", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"])),
        LifetimeValidator = (_, expireDate, token, _) => token != null ? expireDate > DateTime.Now : false
    };
});
builder.Services.AddAutoMapper(typeof(AdminProfile));

builder.Services.AddServices(builder.Configuration);
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
              {
                  {
                      new OpenApiSecurityScheme
                      {
                          Reference = new OpenApiReference
                          {
                              Type = ReferenceType.SecurityScheme,
                              Id = "Bearer"
                          }
                      },
                      new string[]{}
                  }
              });
});
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSettings"));
var app = builder.Build();

// ? Seed AFTER Build
using (var scope = app.Services.CreateScope())
{
    await StudentCrmDbContextSeed.SeedAsync(scope.ServiceProvider);
}

app.UseMiddleware<StudentCrm.Application.GlobalAppException.GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("corsapp");

app.UseHttpsRedirection();

app.UseAuthentication();   // Identity i?l?sin dey?
app.UseAuthorization();

app.MapControllers();
app.Run();
