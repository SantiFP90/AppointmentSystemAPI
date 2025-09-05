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

app.UseCors("Front");
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

app.Run();
