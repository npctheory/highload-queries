```bash
dotnet new sln -o HighloadSocial
mv HighloadSocial server

dotnet new webapi -o server/Api
dotnet new classlib -o server/Application
dotnet new classlib -o server/Infrastructure
dotnet new classlib -o server/Domain

dotnet sln server/HighloadSocial.sln add server/Api/
dotnet sln server/HighloadSocial.sln add server/Application/
dotnet sln server/HighloadSocial.sln add server/Infrastructure/
dotnet sln server/HighloadSocial.sln add server/Domain/

dotnet add server/Application/Application.csproj reference server/Domain/Domain.csproj
dotnet add server/Infrastructure/Infrastructure.csproj reference server/Domain/Domain.csproj server/Application/
dotnet add server/Api/Api.csproj reference server/Application/Application.csproj server/Infrastructure/Infrastructure.csproj

rm server/Api/Api.http
rm server/Application/Class1.cs
rm server/Infrastructure/Class1.cs
rm server/Domain/Class1.cs

mkdir -p server/Api/Controllers
mkdir -p server/Application/Users/Queries/GetUser
mkdir -p server/Application/Users/Queries/SearchUsers
mkdir -p server/Application/Users/Queries/Login
mkdir -p server/Application/Users/DTO
mkdir -p server/Application/Mapping
mkdir -p server/Application/Abstractions
mkdir -p server/Infrastructure/Snapshots
mkdir -p server/Infrastructure/Mapping
mkdir -p server/Infrastructure/Configuration
mkdir -p server/Infrastructure/Repositories
mkdir -p server/Infrastructure/Services
mkdir -p server/Infrastructure/Generators
mkdir -p server/Infrastructure/Providers
mkdir -p server/Domain/Entities
mkdir -p server/Domain/Interfaces


touch server/Api/DependencyInjection.cs
touch server/Application/DependencyInjection.cs
touch server/Infrastructure/DependencyInjection.cs
touch server/Domain/Entities/User.cs
touch server/Infrastructure/Snapshots/UserSnapshot.cs

touch server/Application/Abstractions/ICacheService.cs
touch server/Application/Abstractions/IDateTimeProvider.cs
touch server/Application/Abstractions/IEventBus.cs
touch server/Application/Abstractions/IJwtTokenGenerator.cs


touch server/Infrastructure/Configuration/JwtSettings.cs
touch server/Infrastructure/Services/RedisCacheService.cs
touch server/Infrastructure/Services/EventBus.cs
touch server/Infrastructure/Providers/DateTimeProvider.cs
touch server/Infrastructure/Generators/JwtTokenGenerator.cs



touch server/Domain/Interfaces/IUserRepository.cs
touch server/Application/Users/DTO/UserDTO.cs
touch server/Application/Users/DTO/TokenDTO.cs
touch server/Application/Mapping/ApplicationProfile.cs
touch server/Application/Users/Queries/GetUser/GetUserQuery.cs
touch server/Application/Users/Queries/GetUser/GetUserQueryHandler.cs
touch server/Application/Users/Queries/SearchUsers/SearchUsersQuery.cs
touch server/Application/Users/Queries/SearchUsers/SearchUsersQueryHandler.cs
touch server/Application/Users/Queries/Login/LoginQuery.cs
touch server/Application/Users/Queries/Login/LoginQueryHandler.cs
touch server/Infrastructure/Mapping/InfrastructureProfile.cs
touch server/Infrastructure/Repositories/UserRepository.cs
touch server/Api/Controllers/UserController.cs
touch server/Api/Dockerfile




dotnet add server/Api/ package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add server/Api/ package Microsoft.Extensions.Options
dotnet add server/Api/ package Microsoft.Extensions.Options.ConfigurationExtensions
dotnet add server/Application/ package MediatR
dotnet add server/Application/ package AutoMapper
dotnet add server/Application/ package Microsoft.Extensions.Configuration

dotnet add server/Infrastructure/ package Microsoft.IdentityModel.Tokens
dotnet add server/Infrastructure/ package System.IdentityModel.Tokens.Jwt
dotnet add server/Infrastructure/ package AutoMapper
dotnet add server/Infrastructure/ package Bogus
dotnet add server/Infrastructure/ package Npgsql
dotnet add server/Infrastructure/ package StackExchange.Redis
dotnet add server/Infrastructure/ package MassTransit
dotnet add server/Infrastructure/ package MassTransit.RabbitMQ
```


