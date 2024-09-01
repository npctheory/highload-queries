```bash
dotnet add server/Api/ package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add server/Infrastructure/ package Microsoft.Extensions.Options
dotnet add server/Infrastructure/ package System.IdentityModel.Tokens.Jwt

mkdir -p server/Infrastructure/Generators
mkdir -p server/Infrastructure/Providers
mkdir -p server/Application/Users/Queries/Login


touch server/Application/Abstractions/IDateTimeProvider.cs
touch server/Application/Abstractions/IJwtTokenGenerator.cs
touch server/Infrastructure/Providers/DateTimeProvider.cs
touch server/Infrastructure/Generators/JwtTokenGenerator.cs
touch server/Infrastructure/Configuration/JwtSettings.cs

touch server/Application/Users/DTO/TokenDTO.cs

touch server/Application/Users/Queries/Login/LoginQuery.cs
touch server/Application/Users/Queries/Login/LoginQueryHandler.cs
```

**IDateTimeProvider.cs**
```csharp
namespace Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow {get;}
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
**IJwtTokenGenerator.cs**
```csharp
namespace Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(string user_id,string first_name,string second_name);
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


**TokenDTO.cs**
```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.DTO;

public class TokenDTO
{
    public string? token { get; set; }
}
```


**LoginQuery.cs**
```csharp
using Application.DTO;
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
using Application.Interfaces;
using Application.DTO;
using Domain.Entities;
using Application.DAO;
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
            ITokenRepository tokenRepository,
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

            if (user == null || !VerifyPassword(request.Password, userDAO.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid user ID or password.");
            }

            TokenDTO token = new TokenDTO
            {
                UserId = user.Id,
                Value = _jwtTokenGenerator.GenerateToken(user.Id, user.FirstName, user.SecondName),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(2)
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

**appsettings.json**
```csharp
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
            CreateMap<UserDAO, User>();
            CreateMap<User, UserDTO>();

            CreateMap<TokenDAO, Token>();
            CreateMap<Token, TokenDAO>();
            
            CreateMap<Token, TokenDTO>()
                .ForMember(dest => dest.token, opt => opt.MapFrom(src => src.Value));

            CreateMap<TokenDTO, Token>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.token));
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
using Application.Users.Queries.Login;
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement jsonElement)
        {
            string id = jsonElement.GetProperty("id").GetString();
            string password = jsonElement.GetProperty("password").GetString();
            var loginResult = await _mediator.Send(new LoginQuery(id,password)); 
            return Ok(loginResult);
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

**Program.cs**
```csharp
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Infrastructure.Common;
using Application.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Providers;
using Infrastructure.Generators;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Application.Mapping;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);
{
    var configuration = builder.Configuration;
    builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
    builder.Services.AddSingleton<IJwtTokenGenerator,JwtTokenGenerator>();
    builder.Services.AddSingleton<IDateTimeProvider,DateTimeProvider>();
    builder.Services.AddSingleton<PostgresConnectionFactory>();

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    var connectionString = (new PostgresConnectionFactory()).CreateConnection().ConnectionString;

    builder.Services.AddScoped<IUserRepository>(sp => new UserRepository(connectionString));
    builder.Services.AddScoped<ITokenRepository>(sp => new TokenRepository(connectionString));

    builder.Services.AddControllers();

    var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        builder.Services.AddAuthentication(options =>
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
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}
```