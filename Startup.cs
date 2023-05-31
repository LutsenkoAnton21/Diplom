using Diplom.Auth;
using Diplom.Entities;
using Diplom.Interfacec;
using Diplom.Repositories;
using Diplom.Services;
using Diplom.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WebSockets = System.Net.WebSockets;

namespace Diplom
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddWebSockets();

            services.AddSingleton<WebSocketConnectionManager>();
            services.AddScoped<IMessageService, MessageService>();

            services.AddTransient<UserService>();
            services.AddTransient<UserRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<PostService>();
            services.AddTransient<PostRepository>();
            services.AddTransient<IPostRepository, PostRepository>();
            // получаем строку подключения из файла конфигурации
            string connection = Configuration.GetConnectionString("DefaultConnection");
            // добавляем контекст ApplicationContext в качестве сервиса в приложение
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(connection));
            services.AddControllersWithViews();
            services.AddControllers();
            services.AddScoped<ITokenService, TokenService>();
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationContext>();
            services.AddScoped<UserManager<User>>();

            services.AddSwaggerGen((option) =>
            {
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                            new string[] {}
                    }
                });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebSocketConnectionManager connectionManager)
        {
            app.UseAuthentication();

            app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Diplom v1"));
            }

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    // Отримайте ідентифікатор користувача з запиту, наприклад, через токен або параметр URL
                    string userId = context.Request.Query["userId"];

                    // Додайте підключення до WebSocketConnectionManager
                    connectionManager.AddConnection(userId, webSocket);

                    // Обробляйте WebSocket-підключення та повідомлення тут

                    // Після закриття підключення видаліть його з WebSocketConnectionManager
                    connectionManager.RemoveConnection(userId);
                }
                else
                {
                    await next();
                }
            });

            app.UseWebSockets();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapWebSocket("/ws", (context, webSocket) =>
                //{
                //    // Обробка WebSocket-підключення
                //    // Використовуйте вашу власну реалізацію WebSocket-класу тут
                //    var socketHandler = new WebSocketHandler();
                //    socketHandler.HandleConnection(context, webSocket);
                //});
            });
        }
    }
}
