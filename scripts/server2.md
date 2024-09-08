```bash
mkdir -p server/Core.Application/Friends/DTO/
mkdir -p server/Core.Application/Friends/Queries/ListFriends
mkdir -p server/Core.Application/Friends/Commands/AddFriend
mkdir -p server/Core.Application/Friends/Commands/DeleteFriend



touch server/Core.Domain/Entities/Friendship.cs
touch server/Core.Domain/Interfaces/IFriendshipRepository.cs


touch server/Core.Application/Friends/DTO/FriendDTO.cs
touch server/Core.Application/Friends/Queries/ListFriends/ListFriendsQuery.cs
touch server/Core.Application/Friends/Queries/ListFriends/ListFriendsQueryHandler.cs

touch server/Core.Application/Friends/Commands/AddFriend/AddFriendQuery.cs
touch server/Core.Application/Friends/Commands/AddFriend/AddFriendQueryHandler.cs

touch server/Core.Application/Friends/Commands/DeleteFriend/DeleteFriendQuery.cs
touch server/Core.Application/Friends/Commands/DeleteFriend/DeleteFriendQueryHandler.cs

touch server/Core.Infrastructure/Snapshots/FriendshipSnapshot.cs
touch server/Core.Infrastructure/Repositories/FriendshipRepository.cs


touch server/Core.Api/Controllers/FriendController.cs
```

**FriendAddedEvent.cs**
```csharp
using MediatR;

namespace EventBus.Events;

public class FriendAddedEvent : INotification
{
    public string UserId { get; set; }
    public string FriendId { get; set; }


    public FriendAddedEvent(string userId, string friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}
```

**FriendDeletedEvent.cs**
```csharp
using MediatR;

namespace EventBus.Events;

public class FriendDeletedEvent : INotification
{
    public string UserId { get; set; }
    public string FriendId { get; set; }


    public FriendDeletedEvent(string userId, string friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}
```

**Friendship.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Entities;

public class Friendship
{
    public string UserId { get; set; }
    public string FriendId { get; set; }
}
```

**IFriendshipRepository.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IFriendshipRepository
{
    Task AddFriendship(string userId, string friendId);
    Task DeleteFriendship(string userId, string friendId);
    Task<List<Friendship>> ListFriendships(string userId);
    Task<List<Friendship>> ListUsersWithFriend(string friendId);
}
```

**ApplicationProfile.cs**
```csharp
using Core.Application.Users.DTO;
using Core.Application.Users.Queries.GetUser;
using Core.Application.Users.Queries.SearchUsers;
using AutoMapper;
using Core.Domain.Entities;
using Core.Application.Friends.DTO;

namespace Core.Application.Mapping
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<Friendship, FriendDTO>();
        }
    }
}
```

**FriendDTO.cs**
```csharp
namespace Core.Application.Friends.DTO;

public class FriendDTO
{
    public string UserId { get; set; }
    public string FriendId { get; set; }
}
```

**ListFriendsQuery.cs**
```csharp
using MediatR;
using Core.Application.Friends.DTO;

namespace Core.Application.Friends.Queries.ListFriends;

public record ListFriendsQuery(string userId) : IRequest<List<FriendDTO>>;
```

**ListFriendsQueryHandler.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Core.Application.Friends.DTO;
using Core.Domain.Entities;
using Core.Domain.Interfaces;


namespace Core.Application.Friends.Queries.ListFriends;

public class ListFriendsQueryHandler : IRequestHandler<ListFriendsQuery, List<FriendDTO>>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IMapper _mapper;

    public ListFriendsQueryHandler(IFriendshipRepository friendshipRepository, IMapper mapper)
    {
        _friendshipRepository = friendshipRepository;
        _mapper = mapper;
    }

    public async Task<List<FriendDTO>> Handle(ListFriendsQuery request, CancellationToken cancellationToken)
    {
        List<Friendship> friends = await _friendshipRepository.ListFriendships(request.userId);
        return _mapper.Map<List<FriendDTO>>(friends);
    }
}
```

**AddFriendQuery.cs**
```csharp
using MediatR;

namespace Core.Application.Friends.Commands.AddFriend;

public record AddFriendQuery(string UserId, string FriendId) : IRequest<bool>;
```

**AddFriendQueryHandler.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Core.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using MediatR;


namespace Core.Application.Friends.Commands.AddFriend;

public class AddFriendQueryHandler : IRequestHandler<AddFriendQuery, bool>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public AddFriendQueryHandler(
        IFriendshipRepository friendshipRepository, 
        IMapper mapper, 
        IEventBus eventBus)
    {
        _friendshipRepository = friendshipRepository;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async Task<bool> Handle(AddFriendQuery request, CancellationToken cancellationToken)
    {
        await _friendshipRepository.AddFriendship(request.UserId, request.FriendId);
        var friendAddEvent = new FriendAddedEvent(request.UserId,request.FriendId);
        await _eventBus.PublishAsync(friendAddEvent, cancellationToken);
        return true;
    }
}
```

