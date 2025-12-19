using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Persistence.Concretes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ITokenService, TokenService>();


        }
    }
}
