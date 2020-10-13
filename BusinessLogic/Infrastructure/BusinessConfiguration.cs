using AutoMapper;
using BusinessLogic.Services;
using DataAccess.Context;
using DataAccess.Infrastructure;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TimeOffTracker.WebApi.MapperProfile;

namespace BusinessLogic.Infrastructure
{
    public static class BusinessConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {            
            services.AddTransient(typeof(TimeOffRequestService));

            services.AddScoped<IRepository<User, string>, UserRepository>();

            services.AddScoped(typeof(UserService));

            services.AddSingleton(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MapperProfile());
            }).CreateMapper());

            DataAccessConfiguration.ConfigureServices(services, configuration);
        }
        public static async Task ConfigureIdentityInicializerAsync(IServiceProvider provider)
        {
            var userManager = provider.GetRequiredService<UserManager<User>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            await new IdentityInitializer(userManager, roleManager).SeedAsync();
        }
    }
}