**User.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class User
{
    public string Id { get; set; }

    public string PasswordHash {get; set; }

    public string FirstName { get; set; }

    public string SecondName { get; set; }

    public DateTime Birthdate { get; set; }

    public string Biography { get; set; }

    public string City { get; set; }
}
```

**IUserRepository.cs**
```csharp
using Domain.Entities;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(string userId);
        Task<List<User>> SearchUsersAsync(string firstName, string lastName);
        Task CreateUserAsync(User user); // Add this line
    }
}
```

**UserSnapshot.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Snapshots;

public class UserSnapshot
{
    public string Id { get; set; }

    public string PasswordHash { get; set; }

    public string FirstName { get; set; }

    public string SecondName { get; set; }

    public DateTime Birthdate { get; set; }

    public string Biography { get; set; }

    public string City { get; set; }
}
```

**UserRepository.cs**
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Npgsql;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Snapshots;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly IMapper _mapper;

        public UserRepository(string connectionString, IMapper mapper)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT id, password_hash, first_name, second_name, birthdate, biography, city FROM users WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("id", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var userSnapshot = new UserSnapshot
                            {
                                Id = reader.GetString(0),
                                PasswordHash = reader.GetString(1),
                                FirstName = reader.GetString(2),
                                SecondName = reader.GetString(3),
                                Birthdate = reader.GetDateTime(4),
                                Biography = reader.IsDBNull(5) ? null : reader.GetString(5),
                                City = reader.IsDBNull(6) ? null : reader.GetString(6)
                            };

                            return _mapper.Map<User>(userSnapshot);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<List<User>> SearchUsersAsync(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName)) throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
            if (string.IsNullOrEmpty(lastName)) throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));

            var users = new List<User>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT id, password_hash, first_name, second_name, birthdate, biography, city 
                    FROM users 
                    WHERE first_name ILIKE @firstName AND second_name ILIKE @lastName";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("firstName", $"{firstName}%");
                    command.Parameters.AddWithValue("lastName", $"{lastName}%");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var userSnapshot = new UserSnapshot
                            {
                                Id = reader.GetString(0),
                                PasswordHash = reader.GetString(1),
                                FirstName = reader.GetString(2),
                                SecondName = reader.GetString(3),
                                Birthdate = reader.GetDateTime(4),
                                Biography = reader.IsDBNull(5) ? null : reader.GetString(5),
                                City = reader.IsDBNull(6) ? null : reader.GetString(6)
                            };

                            users.Add(_mapper.Map<User>(userSnapshot));
                        }
                    }
                }
            }

            return users;
        }

        public async Task CreateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO users (id, password_hash, first_name, second_name, birthdate, biography, city) 
                    VALUES (@id, @passwordHash, @firstName, @secondName, @birthdate, @biography, @city)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("id", user.Id);
                    command.Parameters.AddWithValue("passwordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("firstName", user.FirstName);
                    command.Parameters.AddWithValue("secondName", user.SecondName);
                    command.Parameters.AddWithValue("birthdate", user.Birthdate);
                    command.Parameters.AddWithValue("biography", (object)user.Biography ?? DBNull.Value);
                    command.Parameters.AddWithValue("city", (object)user.City ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
```

**InfrastructureProfile.cs**
```csharp
using AutoMapper;
using Domain.Entities;
using Infrastructure.Snapshots;

namespace Infrastructure.Mapping
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<UserSnapshot, User>();
        }
    }
}
```

**ICacheService.cs**
```csharp
namespace Application.Abstractions;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    Task<T> SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default);
}
```
**IDateTimeProvider.cs**
```csharp
namespace Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow {get;}
}
```

**IEventBus.cs**
```csharp
namespace Application.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationtoken = default);
}
```

**IJwtTokenGenerator.cs**
```csharp
namespace Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(string user_id,string first_name,string second_name);
}
```

**IPasswordHasher.cs**
```csharp
namespace Application.Abstractions;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
```


**UserDTO.cs**
```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.DTO;

