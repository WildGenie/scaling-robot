﻿using FluentValidation;
using Librarian.Application.Common.Behaviors;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Librarian.Application
{
    /*
     * Uygulama katmanının içindeki servisleri kullanan tarafa(örneğin Web API) bildirmek için kullandığımız bir sınıf olarak düşünebiliriz.
     * IServiceCollection zaten .Net'in dahili DI servislerine ulaşmakta önemli bir aracı.
     * AutoMapper, MediatR, Validation ve Behavior'ları servisler koleksiyonuna ekleyen bir metot.
     * 
     * Bu sınıfı büyük ihtimalle Web API projesinin Startup'ındaki ConfigureServices metodunda kullanacağız. Böylece Web API çalışma zamanına
     * gerekli DI bağımlılıkları otomatik olarak yüklenmiş olacak.
     */
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Redis entegrasyonu. Servis adresi çalıştıran uygulamanın appSettings kısmından alınacağı için metoda IConfiguration arayüzü de eklendi.
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisServer");
                options.InstanceName = Assembly.GetExecutingAssembly().GetName().Name;
            });

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehavior<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));

            return services;
        }
    }
}
