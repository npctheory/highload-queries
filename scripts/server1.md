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

dotnet add server/Api/Api.csproj reference server/Application/Application.csproj
dotnet add server/Api/Api.csproj reference server/Infrastructure/Infrastructure.csproj
dotnet add server/Infrastructure/Infrastructure.csproj reference server/Application/
dotnet add server/Application/Application.csproj reference server/Infrastructure/Infrastructure.csproj
dotnet add server/Application/Application.csproj reference server/Domain/Domain.csproj

rm server/Application/Class1.cs
rm server/Infrastructure/Class1.cs
rm server/Domain/Class1.cs

mkdir -p server/Api/Controllers
mkdir -p server/Application/Users/Queries/GetUser
mkdir -p server/Application/Users/Queries/SearchUsers
mkdir -p server/Infrastructure/Snapshots
mkdir -p server/Application/Mapping
mkdir -p server/Infrastructure/Common
mkdir -p server/Infrastructure/Repositories
mkdir -p server/Domain/Entities
mkdir -p server/Domain/Interfaces


touch server/Application/Users/Queries/GetUser/GetUserDTO.cs
touch server/Application/Users/Queries/SearchUsers/SearchUsersDTO.cs
touch server/Domain/Entities/User.cs
touch server/Infrastructure/Snapshots/UserSnapshot.cs
touch server/Infrastructure/Common/PostgresConnectionFactory.cs
touch server/Application/Mapping/MappingProfile.cs
touch server/Domain/Interfaces/IUserRepository.cs
touch server/Infrastructure/Repositories/UserRepository.cs
touch server/Api/Controllers/UserController.cs
touch server/Application/Users/Queries/GetUser/GetUserQuery.cs
touch server/Application/Users/Queries/GetUser/GetUserQueryHandler.cs
touch server/Application/Users/Queries/SearchUsers/SearchUsersQuery.cs
touch server/Application/Users/Queries/SearchUsers/SearchUsersQueryHandler.cs


dotnet add server/Application/ package AutoMapper
dotnet add server/Application/ package MediatR
dotnet add server/Infrastructure/ package Bogus
dotnet add server/Infrastructure/ package Npgsql
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
using System.Net.Sockets;
using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Bogus;
using Npgsql;
using Application.Interfaces;
using Application.DAO;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<UserDAO> GetUserByIdAsync(string userId)
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
                        return new UserDAO
                        {
                            Id = reader.GetString(0),
                            PasswordHash = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            SecondName = reader.GetString(3),
                            Birthdate = reader.GetDateTime(4),
                            Biography = reader.IsDBNull(5) ? null : reader.GetString(5),
                            City = reader.IsDBNull(6) ? null : reader.GetString(6)
                        };
                    }
                    return null;
                }
            }
        }
    }

    public async Task<List<UserDAO>> SearchUsersAsync(string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName)) throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrEmpty(lastName)) throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));

        var users = new List<UserDAO>();

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
                        users.Add(new UserDAO
                        {
                            Id = reader.GetString(0),
                            PasswordHash = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            SecondName = reader.GetString(3),
                            Birthdate = reader.GetDateTime(4),
                            Biography = reader.IsDBNull(5) ? null : reader.GetString(5),
                            City = reader.IsDBNull(6) ? null : reader.GetString(6)
                        });
                    }
                }
            }
        }

        return users;
    }

    public async Task AddUserAsync(UserDAO user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var faker = new Faker();
        var randomUsername = faker.Random.Word() + faker.Random.Word() + faker.Random.Number(1000, 9999);

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand("INSERT INTO users (id, password_hash, first_name, second_name, birthdate, biography, city) VALUES (@id, @passwordHash, @firstName, @secondName, @birthdate, @biography, @city)", connection))
            {
                command.Parameters.AddWithValue("id", randomUsername);
                command.Parameters.AddWithValue("passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("firstName", user.FirstName);
                command.Parameters.AddWithValue("secondName", user.SecondName);
                command.Parameters.AddWithValue("birthdate", user.Birthdate);
                command.Parameters.AddWithValue("biography", user.Biography ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("city", user.City ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
```

**GetUserDTO.cs**
```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.Queries.GetUser;

public record GetUserDTO(
    string Id,
    string FirstName,
    string SecondName,
    DateTime Birthdate,
    string Biography,
    string City
);
```

**SearchUsersDTO.cs**
```csharp
using System;
using System.Collections.Generic;

namespace Application.Users.Queries.SearchUsers
{
    public class SearchUsersDTO
    {
        public List<UserDTO> Users { get; set; } = new List<UserDTO>();

        public class UserDTO
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string SecondName { get; set; }
            public DateTime Birthdate { get; set; }
            public string Biography { get; set; }
            public string City { get; set; }
        }
    }
}
```

**MappingProfile.cs**
```csharp
using AutoMapper;
using Application.DTO;
using Domain.Entities;
using Application.DAO;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserSnapshot, User>();
            CreateMap<User, UserDTO>();
        }
    }
}
```


**GetUserQuery.cs**
```csharp
using Application.DTO;
using MediatR;

