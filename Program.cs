using InsightCore.Configuration;
using InsightCore.Engine.ML;
using InsightCore.Engine.Search;
using Microsoft.ML;

namespace InsightCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // Register LogEngineService as scoped if it depends on per-request data
            builder.Services.AddScoped<LogEngineService>();


            builder.Services.AddSingleton<ApplicationSettings>();
            builder.Services.AddSingleton<PredictionEngineService>();

            builder.Services.AddSingleton<MLContext>();

            builder.Services.AddSession();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
