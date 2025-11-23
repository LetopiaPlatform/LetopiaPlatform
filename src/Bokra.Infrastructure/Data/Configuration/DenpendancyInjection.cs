
using Bokra.Core.Interfaces;
using Bokra.Infrastructure.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace Bokra.Infrastructure.Data.Configuration
{
    public static class DenpendancyInjection
    {
        public static IServiceCollection AddDebendancy(this IServiceCollection service)
        {
          
            

            service.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            service.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
           
            return service;
        }
    }
}
