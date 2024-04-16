using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RVFaceRecognitionAPI.Models;
using System.Reflection;
using System.Text;
using RVFaceRecognitionAPI.Services;
using System.Net.WebSockets;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

        ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
        var streamService = new StreamService(serviceProvider);

        builder.Services.AddSingleton(streamService); // Регистрация SteamService в контейнере DI
        builder.Services.AddScoped<ILoggingService, LoggingService>();
        builder.Services.AddTransient<IAuthService, AuthService>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddCookie(options =>
        {
            options.Cookie.Name = "AccessToken";
        }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateActor = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,
                    ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value ?? ""))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["AccessToken"];
                        return Task.CompletedTask;
                    }
                };
            }
        );
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "RVFaceRecognitionAPI",
                Description = "Web API for \"RVFaceRecognition\" project",
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddCors(options =>
        {
            // Vue
            options.AddPolicy("vueApp", policyBuilder =>
            {
                policyBuilder.WithOrigins(builder.Configuration.GetSection("VueAddress").Value);
                policyBuilder.AllowAnyHeader();
                policyBuilder.AllowAnyMethod();
                policyBuilder.AllowCredentials();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("vueApp");

        app.UseAuthentication();
        app.UseAuthorization();

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        webSocketOptions.AllowedOrigins.Add(builder.Configuration.GetSection("VueAddress").Value);

        app.UseWebSockets(webSocketOptions);
        app.Use(async (context, next) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                Console.WriteLine("Start sending the frames...");
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                var cancellationToken = context.RequestAborted;
                await streamService.SendImageToWebSocket(webSocket, cancellationToken);
            }
            else
            {
                await next();
            }
        });

        app.MapControllers();

        app.Run();
    }
}