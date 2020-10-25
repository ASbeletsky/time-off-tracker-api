using AutoMapper;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.Settings;
using DataAccess.Context;
using DataAccess.Infrastructure;
using Domain.EF_Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EmailTemplateRender.Infrastructure;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TimeOffTracker.WebApi.MapperProfile;

namespace BusinessLogic.Infrastructure
{
    public static class BusinessConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            DataAccessConfiguration.ConfigureServices(services, configuration);
            RazorConfiguration.ConfigureServices(services, configuration);

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddScoped<IUserService, UserService>();

            services.AddSingleton(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MapperProfile());
            }).CreateMapper());

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITimeOffRequestService, TimeOffRequestService>();
            services.AddScoped<ITimeOffRequestReviewService, TimeOffRequestReviewService>();

            services.Configure<SmtpSettings>(opt => configuration.GetSection("SmtpSettings").Bind(opt));
            services.AddSingleton<IEmailService, EmailService>();
        }
        public static async Task ConfigureIdentityInicializerAsync(IServiceProvider provider)
        {
            var userManager = provider.GetRequiredService<UserManager<User>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            await new IdentityInitializer(userManager, roleManager).SeedAsync();
        }
    }
}
