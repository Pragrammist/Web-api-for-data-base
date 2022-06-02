using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAccount.Models;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using WebApiAccount.Services;
using UserAccountsDataBaseWebApi;
using WebApiAccount.Infrastructure.ModelBinders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WebApiAccount
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            // укзывает, будет ли валидироваться издатель при валидации токена
                            ValidateIssuer = true,
                            // строка, представляющая издателя
                            ValidIssuer = AuthOptions.ISSUER,

                            // будет ли валидироваться потребитель токена
                            ValidateAudience = true,
                            // установка потребителя токена
                            ValidAudience = AuthOptions.AUDIENCE,
                            // будет ли валидироваться время существования
                            ValidateLifetime = true,

                            // установка ключа безопасности
                            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                            // валидация ключа безопасности
                            ValidateIssuerSigningKey = true,
                        };
                    });
            services.AddControllers(opts =>
            {
                opts.ModelBinderProviders.Insert(0, new UserModelBinderProvider());
                opts.ModelBinderProviders.Insert(1, new AvatarModelBinderProvider());
                opts.ModelBinderProviders.Insert(2, new BillModelBinderProvider());
                opts.ModelBinderProviders.Insert(3, new GuildModelBinderProvider());
                opts.ModelBinderProviders.Insert(4, new OrderModelBinderProvider());
                opts.ModelBinderProviders.Insert(5, new ProductModelBinderProvider());
                opts.ModelBinderProviders.Insert(6, new ReportModelBinderProvider());
            }
            );
            services.AddDbContext<MainDb>((b)=>b.UseSqlServer(Configuration.GetConnectionString("mainDb")));
            ConfigureDbServices(services);
        }

        protected void ConfigureDbServices(IServiceCollection services)
        {
            services.AddScoped<AvatarStore>();
            services.AddScoped<BillStore>();
            services.AddScoped<GuildStore>();
            services.AddScoped<OrderStore>();
            services.AddScoped<ProductStore>();
            services.AddScoped<ReportStore>();
            services.AddScoped<UserStore>();

            services.AddTransient<AvatarValidation>();
            services.AddTransient<BillValidation>();
            services.AddTransient<GuildValidation>();
            services.AddTransient<OrderValidation>();
            services.AddTransient<ProductValidation>();
            services.AddTransient<ReportValidation>();
            services.AddTransient<UserValidation>();

            services.AddScoped<AvatarsAgent>();
            services.AddScoped<BillsAgent>();
            services.AddScoped<GuildsAgent>();
            services.AddScoped<OrdersAgent>();
            services.AddScoped<ReportsAgent>();
            services.AddScoped<ProductsAgent>();
            services.AddScoped<UsersAgent>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();



            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
