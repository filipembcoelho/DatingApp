using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // Dependency Injection Container
        // Services ethod order is not important
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(
                x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
            );
            services.AddControllers();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Here order is extremely important
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // MiddleWare - Software to interact with our request through the pipeline


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseCors(x => x
                                .AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader()
            );

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}