```bash
mkdir -p server/Application/Friends/Queries/ListFriends
mkdir -p server/Application/Friends/Queries/SetFriend
mkdir -p server/Application/Friends/Queries/DeleteFriend


touch server/Domain/Entities/Friend.cs
touch server/Application/DAO/FriendDAO.cs
touch server/Application/DTO/FriendDTO.cs
touch server/Application/Interfaces/IFriendRepository.cs
touch server/Infrastructure/Repositories/FriendRepository.cs
touch server/Application/Users/Queries/Login/ListFriendsQuery.cs
touch server/Application/Users/Queries/Login/ListFriendsQueryHandler.cs
touch server/Application/Users/Queries/Login/SetFriendQuery.cs
touch server/Application/Users/Queries/Login/SetFriendQueryHandler.cs
touch server/Application/Users/Queries/Login/DeleteFriendQuery.cs
touch server/Application/Users/Queries/Login/DeleteFriendQueryHandler.cs
```

**Friend.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Friend
{
    public string UserId { get; set; }
    public string FriendId { get; set; }
}
```
**FriendDAO.cs**
```csharp
namespace Application.DAO;

public class FriendDAO
{
    public string UserId { get; set; }
    public string FriendId { get; set; }
}
```
**FriendDTO.cs**
```csharp
namespace Application.DTO;

public class FriendDTO
{
    public string user_id { get; set; }
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

            CreateMap<FriendDAO, Friend>();
            CreateMap<Friend, FriendDTO>()
                .ForMember(dest => dest.user_id, opt => opt.MapFrom(src => src.FriendId));;
        }
    }
}
```
**IFriendRepository.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DAO;

namespace Application.Interfaces;

public interface IFriendRepository
{
    Task AddAsync(string userId, string friendId);
    Task DeleteAsync(string userId, string friendId);
    Task<List<FriendDAO>> ListAsync(string userId);
}
```
**FriendRepository.cs**
```csharp
using Application.DAO;
using Application.Interfaces;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FriendRepository : IFriendRepository
{
    private readonly string _connectionString;

    public FriendRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(string userId, string friendId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("INSERT INTO friends (user_id, friend_id) VALUES (@UserId, @FriendId)", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@FriendId", friendId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteAsync(string userId, string friendId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("DELETE FROM friends WHERE user_id = @UserId AND friend_id = @FriendId", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@FriendId", friendId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<List<FriendDAO>> ListAsync(string userId)
    {
        var friends = new List<FriendDAO>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("SELECT friend_id FROM friends WHERE user_id = @UserId", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        friends.Add(new FriendDAO
                        {
                            UserId = userId,
                            FriendId = reader.GetString(0) // Assuming friend_id is a string
                        });
                    }
                }
            }
        }

        return friends;
    }
}
```
**ListFriendsQuery.cs**
```csharp
using Application.DTO;
using MediatR;

namespace Application.Friends.Queries.ListFriends;

public record ListFriendsQuery(string userId) : IRequest<List<FriendDTO>>;
```
**ListFriendsQueryHandler.cs**
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

namespace Application.Friends.Queries.ListFriends
{
    public class ListFriendsQueryHandler : IRequestHandler<ListFriendsQuery, List<FriendDTO>>
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IMapper _mapper;

        public ListFriendsQueryHandler(IFriendRepository friendRepository, IMapper mapper)
        {
            _friendRepository = friendRepository;
            _mapper = mapper;
        }

        public async Task<List<FriendDTO>> Handle(ListFriendsQuery request, CancellationToken cancellationToken)
        {
            List<FriendDAO> friendDAOs = await _friendRepository.ListAsync(request.userId);

            List<Friend> friends = _mapper.Map<List<Friend>>(friendDAOs);
            return _mapper.Map<List<FriendDTO>>(friends);
        }
    }
}
```
**DeleteFriendQuery.cs**
```csharp
using Application.DTO;
using MediatR;

namespace Application.Friends.Queries.DeleteFriend;

public record DeleteFriendQuery(string UserId, string FriendId) : IRequest<bool>;
```
**DeleteFriendQueryHandler.cs**
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

namespace Application.Friends.Queries.DeleteFriend
{
    public class DeleteFriendQueryHandler : IRequestHandler<DeleteFriendQuery, bool>
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IMapper _mapper;

        public DeleteFriendQueryHandler(IFriendRepository friendRepository, IMapper mapper)
        {
            _friendRepository = friendRepository;
            _mapper = mapper;
        }

        public async Task<bool> Handle(DeleteFriendQuery request, CancellationToken cancellationToken)
        {
            await _friendRepository.DeleteAsync(request.UserId, request.FriendId);
            return true;
        }
    }
}
```
**SetFriendQuery.cs**
```csharp
using Application.DTO;
using MediatR;

namespace Application.Friends.Queries.SetFriend;

public record SetFriendQuery(string UserId, string FriendId) : IRequest<bool>;
```
**SetFriendQueryHandler.cs**
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

namespace Application.Friends.Queries.SetFriend
{
    public class SetFriendQueryHandler : IRequestHandler<SetFriendQuery, bool>
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IMapper _mapper;

        public SetFriendQueryHandler(IFriendRepository friendRepository, IMapper mapper)
        {
            _friendRepository = friendRepository;
            _mapper = mapper;
        }

        public async Task<bool> Handle(SetFriendQuery request, CancellationToken cancellationToken)
        {
            await _friendRepository.AddAsync(request.UserId, request.FriendId);
            return true;
        }
    }
}
```
**FriendController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Application.DTO;
using Application.Users.Queries.GetUser;
using Application.Users.Queries.SearchUsers;
using Application.Users.Queries.Login;
using MediatR;
using Application.Friends.Queries.ListFriends;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Application.Friends.Queries.DeleteFriend;
using Application.Friends.Queries.SetFriend;

namespace Api.Controllers
{
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly ISender _mediator;

        public FriendController(ISender mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("friend/list")]
        public async Task<IActionResult> GetFriends()
        {
            var user_id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                var friends = await _mediator.Send(new ListFriendsQuery(user_id));
                return Ok(friends);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPut("friend/delete/{friend_id}")]
        public async Task<IActionResult> DeleteFriend([FromRoute] string friend_id)
        {
            var user_id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                await _mediator.Send(new DeleteFriendQuery(user_id, friend_id));
                return Ok(await _mediator.Send(new ListFriendsQuery(user_id)));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPut("friend/set/{friend_id}")]
        public async Task<IActionResult> SetFriend([FromRoute] string friend_id)
        {
            var user_id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                await _mediator.Send(new SetFriendQuery(user_id, friend_id));
                return Ok(await _mediator.Send(new ListFriendsQuery(user_id)));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
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
    builder.Services.AddScoped<IFriendRepository>(sp => new FriendRepository(connectionString));

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
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
```