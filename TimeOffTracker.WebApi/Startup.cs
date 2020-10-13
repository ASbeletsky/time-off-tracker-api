using BusinessLogic.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TimeOffTracker.WebApi.AuthHelpers;
using TimeOffTracker.WebApi.Filters;
using TimeOffTracker.WebApi.Infrastructure;
using TimeOffTracker.WebApi.Services;

namespace TimeOffTracker.WebApi
{
    public class Startup
    {

        public Startup(IConfiguration configuration) 
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            BusinessConfiguration.ConfigureServices(services, Configuration);
            BusinessConfiguration.ConfigureIdentityInicializerAsync(services.BuildServiceProvider());

            JwtConfiguration.Configure(services, Configuration);

            services.AddControllers(mvcOtions =>
            {
                mvcOtions.EnableEndpointRouting = false;
            });

            services.AddScoped(typeof(UserService));

            services.AddScoped<ExceptionFilter>();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimeOffTrackerApi", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."

                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //CORS setting
            app.UseCors(builder => builder.AllowAnyOrigin());
            //.WithOrigins(<front address>)
            //.AllowAnyMethod()
            //.AllowCredentials()

            app.UseMvc();
        }
    }
}