public record UserDTO(
    string Id,
    string FirstName,
    string SecondName,
    DateTime Birthdate,
    string Biography,
    string City
);
```

**TokenDTO.cs**
```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.DTO;

public class TokenDTO
{
    public string Value { get; set; }
}
```

**GetUserQuery.cs**
```csharp
using Application.Users.DTO;
using Domain.Entities;
using MediatR;

namespace Application.Users.Queries.GetUser;

public record GetUserQuery(string Id) : IRequest<UserDTO>;
```


**GetUserQueryHandler.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Users.Queries.GetUser;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using MediatR;
using Application.Users.DTO;

namespace Application.Users.Queries.GetUser
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDTO> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            User user = await _userRepository.GetUserByIdAsync(request.Id);
            return _mapper.Map<UserDTO>(user);
        }
    }
}
```


**SearchUsersQuery.cs**
```csharp
using Application.Users.DTO;
using Domain.Entities;
using MediatR;

namespace Application.Users.Queries.SearchUsers;

public record SearchUsersQuery(string first_name, string second_name) : IRequest<List<UserDTO>>;
```

**SearchUsersQueryHandler.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Users.Queries.SearchUsers;
using Domain.Entities;
using AutoMapper;
using MediatR;
using Domain.Interfaces;
using Application.Users.DTO;

namespace Application.Users.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public SearchUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<UserDTO>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            List<User> users = await _userRepository.SearchUsersAsync(request.first_name, request.second_name);
            return _mapper.Map<List<UserDTO>>(users);
        }
    }
}
```

**LoginQuery.cs**
```csharp
using Application.Users.DTO;
using MediatR;

namespace Application.Users.Queries.Login;

public record LoginQuery(string Id, string Password) : IRequest<TokenDTO>;
```

**LoginQueryHandler.cs**
```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading;
using Application.Users.DTO;
using Application.Abstractions;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Security.Claims;

namespace Application.Users.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, TokenDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;

        public LoginQueryHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<TokenDTO> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            User user = await _userRepository.GetUserByIdAsync(request.Id);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid user ID or password.");
            }

            TokenDTO token = new TokenDTO
            {
                Value = _jwtTokenGenerator.GenerateToken(user.Id, user.FirstName, user.SecondName),
            };


            return token;
        }

        private bool VerifyPassword(string password, string storedPasswordHash)
        {
            var parts = storedPasswordHash.Split(':');
            if (parts.Length != 2) return false;

            var salt = parts[0];
            var hash = parts[1];

            using (var sha256 = SHA256.Create())
            {
                var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
                var computedHashString = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

                return hash == computedHashString;
            }
        }
    }
}
```

**RegisterQuery.cs**
```csharp
using Application.Users.DTO;
using MediatR;

namespace Application.Users.Queries.Register;

public record RegisterQuery(string FirstName, string SecondName, string Birthdate, string Biography, string City, string Password) : IRequest<UserDTO>;
```

**RegisterQueryHandler.cs**
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Users.DTO;
using Application.Abstractions;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Users.Queries.Register
{
    public class RegisterQueryHandler : IRequestHandler<RegisterQuery, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterQueryHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IMapper mapper,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<UserDTO> Handle(RegisterQuery request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                Birthdate = DateTime.Parse(request.Birthdate),
                Biography = request.Biography,
                City = request.City,
                PasswordHash = _passwordHasher.HashPassword(request.Password)
            };

            await _userRepository.CreateUserAsync(user);

            return _mapper.Map<UserDTO>(user);
        }
    }
}
```

