using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PFCWebApplication.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using PFCWebApplication.Repositories;

namespace PFCWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string absolutePath = builder.Environment.ContentRootPath + "//msd63a2025-d26c9af97596.json";

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", absolutePath);

            // Add services to the container.
            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            // .AddEntityFrameworkStores<ApplicationDbContext>();


            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddGoogle(options =>
                { //using builder.Configuration["Authentication:Google:ClientId"] means that credentials will be loaded
                    //from secrets.json OR alternativaly we can store the CLIENTID and SECRET on the cloud
                    //reason is: so important secret key and clientid are not mistakenly uploaded on some public repository

                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                });

            builder.Services.AddRazorPages();

            builder.Services.AddControllersWithViews();


            //we are going to register any services that need to be instantiated here
            //why? this is the only place in the application that will run only once
            //we need to inform the application about the existence of such service (classes) that need to be
            //instantiated at some point in time e.g. FirestoreRepository

            string project = builder.Configuration.GetValue<string>("project");
            string bucket = builder.Configuration.GetValue<string>("bucket");

            string redisConnection = builder.Configuration["redisConnection"]; 
            string redisUser = builder.Configuration["redisUser"];
            string redisPassword = builder.Configuration["redisPassword"];


            builder.Services.AddScoped(x=> new FirestoreRepository(project));
            builder.Services.AddScoped(x => new BucketRepository(bucket));
            builder.Services.AddScoped(x => new RedisRepository(redisConnection, redisUser, redisPassword));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