namespace Application.Users.Queries.GetUser;

public record GetUserQuery(string Id) : IRequest<GetUserDTO>;
```


**GetUserQueryHandler.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.DAO;
using Application.Users.Queries.GetUser;
using Domain.Entities;
using AutoMapper;
using MediatR;
using Application.DTO;

namespace Application.Users.Queries.GetUser
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, GetUserDTO>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IMapper _mapper;

        public GetUserQueryHandler(IUserRepository usersRepository, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _mapper = mapper;
        }

        public async Task<UserDTO> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            UserDAO userDAO = await _usersRepository.GetUserByIdAsync(request.Id);
            User user = _mapper.Map<User>(userDAO);
            return _mapper.Map<UserDTO>(user);
        }
    }
}
```

**SearchUsersQuery.cs**
```csharp
using Application.DTO;
using MediatR;

namespace Application.Users.Queries.SearchUsers;

public record SearchUsersQuery(string first_name, string second_name) : IRequest<SearchUsersDTO>;
```

**SearchUsersQueryHandler.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.DAO;
using Application.Users.Queries.SearchUsers;
using Domain.Entities;
using AutoMapper;
using MediatR;
using Application.DTO;

namespace Application.Users.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<SearchUsersDTO>>
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
            List<UserDAO> userDAOs = await _userRepository.SearchUsersAsync(request.first_name, request.second_name);
            List<User> users = _mapper.Map<List<User>>(userDAOs);
            return _mapper.Map<List<UserDTO>>(users);
        }
    }
}
```

**UserController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Application.DTO;
using Application.Users.Queries.GetUser;
using Application.Users.Queries.SearchUsers;
using MediatR;

namespace Api.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ISender _mediator;

        public UserController(ISender mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet("user/get/{id}")]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {
            var userResult = await _mediator.Send(new GetUserQuery(id));        
            return Ok(userResult);
        }

        [AllowAnonymous]
        [HttpGet("user/search")]
        public async Task<IActionResult> SearchUsersAsync([FromQuery] string first_name, [FromQuery] string second_name)
        {
            var usersResult = await _mediator.Send(new SearchUsersQuery(first_name,second_name));
            return Ok(usersResult);
        }
    }
}
```


**PostgresConnectionFactory.cs**
```csharp
namespace Infrastructure.Common;

using Npgsql;
using System;

public class PostgresConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory()
    {
        _connectionString = BuildConnectionString();
    }

    private string BuildConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("PRIMARY_DB_HOST") ?? throw new InvalidOperationException("PRIMARY_DB_HOST environment variable is not set.");
        var port = Environment.GetEnvironmentVariable("PRIMARY_DB_PORT") ?? throw new InvalidOperationException("PRIMARY_DB_PORT environment variable is not set.");
        var database = Environment.GetEnvironmentVariable("PRIMARY_DB_NAME") ?? throw new InvalidOperationException("PRIMARY_DB_NAME environment variable is not set.");
        var username = Environment.GetEnvironmentVariable("PRIMARY_DB_USER") ?? throw new InvalidOperationException("PRIMARY_DB_USER environment variable is not set.");
        var password = Environment.GetEnvironmentVariable("PRIMARY_DB_PASSWORD") ?? throw new InvalidOperationException("PRIMARY_DB_PASSWORD environment variable is not set.");

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
```


**Program.cs**
```csharp
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Infrastructure.Common;
using Application.Interfaces;
using Infrastructure.Repositories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
builder.Services.AddSingleton<PostgresConnectionFactory>();
builder.Services.AddScoped<IUserRepository>(sp => new UserRepository(sp.GetRequiredService<PostgresConnectionFactory>().CreateConnection().ConnectionString));
builder.Services.AddControllers();
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```