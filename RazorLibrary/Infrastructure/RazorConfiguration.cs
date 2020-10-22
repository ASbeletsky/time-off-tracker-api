using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLibrary.Services;
using RazorLibrary.Services.Interfaces;

namespace RazorLibrary.Infrastructure
{
    public static class RazorConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorPages();

            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            services.AddLocalization(opt => opt.ResourcesPath = "Resources");
        }
    }
}
