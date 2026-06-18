
using Leaderboard.Repository;
using Leaderboard.Services;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging.Console;

namespace Leaderboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            builder.Logging.AddSimpleConsole(option => { option.SingleLine = true; });
            builder.Services.AddHttpLogging(options => { options.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseBody; });
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();


            app.UseHttpLogging();
            app.Run();
        }
    }
}
