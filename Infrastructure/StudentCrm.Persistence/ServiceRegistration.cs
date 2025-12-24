using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentCrm.Application.Abstract.Repositories.Coordinators;
using StudentCrm.Application.Abstract.Repositories.Courses;
using StudentCrm.Application.Abstract.Repositories.Enrollments;
using StudentCrm.Application.Abstract.Repositories.Sections;
using StudentCrm.Application.Abstract.Repositories.Students;
using StudentCrm.Application.Abstract.Repositories.Teachers;
using StudentCrm.Application.Abstract.Repositories.Terms;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Infrastructure.Concretes;
using StudentCrm.Persistence.Concretes.Repositories.Coordinators;
using StudentCrm.Persistence.Concretes.Repositories.Courses;
using StudentCrm.Persistence.Concretes.Repositories.Enrollments;
using StudentCrm.Persistence.Concretes.Repositories.Sections;
using StudentCrm.Persistence.Concretes.Repositories.Students;
using StudentCrm.Persistence.Concretes.Repositories.Teachers;
using StudentCrm.Persistence.Concretes.Repositories.Terms;
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
            services.AddScoped<IMailService, MailService>();


            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ITeacherReadRepository, TeacherReadRepository>();
            services.AddScoped<ITeacherWriteRepository, TeacherWriteRepository>();

            services.AddScoped<ITermService, TermService>();
            services.AddScoped<ITermReadRepository,TermReadRepository>();
            services.AddScoped<ITermWriteRepository, TermWriteRepository>();

            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ISectionReadRepository, SectionReadRepository>();
            services.AddScoped<ISectionWriteRepository, SectionWriteRepository>();

            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IStudentReadRepository, StudentReadRepository>();
            services.AddScoped<IStudentWriteRepository, StudentWriteRepository>();

            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ICourseReadRepository, CourseReadRepository>();
            services.AddScoped<ICourseWriteRepository, CourseWriteRepository>();

            services.AddScoped<IEnrollmentService,EnrollmentService>();
            services.AddScoped<IEnrollmentReadRepository, EnrollmentReadRepository>();
            services.AddScoped<IEnrollmentWriteRepository, EnrollmentWriteRepository>();

            services.AddScoped<ICoordinatorService, CoordinatorService>();  
            services.AddScoped<ICoordinatorReadRepository, CoordinatorReadRepository>();
            services.AddScoped<ICoordinatorWriteRepository, CoordinatorWriteRepository>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();



        }
    }
}