**DeleteFriendQuery.cs**
```csharp
using MediatR;

namespace Core.Application.Friends.Commands.DeleteFriend;

public record DeleteFriendQuery(string UserId, string FriendId) : IRequest<bool>;
```

**DeleteFriendQueryHandler.cs**
```csharp
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Core.Domain.Interfaces;
using EventBus;
using EventBus.Events;

namespace Core.Application.Friends.Commands.DeleteFriend
{
    public class DeleteFriendQueryHandler : IRequestHandler<DeleteFriendQuery, bool>
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;

        public DeleteFriendQueryHandler(
            IFriendshipRepository friendshipRepository, 
            IMapper mapper, 
            IEventBus eventBus)
        {
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(DeleteFriendQuery request, CancellationToken cancellationToken)
        {
            await _friendshipRepository.DeleteFriendship(request.UserId, request.FriendId);
            var friendDeleteEvent = new FriendDeletedEvent(request.UserId,request.FriendId);
            await _eventBus.PublishAsync(friendDeleteEvent, cancellationToken);
            return true;
        }
    }
}
```

**FriendAddEvent.cs**
```csharp
namespace EventBus.Events;

public class FriendAddEvent
{
    public string UserId { get; set; }
    public string FriendId { get; set; }


    public FriendAddEvent(string userId, string friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}
```

**FriendDeleteEvent.cs**
```csharp
namespace EventBus.Events;

public class FriendDeleteEvent
{
    public string UserId { get; set; }
    public string FriendId { get; set; }


    public FriendDeleteEvent(string userId, string friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}
```

**InfrastructureProfile.cs**
```csharp
using AutoMapper;
using Core.Domain.Entities;
using Core.Infrastructure.Snapshots;

namespace Core.Infrastructure.Mapping
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<UserSnapshot, User>();
            CreateMap<FriendshipSnapshot, Friendship>();
        }
    }
}
```

**FriendshipSnapshot.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Snapshots;

public class FriendshipSnapshot
{
    public string UserId { get; set; }
    public string FriendId { get; set; }
}
```

**FriendshipRepository.cs**
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Npgsql;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Infrastructure.Snapshots;

namespace Core.Infrastructure.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly string _connectionString;
    private readonly IMapper _mapper;

    public FriendshipRepository(string connectionString, IMapper mapper)
    {
        _connectionString = connectionString;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task AddFriendship(string userId, string friendId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("INSERT INTO friendships (user_id, friend_id) VALUES (@UserId, @FriendId)", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@FriendId", friendId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteFriendship(string userId, string friendId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("DELETE FROM friendships WHERE user_id = @UserId AND friend_id = @FriendId", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@FriendId", friendId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<List<Friendship>> ListFriendships(string userId)
    {
        var friendships = new List<Friendship>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("SELECT friend_id FROM friendships WHERE user_id = @UserId", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var friendshipSnapshot = new FriendshipSnapshot
                        {
                            UserId = userId,
                            FriendId = reader.GetString(0)
                        };

                        friendships.Add(_mapper.Map<Friendship>(friendshipSnapshot));
                    }
                }
            }
        }

        return friendships;
    }

    public async Task<List<Friendship>> ListUsersWithFriend(string friendId)
    {
        var friendships = new List<Friendship>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("SELECT user_id FROM friendships WHERE friend_id = @FriendId", connection))
            {
                command.Parameters.AddWithValue("@FriendId", friendId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        friendships.Add(new Friendship
                        {
                            UserId = reader.GetString(0),
                            FriendId = friendId
                        });
                    }
                }
            }
        }

        return friendships;
    }
}
```

**Infrastructure/DependencyInjection.cs**
```csharp
using Core.Domain.Interfaces;
using Core.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Core.Infrastructure.Mapping;
using StackExchange.Redis;
using Core.Application.Abstractions;
using Core.Application.Services;
using Core.Infrastructure.Services;
using MassTransit;
using Core.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Core.Infrastructure.Generators;
using System.Text;
using Core.Infrastructure.Providers;
using EventBus;

namespace Core.Infrastructure
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

            services.AddScoped<IFriendshipRepository>(sp =>
            {
                var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;
                var mapper = sp.GetRequiredService<IMapper>();
                return new FriendshipRepository(connectionString, mapper);
            });


            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

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
            services.AddTransient<IEventBus,RabbitMQEventBus>();

            return services;
        }
    }
}
```

**FriendController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using MediatR;
using System.Security.Claims;
using Core.Application.Friends.Queries.ListFriends;
using Core.Application.Friends.Commands.AddFriend;
using Core.Application.Friends.Commands.DeleteFriend;

namespace Core.Api.Controllers;

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
            await _mediator.Send(new AddFriendQuery(user_id, friend_id));
            return Ok(await _mediator.Send(new ListFriendsQuery(user_id)));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}
```