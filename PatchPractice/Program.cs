
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PatchPractice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddControllers().AddNewtonsoftJson(); // Have to add in new project
            builder.Services.AddHttpContextAccessor();

            IConfiguration configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();
            builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseSqlite(configuration.GetConnectionString("DatabaseConnection"),
                   b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)
            ), ServiceLifetime.Scoped
            );

            var app = builder.Build();

            app.MapControllers();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Run();
        }
    }
}