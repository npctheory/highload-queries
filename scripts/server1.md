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

rm server/Application/Class1.cs
rm server/Infrastructure/Class1.cs
rm server/Domain/Class1.cs

mkdir -p server/Api/Controllers
mkdir -p server/Application/Users/Queries/GetUser
mkdir -p server/Application/Users/Queries/SearchUsers
mkdir -p server/Application/Users/DTO
mkdir -p server/Application/Mapping
mkdir -p server/Infrastructure/Snapshots
mkdir -p server/Infrastructure/Mapping
mkdir -p server/Infrastructure/Repositories
mkdir -p server/Domain/Entities
mkdir -p server/Domain/Interfaces



touch server/Application/DependencyInjection.cs
touch server/Infrastructure/DependencyInjection.cs
touch server/Domain/Entities/User.cs
touch server/Infrastructure/Snapshots/UserSnapshot.cs
touch server/Application/Users/DTO/UserDTO.cs
touch server/Application/Mapping/ApplicationProfile.cs
touch server/Infrastructure/Mapping/InfrastructureProfile.cs
touch server/Domain/Interfaces/IUserRepository.cs
touch server/Infrastructure/Repositories/UserRepository.cs
touch server/Api/Controllers/UserController.cs
touch server/Application/Users/Queries/GetUser/GetUserQuery.cs
touch server/Application/Users/Queries/GetUser/GetUserQueryHandler.cs
touch server/Application/Users/Queries/SearchUsers/SearchUsersQuery.cs
touch server/Application/Users/Queries/SearchUsers/SearchUsersQueryHandler.cs


dotnet add server/Application/ package MediatR
dotnet add server/Application/ package AutoMapper
dotnet add server/Application/ package Microsoft.Extensions.Configuration
dotnet add server/Infrastructure/ package AutoMapper
dotnet add server/Infrastructure/ package Bogus
dotnet add server/Infrastructure/ package Npgsql
dotnet add server/Infrastructure/ package Microsoft.Extensions.Configuration
```


**User.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class User
{
    public string Id { get; set; }

    public string? Password { get; set; }

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

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<User> GetUserByIdAsync(string userId);
    Task<List<User>> SearchUsersAsync(string firstName, string lastName);
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


**Infrastructure/DependencyInjection.cs**
```csharp
using Domain.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Infrastructure.Mapping;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(cfg => 
            {
                cfg.AddProfile<InfrastructureProfile>();
            }, typeof(DependencyInjection).Assembly);


            services.AddScoped<IUserRepository>(sp =>
            {
                var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;
                var mapper = sp.GetRequiredService<IMapper>();
                return new UserRepository(connectionString, mapper);
            });

            return services;
        }
    }
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

**Program.cs**
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
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
    }
}
```