**ApplicationProfile.cs**
```csharp
using Application.Users.DTO;
using Application.Users.Queries.GetUser;
using Application.Users.Queries.SearchUsers;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<User, UserDTO>();
        }
    }
}
```


**Application/DependencyInjection.cs**
```csharp
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Application.Mapping;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => 
            {
                cfg.AddProfile<ApplicationProfile>();
            }, typeof(DependencyInjection).Assembly);


            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

            return services;
        }
    }
}
```

**JwtSettings.cs**
```csharp
namespace Infrastructure.Configuration;

public class JwtSettings
{
    public string Secret {get; init;} = null!;
    public int ExpirationTimeInMinutes {get;init;}
    public string Issuer {get;init;} = null!;
    public string Audience{get;init;} = null!;
}
```

**JwtTokenGenerator.cs**
```csharp
using System.Net.Mime;
using System;
using System.Security.Claims;
using System.Text;
using Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Infrastructure.Configuration;

namespace Infrastructure.Generators;


public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IDateTimeProvider dateTimeProvider, IOptions<JwtSettings> jwtOptions)
    {
        _dateTimeProvider = dateTimeProvider;
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(string user_id, string first_name, string second_name)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user_id),
            new Claim(JwtRegisteredClaimNames.GivenName, first_name),
            new Claim(JwtRegisteredClaimNames.FamilyName, second_name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.ExpirationTimeInMinutes),
            claims:claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

}
```

**DateTimeProvider.cs**
```csharp
namespace Infrastructure.Providers;

using Application.Abstractions;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
```


**EventBus.cs**
```csharp
using Application.Abstractions;

namespace Infrastructure.Services;

public class EventBus : IEventBus
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationtoken = default)
    {
        throw new NotImplementedException();
    }
}
```

**RedisCacheService.cs**
```csharp
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using Application.Abstractions;

namespace Application.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;

        // Constructor now takes a database index
        public RedisCacheService(IConnectionMultiplexer redis, int databaseIndex = 0)
        {
            _database = redis.GetDatabase(databaseIndex);
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            var redisValue = await _database.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<T>(redisValue);
        }

        public async Task<T> SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var serializedValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedValue);

            return value;
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            await _database.KeyDeleteAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prefixKey))
                throw new ArgumentException("Prefix key cannot be null or whitespace.", nameof(prefixKey));

            var server = GetServer();
            var keys = server.Keys(pattern: $"{prefixKey}*");

            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }

        private IServer GetServer()
        {
            var endpoints = _database.Multiplexer.GetEndPoints();
            return _database.Multiplexer.GetServer(endpoints[0]);
        }
    }
}
```

**PasswordHasher.cs**
```csharp
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;

namespace Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 16 bytes = 128 bits

    public string HashPassword(string password)
    {
        var salt = GenerateSalt();
        var hash = ComputeHash(password, salt);
        return $"{salt}:{hash}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(':');
        if (parts.Length != 2)
        {
            throw new FormatException("The hashed password format is invalid.");
        }

        var salt = parts[0];
        var hash = parts[1];

        var computedHash = ComputeHash(password, salt);
        return hash.Equals(computedHash, StringComparison.OrdinalIgnoreCase);
    }

    private string GenerateSalt()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var saltBytes = new byte[SaltSize];
            rng.GetBytes(saltBytes);
            return BitConverter.ToString(saltBytes).Replace("-", "").ToLower();
        }
    }

    private string ComputeHash(string password, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var passwordWithSaltBytes = Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = sha256.ComputeHash(passwordWithSaltBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
```

