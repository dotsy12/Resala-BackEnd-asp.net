using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Infrastructure.Persistence;
using BackEnd.Infrastructure.Persistence.DbContext;
using BackEnd.Infrastructure.Persistence.Repositories;
using BackEnd.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.InfrastructureDependencies
{
    public static class InfrastructureDependencies
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

          //  services.AddScoped<IIdentityService, IdentityServies>();
           
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
           services.AddScoped<IJwtService, JwtService>();
           services.AddScoped<IOtpService, OtpService>();
           services.AddScoped<IEmailService, EmailService>();
            // InfrastructureModule.cs — أضف هذه الأسطر الثلاثة
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDonorRepository, DonorRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();

            services.AddScoped<ISponsorshipRepository, SponsorshipRepository>();

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();


            // أضف كمان
            services.AddHttpContextAccessor();


            services.AddScoped(
                    typeof(IGenericRepository<,>),
                    typeof(GenericRepository<,>)

                );
            return services;

        }
    }
}
