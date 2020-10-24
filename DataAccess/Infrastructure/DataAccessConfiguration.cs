using DataAccess.Context;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Infrastructure
{
    public static class DataAccessConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRepository<User, int>, UserRepository>();
            services.AddScoped<IRepository<TimeOffRequest, int>, TimeOffRequestRepository>();
            services.AddScoped<IRepository<TimeOffRequestReview, int>, TimeOffRequestReviewRepository>();

            services.AddDbContext<TimeOffTrackerContext>(option =>
                option.UseSqlServer(configuration.GetConnectionString("defaultConnection")));

            services.AddIdentity<User, IdentityRole<int>>()
               .AddEntityFrameworkStores<TimeOffTrackerContext>()
               .AddDefaultTokenProviders();
        }
    }
}
