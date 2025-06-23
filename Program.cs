using System.Text;
using backend.Data;
using backend.Interfaces;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Interfaces 
builder.Services.AddScoped<ITokenService, TokenService>();

// Repository pattern
builder.Services.AddScoped<backend.Repositories.Interfaces.IUnitOfWork, backend.Repositories.Implementations.UnitOfWork>();
builder.Services.AddScoped<backend.Repositories.Interfaces.ISectionRepository, backend.Repositories.Implementations.SectionRepository>();
builder.Services.AddScoped<backend.Repositories.Interfaces.IStudentRepository, backend.Repositories.Implementations.StudentRepository>();
builder.Services.AddScoped<backend.Repositories.Interfaces.IInstructorRepository, backend.Repositories.Implementations.InstructorRepository>();
builder.Services.AddScoped<backend.Repositories.Interfaces.IAssessmentVisibilityRepository, backend.Repositories.Implementations.AssessmentVisibilityRepository>();
builder.Services.AddScoped<backend.Repositories.Interfaces.IResultRepository, backend.Repositories.Implementations.ResultRepository>();
builder.Services.AddScoped<backend.Repositories.Interfaces.IAssessmentRepository, backend.Repositories.Implementations.AssessmentRepository>();

// Configure URLs for development
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://localhost:5240");
}
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// connection to database
builder.Services.AddDbContext<PolarisDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

builder.Services.AddControllers();

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PolarisDbContext>()
    .AddDefaultTokenProviders();

// Explicitly register RoleManager
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        // Lockout settings

        // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        // options.Lockout.MaxFailedAccessAttempts = 5;
    });

// AllowAnyOrigin
builder.Services.AddCors(policy =>
        { policy.AddPolicy("AllowOrigin", option => option.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()); });

// Configure JWT authentication
// Configure authentication to use JWT bearer tokens instead of cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    // Set up the token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))

    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization();


// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowOrigin");
app.UseAuthorization(); // if using [Authorize] somewhere
app.MapControllers();   // required for route mapping

app.Run();
