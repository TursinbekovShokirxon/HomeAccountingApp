using Application.Interfaces.Repositories.Model;
using Application.Interfaces.Services;
using Application.Security;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace HomeAccountingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddAuthentication("MyCookieAuth")
                .AddCookie("MyCookieAuth", options =>
                {
                    options.Cookie.Name = "Accounting.Cookie";
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.SlidingExpiration = true;
                });
            //�����������   
            builder.Services.AddMemoryCache();


            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<LoginProtectionService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            builder.Services.AddDbContext<HomeAccountingContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
