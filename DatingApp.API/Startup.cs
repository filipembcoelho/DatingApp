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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DatingApp.API.Helpers;
using AutoMapper;

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

    public void ConfigureDevelopmentServices(IServiceCollection services)
    {
      services.AddDbContext<DataContext>(x =>
      {
        x.UseLazyLoadingProxies();
        x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
      });

      ConfigureServices(services);
    }

    public void ConfigureProductionServices(IServiceCollection services)
    {
      services.AddDbContext<DataContext>(x =>
      {
        x.UseLazyLoadingProxies();
        x.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
      });

      ConfigureServices(services);
    }

    public void ConfigureServices(IServiceCollection services)
    {

      services.AddControllers().AddNewtonsoftJson(opt =>
      {
        opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
      });

      services.AddCors();

      services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

      services.AddAutoMapper(typeof(DatingRepository).Assembly);

      services.AddScoped<IAuthRepository, AuthRepository>();
      services.AddScoped<IDatingRepository, DatingRepository>();

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
              ValidateIssuerSigningKey = true,
              IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
              ValidateIssuer = false,
              ValidateAudience = false
            };
          });

      services.AddScoped<LogUserActivity>();
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
      else
      {
        app.UseExceptionHandler(builder =>
        {
          builder.Run(async context =>
                                          {
                                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                            var error = context.Features.Get<IExceptionHandlerFeature>();
                                            if (error != null)
                                            {
                                              context.Response.AddApplicationError(error.Error.Message);
                                              await context.Response.WriteAsync(error.Error.Message);
                                            }
                                          });
        });
      }

      // app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthentication();

      app.UseAuthorization();

      // Adding CORS
      app.UseCors(x => x
          .AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader()
      );

      // serve the index.html in the default wwwroot folder
      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapFallbackToController("Index", "Fallback");
      });
    }
  }
}