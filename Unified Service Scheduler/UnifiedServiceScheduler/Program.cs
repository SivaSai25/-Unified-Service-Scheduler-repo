
using UnifiedServiceScheduler.Service;
using UnifiedServiceScheduler.DataAccess;
using Microsoft.Data.SqlClient;
using NPoco;
using Serilog;
namespace UnifiedServiceScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Host.UseSerilog((context, services, configuration) =>
                                            configuration
                                                .ReadFrom.Configuration(context.Configuration)
                                                .ReadFrom.Services(services)
                                                .Enrich.FromLogContext());
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IDatabase>(sp =>
            {
                var cs = builder.Configuration.GetConnectionString("Default");

                var connection = new SqlConnection(cs);

                return new Database(connection);
            });
            builder.Services.AddScoped<IDatabase>(_ =>
           new Database(
        builder.Configuration.GetConnectionString("Default"),
        DatabaseType.SqlServer2012,
        Microsoft.Data.SqlClient.SqlClientFactory.Instance));
            builder.Services.AddScoped<IUnifiedService,UnifiedService>();
            builder.Services.AddScoped<IUnifiesServiceDataAccess,UnifiedServiceDataAccess>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<TraceIdMiddleware>();

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}