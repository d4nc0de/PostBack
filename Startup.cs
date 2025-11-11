using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace HR
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

            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("AngularDev", builder =>
                {
                    builder
                        .WithOrigins("http://localhost:4200") // tu front
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        // Si vas a usar cookies/autenticación por navegador, descomenta:
                        // .AllowCredentials()
                        ;
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HR", Version = "v1" });
            });

            // Configuración de MongoDB
            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = Configuration.GetConnectionString("MongoDB") 
                    ?? "mongodb://root:password123@localhost:27017/?authSource=admin";
                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var databaseName = Configuration["MongoDB:DatabaseName"] ?? "biblioteca";
                return client.GetDatabase(databaseName);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // ?? CORS debe ir entre UseRouting y UseAuthorization
            app.UseCors("AngularDev");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
