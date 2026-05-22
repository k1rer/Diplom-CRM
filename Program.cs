using Diplom_CRM.Services;
using Diplom_CRM.Services.Implementations;

namespace Diplom_CRM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string? connectionString = builder.Configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new MissingFieldException("Failed to get Default connection string");

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IDealService, DealService>();
            builder.Services.AddScoped<IActivityService, ActivityService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
