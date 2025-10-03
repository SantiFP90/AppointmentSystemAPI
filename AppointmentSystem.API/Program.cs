using AppointmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Application.Interfaces.Services;
using AppointmentSystem.Application.Mapper;
using AppointmentSystem.Infrastructure.Services;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Infrastructure.Repositories;
using AppointmentSystem.Infrastructure.Security;
using AppointmentSystem.Application.Interfaces.Fatories;
using AppointmentSystem.Infrastructure.Factories;
using AppointmentSystem.Infrastructure.Strategies;
using AppointmentSystem.Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔌 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Swagger clásico
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Mapper
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

// ✏️ Controllers
builder.Services.AddControllers();

//Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWorkingDayService, WorkingDayService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<INotificationLogService, NotificationLogService>();

//SmtpServices
builder.Services.AddScoped<ISmtpService, SmtpServices>();

//Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

//UoW Repository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Strategies
builder.Services.AddScoped<ExistingClientStrategy>();
builder.Services.AddScoped<NewUserStrategy>();

//Factories
builder.Services.AddScoped<IClientCreationStrategyFactory, ClientCreationStrategyFactory>();

//Security
builder.Services.AddScoped<JwtSecurityService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        //Servidor
        ValidateIssuer = false,
        //Cliente
        ValidateAudience = false,
        //Tiempo de expiración
        ValidateLifetime = true,
        //Firma de cliente
        ValidateIssuerSigningKey = true,
        //Firma servidor
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)
        ),
        ClockSkew = TimeSpan.Zero
    };
});

//Workers
builder.Services.AddHostedService<AppointmentNotificationWorker>();
builder.Services.AddHostedService<AppoimentDeleteTimeSlotWorker>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

// 🧪 Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Front");           // 1️⃣ CORS primero

app.UseHttpsRedirection();      // 2️⃣ HTTPS

app.UseAuthentication();        // 3️⃣ Autenticación (valida el token)
app.UseAuthorization();         // 4️⃣ Autorización (valida permisos)

app.MapControllers();

app.Run();