**Infrastructure/DependencyInjection.cs**
```csharp
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
```



**appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "Secret": "this-is-a-very-secure-and-long-key-32-bytes-long",
    "ExpirationTimeInMinutes": 60000,
    "Issuer": "HighloadSocial",
    "Audience": "HighloadSocial"
  },
  "RabbitMqSettings": {
    "HostName": "rabbitmq",
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "my_queue"
  },
  "RedisSettings": {
    "ConnectionString": "redis:6379"
  },
  "DatabaseSettings": {
    "ConnectionString": "Host=pg_master;Port=5432;Database=highloadsocial;Username=postgres;Password=postgres;"
  }  
}
```

**appsettings.Development.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "Secret": "this-is-a-very-secure-and-long-key-32-bytes-long",
    "ExpirationTimeInMinutes": 60000,
    "Issuer": "HighloadSocial",
    "Audience": "HighloadSocial"
  },
  "RabbitMqSettings": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "my_queue"
  },
  "RedisSettings": {
    "ConnectionString": "localhost:6379"
  },
  "DatabaseSettings": {
    "ConnectionString": "Host=localhost;Port=35432;Database=highloadsocial;Username=postgres;Password=postgres;"
  }  
}
```

**Api/DependencyInjection.cs**
```csharp
using System.Text;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            return services;
        }
    }
}
```


**Program.cs**
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application;
using Infrastructure;
using Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);



builder.Services.AddControllers();

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

**UserController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Users.Queries.GetUser;
using Application.Users.Queries.SearchUsers;
using MediatR;
using Domain.Entities;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Users.DTO;
using System.Text.Json;
using Application.Users.Queries.Login;
using Application.Users.Queries.Register;

namespace Api.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public UserController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("user/get/{id}")]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {
            UserDTO user = await _mediator.Send(new GetUserQuery(id));
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("user/search")]
        public async Task<IActionResult> SearchUsersAsync([FromQuery] string first_name, [FromQuery] string second_name)
        {
            List<UserDTO> users = await _mediator.Send(new SearchUsersQuery(first_name, second_name));
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement jsonElement)
        {
            string id = jsonElement.GetProperty("id").GetString();
            string password = jsonElement.GetProperty("password").GetString();

            TokenDTO token = await _mediator.Send(new LoginQuery(id,password)); 
            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] JsonElement jsonElement)
        {
            string first_name = jsonElement.GetProperty("first_name").GetString();
            string second_name = jsonElement.GetProperty("second_name").GetString();
            string birthdate = jsonElement.GetProperty("birthdate").GetString();
            string biography = jsonElement.GetProperty("biography").GetString();
            string city = jsonElement.GetProperty("city").GetString();
            string password = jsonElement.GetProperty("password").GetString();

            UserDTO user = await _mediator.Send(new RegisterQuery(first_name,second_name,birthdate,biography,city,password)); 
            return Ok(user);
        }
    }
}
```


```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY HighloadSocial.sln ./
COPY Api/Api.csproj ./Api/
COPY Application/Application.csproj ./Application/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY Domain/Domain.csproj ./Domain/
RUN dotnet restore
COPY . .
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
RUN apt-get update && \
    apt-get install -y python3 python3-venv python3-pip && \
    python3 -m venv /opt/venv && \
    /opt/venv/bin/python -m pip install --upgrade pip && \
    /opt/venv/bin/python -m pip install psycopg2-binary requests==2.31.0
ENV VIRTUAL_ENV=/opt/venv
ENV PATH="$VIRTUAL_ENV/bin:$PATH"
ARG UID=10001
RUN adduser --disabled-password --gecos "" --home "/nonexistent" --shell "/sbin/nologin" --no-create-home --uid "${UID}" appuser
USER appuser
EXPOSE 80
ENTRYPOINT ["dotnet", "Api.dll"]
```


**launchSettings.json**
```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:8080",
      "sslPort": 44394
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7140;http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```