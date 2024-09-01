using Domain.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Infrastructure.Mapping;
using StackExchange.Redis;
using Application.Abstractions;
using Application.Services;
using Infrastructure.Services;
using MassTransit;
using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Infrastructure.Generators;
using System.Text;
using Infrastructure.Providers;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDateTimeProvider,DateTimeProvider>();
            services.AddSingleton<IJwtTokenGenerator,JwtTokenGenerator>();
            services.AddSingleton<IPasswordHasher,PasswordHasher>();


            services.AddAutoMapper(cfg => 
            {
                cfg.AddProfile<InfrastructureProfile>();
            }, typeof(DependencyInjection).Assembly);

            var redisConnectionString = configuration.GetSection("RedisSettings:ConnectionString").Value;
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddSingleton<ICacheService>(sp =>
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                return new RedisCacheService(redis, databaseIndex: 0);
            });

            // Register RedisMessageRepository with database 1
            // services.AddSingleton<IMessageRepository>(sp => new RedisMessageRepository(redis, databaseIndex: 1));

            services.AddScoped<IUserRepository>(sp =>
            {
                var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;
                var mapper = sp.GetRequiredService<IMapper>();
                return new UserRepository(connectionString, mapper);
            });


            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.UsingRabbitMq((context, rabbitMqConfigurator) =>
                {
                    var rabbitMqSettings = configuration.GetSection("RabbitMqSettings");

                    rabbitMqConfigurator.Host(rabbitMqSettings["HostName"], "/", h =>
                    {
                        h.Username(rabbitMqSettings["UserName"]);
                        h.Password(rabbitMqSettings["Password"]);
                    });

                    rabbitMqConfigurator.ReceiveEndpoint(rabbitMqSettings["QueueName"], endpointConfigurator =>
                    {

                    });
                });
            });
            services.AddTransient<IEventBus,EventBus>();

            return services;
        }
    }
}
