```bash
mkdir -p server/Core.Application/Posts/DTO/
mkdir -p server/Core.Application/Posts/Queries/ListPosts/
mkdir -p server/Core.Application/Posts/Queries/ListFriendsPosts/
mkdir -p server/Core.Application/Posts/Commands/CreatePost/
mkdir -p server/Core.Application/Posts/Queries/GetPost/
mkdir -p server/Core.Application/Posts/Commands/UpdatePost/
mkdir -p server/Core.Application/Posts/Commands/DeletePost/
mkdir -p server/Core.Application/Posts/EventConsumers/

touch server/Core.Domain/Entities/Post.cs
touch server/Core.Domain/Interfaces/IPostRepository.cs

touch server/Core.Application/Posts/DTO/PostDTO.cs
touch server/Core.Application/Posts/Queries/ListPosts/ListPostsQuery.cs
touch server/Core.Application/Posts/Queries/ListPosts/ListPostsQueryHandler.cs
touch server/Core.Application/Posts/Queries/ListFriendsPosts/ListFriendsPostsQuery.cs
touch server/Core.Application/Posts/Queries/ListFriendsPosts/ListFriendsPostsQueryHandler.cs
touch server/Core.Application/Posts/Queries/GetPost/GetPostQuery.cs
touch server/Core.Application/Posts/Queries/GetPost/GetPostQueryHandler.cs

touch server/Core.Application/Posts/Commands/CreatePost/CreatePostCommand.cs
touch server/Core.Application/Posts/Commands/CreatePost/CreatePostCommandHandler.cs
touch server/Core.Application/Posts/Commands/UpdatePost/UpdatePostCommand.cs
touch server/Core.Application/Posts/Commands/UpdatePost/UpdatePostCommandHandler.cs
touch server/Core.Application/Posts/Commands/DeletePost/DeletePostCommand.cs
touch server/Core.Application/Posts/Commands/DeletePost/DeletePostCommandHandler.cs

touch server/Core.Infrastructure/Snapshots/PostSnapshot.cs
touch server/Core.Infrastructure/Repositories/PostRepository.cs

touch server/Core.Api/Controllers/PostController.cs
```
**PostCreatedEvent.cs**
```csharp
using MediatR;

namespace EventBus.Events;

public class PostCreatedEvent : INotification
{
    public string UserId { get; set; }
    public string PostId { get; set; }
    public string Text {get; set;}


    public PostCreatedEvent(string userId, string postId, string text)
    {
        UserId = userId;
        PostId = postId;
        Text = text;
    }
}
```

**PostDeletedEvent.cs**
```csharp
using MediatR;

namespace EventBus.Events;

public class PostDeletedEvent : INotification
{
    public string UserId { get; set; }
    public string PostId { get; set; }


    public PostDeletedEvent(string userId, string postId)
    {
        UserId = userId;
        PostId = postId;
    }
}
```

**PostUpdatedEvent.cs**
```csharp
using MediatR;

namespace EventBus.Events;

public class PostUpdatedEvent : INotification
{
    public string UserId { get; set; }
    public Guid PostId { get; set; }
    public string Text {get; set;}


    public PostUpdatedEvent(string userId, Guid postId, string text)
    {
        UserId = userId;
        PostId = postId;
        Text = text;
    }
}
```

**Post.cs**
```csharp
using System;

namespace Core.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**IPostRepository.cs**
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Domain.Interfaces;
public interface IPostRepository
{
    Task<List<Post>> ListPostsByUserId(string userId);
    Task<List<Post>> ListPostsByUserIds(List<string> userIds, int offset, int limit);
    Task<Post> GetPostById(Guid postId);
    Task<Post> DeletePost(string userId, Guid postId);
    Task<Post> CreatePost(string userId, string text);
    Task<Post> UpdatePost(string userId, Guid postId, string text);
}
```

**PostDTO.cs**
```csharp
using System;

namespace Core.Application.Posts.DTO;
public class PostDTO
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**CreatePostCommand.cs**
```csharp
using Core.Application.Posts.DTO;
using MediatR;
using System;

namespace Core.Application.Posts.Commands.CreatePost;

public record CreatePostCommand(string UserId, string Text) : IRequest<PostDTO>;
```

**CreatePostCommandHandler.cs**
```csharp
using AutoMapper;
using Core.Application.Posts.DTO;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Application.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDTO>
{
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;

        public CreatePostCommandHandler(
            IPostRepository postRepository, 
            IMapper mapper, 
            IEventBus eventBus)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _eventBus = eventBus;
        }

    public async Task<PostDTO> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.CreatePost(request.UserId, request.Text);
        var postCreatedEvent = new PostCreatedEvent(post.UserId, post.Id.ToString(), post.Text);
            await _eventBus.PublishAsync(postCreatedEvent, cancellationToken);
        return _mapper.Map<PostDTO>(post);
    }
}
```

**DeletePostCommand.cs**
```csharp
using Core.Application.Posts.DTO;
using MediatR;

namespace Core.Application.Posts.Commands.DeletePost;

public record DeletePostCommand(string userId, string postId) : IRequest<PostDTO>;
```

**DeletePostCommandHandler.cs**
```csharp
using AutoMapper;
using Core.Application.Posts.DTO;
using Core.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using MediatR;

namespace Core.Application.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, PostDTO>
{
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;

        public DeletePostCommandHandler(
            IPostRepository postRepository, 
            IMapper mapper, 
            IEventBus eventBus)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _eventBus = eventBus;
        }

    public async Task<PostDTO> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var deleted = _mapper.Map<PostDTO>(await _postRepository.DeletePost(request.userId, Guid.Parse(request.postId)));
        var postDeleteEvent = new PostDeletedEvent(request.userId, request.postId);
        await _eventBus.PublishAsync(postDeleteEvent, cancellationToken);
        return deleted;
    }
}
```

**UpdatePostCommand.cs**
```csharp
using Core.Application.Posts.DTO;
using MediatR;
using System;

namespace Core.Application.Posts.Commands.UpdatePost;

public record UpdatePostCommand(string UserId, string PostId, string Text) : IRequest<PostDTO>;
```

**UpdatePostCommandHandler.cs**
```csharp
using AutoMapper;
using Core.Application.Posts.DTO;
using Core.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Application.Posts.Commands.UpdatePost
{
    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDTO>
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;

        public UpdatePostCommandHandler(
            IPostRepository postRepository, 
            IMapper mapper, 
            IEventBus eventBus)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _eventBus = eventBus;
        }

        public async Task<PostDTO> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
        var post = await _postRepository.UpdatePost(request.UserId, Guid.Parse(request.PostId), request.Text);
        var postUpdatedEvent = new PostUpdatedEvent(post.UserId, post.Id, post.Text);
            await _eventBus.PublishAsync(postUpdatedEvent, cancellationToken);
        return _mapper.Map<PostDTO>(post);
        }
    }
}
```

**GetPostQuery.cs**
```csharp
using Core.Application.Posts.DTO;
using MediatR;

namespace Core.Application.Posts.Queries.GetPost;

public record GetPostQuery(string postId) : IRequest<PostDTO>;
```

**GetPostQueryHandler.cs**
```csharp
using AutoMapper;
using Core.Application.Posts.DTO;
using Core.Domain.Interfaces;
using MediatR;

namespace Core.Application.Posts.Queries.GetPost;

public class GetPostQueryHandler : IRequestHandler<GetPostQuery, PostDTO>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetPostQueryHandler(IPostRepository postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<PostDTO> Handle(GetPostQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostById(Guid.Parse(request.postId));
        return _mapper.Map<PostDTO>(post);
    }
}
```

**ListPostsQuery.cs**
```csharp
using Core.Application.Posts.DTO;
using MediatR;

namespace Core.Application.Posts.Queries.ListPosts;

public record ListPostsQuery(string userId) : IRequest<List<PostDTO>>;
```

**ListPostsQueryHandler.cs**
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Posts.DTO;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MediatR;

namespace Core.Application.Posts.Queries.ListPosts;
public class ListPostsQueryHandler : IRequestHandler<ListPostsQuery, List<PostDTO>>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public ListPostsQueryHandler(IPostRepository postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<List<PostDTO>> Handle(ListPostsQuery request, CancellationToken cancellationToken)
    {
        List<Post> posts = await _postRepository.ListPostsByUserId(request.userId);
        return _mapper.Map<List<PostDTO>>(posts);
    }
}
```


**GetPostFeedQuery.cs**
```csharp
using Core.Application.Posts.DTO;
using MediatR;

namespace Core.Application.Posts.Queries.GetPostFeed;

public record GetPostFeedQuery(string userId, int offset, int limit) : IRequest<List<PostDTO>>;
```

**GetPostFeedQueryHandler.cs**
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Abstractions;
using Core.Application.Posts.DTO;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using EventBus;
using MediatR;

namespace Core.Application.Posts.Queries.GetPostFeed;

public class GetPostFeedQueryHandler : IRequestHandler<GetPostFeedQuery, List<PostDTO>>
{
    private readonly IPostRepository _postRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _postFeedTtl;


    public GetPostFeedQueryHandler(IPostRepository postRepository, IFriendshipRepository friendshipRepository, IMapper mapper, ICacheService cacheService)
    {
        _postRepository = postRepository;
        _friendshipRepository = friendshipRepository;
        _mapper = mapper;
        _cacheService = cacheService;
        _postFeedTtl = TimeSpan.FromMinutes(5);
    }

    public async Task<List<PostDTO>> Handle(GetPostFeedQuery request, CancellationToken cancellationToken)
    {
        string cacheKeyForFirst1000 = $"FriendsPosts:{request.userId}:0:1000";
        List<Post> posts;


        List<Post> cachedFirst1000Posts = await _cacheService.GetAsync<List<Post>>(cacheKeyForFirst1000);


        if (cachedFirst1000Posts == null || cachedFirst1000Posts.Count == 0)
        {
            List<Friendship> friendships = await _friendshipRepository.ListFriendships(request.userId);
            List<string> friendsIds = friendships.Select(f => f.FriendId).ToList();


            cachedFirst1000Posts = await _postRepository.ListPostsByUserIds(friendsIds, 0, 1000);

            if (cachedFirst1000Posts.Count > 0)
            {
                await _cacheService.SetAsync(cacheKeyForFirst1000, cachedFirst1000Posts, _postFeedTtl);
            }
        }

        if ((request.offset + request.limit) <= 1000)
        {
            posts = cachedFirst1000Posts.Skip(request.offset).Take(request.limit).ToList();
        }
        else
        {
            string dynamicCacheKey = $"FriendsPosts:{request.userId}:{request.offset}:{request.limit}";

            List<Post> cachedPosts = await _cacheService.GetAsync<List<Post>>(dynamicCacheKey);

            if (cachedPosts != null && cachedPosts.Count > 0)
            {
                posts = cachedPosts;
            }
            else
            {
                List<Friendship> friendships = await _friendshipRepository.ListFriendships(request.userId);
                List<string> friendsIds = friendships.Select(f => f.FriendId).ToList();
                posts = await _postRepository.ListPostsByUserIds(friendsIds, request.offset, request.limit);

                if (posts.Count > 0)
                {
                    await _cacheService.SetAsync(dynamicCacheKey, posts, _postFeedTtl);
                }
            }
        }
        return _mapper.Map<List<PostDTO>>(posts);
    }
}
```

**PostFeedCacheBuilder.cs**
```csharp
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain.Interfaces;
using EventBus.Events;
using Core.Application.Abstractions;
using Core.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Core.Application.Posts.Queries.GetPostFeed;

public class PostFeedCacheBuilder :
    INotificationHandler<UserLoggedInEvent>,
    INotificationHandler<FriendAddedEvent>,
    INotificationHandler<FriendDeletedEvent>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly ICacheService _cacheService;
    private readonly IPostRepository _postRepository;
    private readonly TimeSpan _postFeedTtl;

    public PostFeedCacheBuilder(
        IFriendshipRepository friendshipRepository,
        ICacheService cacheService,
        IPostRepository postRepository)
    {
        _friendshipRepository = friendshipRepository;
        _cacheService = cacheService;
        _postRepository = postRepository;
        _postFeedTtl = TimeSpan.FromMinutes(5);
    }

    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        var userId = notification.UserId;
        await BuildFriendsPostsCacheForUser(userId);
    }

    public async Task Handle(FriendAddedEvent notification, CancellationToken cancellationToken)
    {
        var userId = notification.UserId;
        await BuildFriendsPostsCacheForUser(userId);
    }

    public async Task Handle(FriendDeletedEvent notification, CancellationToken cancellationToken)
    {
        var userId = notification.UserId;
        await BuildFriendsPostsCacheForUser(userId);
    }

    private async Task BuildFriendsPostsCacheForUser(string userId)
    {
        string cacheKeyForFirst1000 = $"FriendsPosts:{userId}:0:1000";
        List<Friendship> friendships = await _friendshipRepository.ListFriendships(userId);

        List<string> friendsIds = friendships.Select(f => f.FriendId).ToList();
        List<Post> cachedFirst1000Posts = await _postRepository.ListPostsByUserIds(friendsIds, 0, 1000);
        if (cachedFirst1000Posts.Count > 0)
        {
            await _cacheService.SetAsync(cacheKeyForFirst1000, cachedFirst1000Posts, _postFeedTtl);
        }
    }
}
```

**FriendsPostFeedCacheRebuilder.cs**
```csharp
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain.Interfaces;
using EventBus.Events;
using Core.Application.Abstractions;
using Core.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Core.Application.Posts.Queries.GetPostFeed;

public class FriendsPostFeedCacheRebuilder :
    INotificationHandler<PostDeletedEvent>,
    INotificationHandler<PostCreatedEvent>,
    INotificationHandler<PostUpdatedEvent>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly ICacheService _cacheService;
    private readonly IPostRepository _postRepository;
    private readonly TimeSpan _postFeedTtl;

    public FriendsPostFeedCacheRebuilder(
        IFriendshipRepository friendshipRepository,
        ICacheService cacheService,
        IPostRepository postRepository)
    {
        _friendshipRepository = friendshipRepository;
        _cacheService = cacheService;
        _postRepository = postRepository;
        _postFeedTtl = TimeSpan.FromMinutes(5);
    }

    public async Task Handle(PostDeletedEvent notification, CancellationToken cancellationToken)
    {
        var userId = notification.UserId;
        await RebuildFriendsPostsCacheForFriends(userId);
    }

    public async Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
    {
        var userId = notification.UserId;
        await RebuildFriendsPostsCacheForFriends(userId);
    }

    public async Task Handle(PostUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var userId = notification.UserId;
        await RebuildFriendsPostsCacheForFriends(userId);
    }

    private async Task RebuildFriendsPostsCacheForFriends(string userId)
    {
        List<Friendship> users = await _friendshipRepository.ListUsersWithFriend(userId);

        foreach (var friendship in users)
        {
            string cacheKeyForFirst1000 = $"FriendsPosts:{friendship.UserId}:0:1000";

            var cachedPosts = await _cacheService.GetAsync<List<Post>>(cacheKeyForFirst1000);
            if (cachedPosts != null && cachedPosts.Count > 0)
            {
                List<Friendship> friends = await _friendshipRepository.ListFriendships(friendship.UserId);
                List<string> friendIds = friends.Select(f => f.FriendId).ToList();

                List<Post> postsToCache = await _postRepository.ListPostsByUserIds(friendIds, 0, 1000);
                if (postsToCache.Count > 0)
                {
                    await _cacheService.SetAsync(cacheKeyForFirst1000, postsToCache, _postFeedTtl);
                }
            }
        }
    }
}
```

**PostSnapshot.cs**
```csharp
using System;

namespace Core.Infrastructure.Snapshots;
public class PostSnapshot
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**PostRepository.cs**
```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Infrastructure.Snapshots;
using Npgsql;

namespace Core.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly string _connectionString;
    private readonly IMapper _mapper;

    public PostRepository(string connectionString, IMapper mapper)
    {
        _connectionString = connectionString;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<Post>> ListPostsByUserId(string userId)
    {
        var posts = new List<Post>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new NpgsqlCommand("SELECT * FROM posts WHERE user_id = @userId ORDER BY created_at DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var postSnapshot = new PostSnapshot
                    {
                        Id = reader.GetGuid(0),
                        Text = reader.GetString(1),
                        UserId = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    };
                    posts.Add(_mapper.Map<Post>(postSnapshot));
                }
            }
        }
        return posts;
    }

    public async Task<List<Post>> ListPostsByUserIds(List<string> userIds, int offset, int limit)
    {
        var posts = new List<Post>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                "SELECT * FROM posts WHERE user_id = ANY(@users) ORDER BY created_at DESC OFFSET @offset LIMIT @limit",
                connection);

            command.Parameters.AddWithValue("@users", userIds);
            command.Parameters.AddWithValue("@offset", offset);
            command.Parameters.AddWithValue("@limit", limit);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var post = new PostSnapshot
                    {
                        Id = reader.GetGuid(0),
                        Text = reader.GetString(1),
                        UserId = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    };

                    posts.Add(_mapper.Map<Post>(post));
                }
            }
        }
        return posts;
    }

    public async Task<Post> GetPostById(Guid postId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new NpgsqlCommand("SELECT * FROM posts WHERE id = @postId", connection);
            command.Parameters.AddWithValue("@postId", postId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var postSnapshot = new PostSnapshot
                    {
                        Id = reader.GetGuid(0),
                        Text = reader.GetString(1),
                        UserId = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    };

                    return _mapper.Map<Post>(postSnapshot);
                }
            }
        }
        return null;
    }

    public async Task<Post> CreatePost(string userId, string text)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                "INSERT INTO posts (text, user_id) VALUES (@text, @userId) RETURNING id, text, user_id, created_at", connection);

            command.Parameters.AddWithValue("@text", text);
            command.Parameters.AddWithValue("@userId", userId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Post
                    {
                        Id = reader.GetGuid(0),
                        Text = reader.GetString(1),
                        UserId = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    };
                }
            }
        }
        return null;
    }

    public async Task<Post> UpdatePost(string userId, Guid postId, string text)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                "UPDATE posts SET text = @text WHERE id = @postId AND user_id = @userId RETURNING id, text, user_id, created_at", connection);

            command.Parameters.AddWithValue("@text", text);
            command.Parameters.AddWithValue("@postId", postId);
            command.Parameters.AddWithValue("@userId", userId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Post
                    {
                        Id = reader.GetGuid(0),
                        Text = reader.GetString(1),
                        UserId = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    };
                }
            }
        }
        return null;
    }

    public async Task<Post> DeletePost(string userId, Guid postId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var post = await GetPostById(postId);

            if (post == null || post.UserId != userId)
                return null;

            var command = new NpgsqlCommand(
                "DELETE FROM posts WHERE id = @postId AND user_id = @userId", connection);

            command.Parameters.AddWithValue("@postId", postId);
            command.Parameters.AddWithValue("@userId", userId);

            var affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows > 0 ? post : null;
        }
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
            CreateMap<PostSnapshot, Post>();
        }
    }
}
```

**Program.cs**
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Core.Application;
using Core.Infrastructure;
using Core.Api;
using Core.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PostHub>("/post/feed/posted");

app.Run();
```

**PostController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using MediatR;
using System.Security.Claims;
using Core.Application.Posts.Queries.ListPosts;
using Core.Application.Posts.Queries.GetPostFeed;
using Core.Application.Posts.Queries.GetPost;
using Core.Application.Posts.Commands.DeletePost;
using Core.Application.Posts.DTO;
using Core.Application.Posts.Commands.UpdatePost;
using Core.Application.Posts.Commands.CreatePost;

namespace Api.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ISender _mediator;

        public PostController(ISender mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("post/list")]
        public async Task<IActionResult> ListPosts()
        {
            var user_id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                var posts = await _mediator.Send(new ListPostsQuery(user_id));
                return Ok(posts);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("post/feed")]
        public async Task<IActionResult> ListFriendPosts([FromQuery] int offset, [FromQuery] int limit)
        {
            var user_id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                var posts = await _mediator.Send(new GetPostFeedQuery(user_id, offset, limit));
                return Ok(posts);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("post/get/{id}")]
        public async Task<IActionResult> GetPostById([FromRoute] string id)
        {
           
            try
            {
                var posts = await _mediator.Send(new GetPostQuery(id));
                return Ok(posts);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPut("post/delete/{post_id}")]
        public async Task<IActionResult> DeletePost([FromRoute] string post_id)
        {
            var user_id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           
            try
            {
                var result = await _mediator.Send(new DeletePostCommand(user_id, post_id));
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

         [Authorize]
        [HttpPost("post/create")]
        public async Task<IActionResult> CreatePost([FromBody] JsonElement jsonElement)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string text = jsonElement.GetProperty("text").GetString();
            try
            {
                PostDTO post = await _mediator.Send(new CreatePostCommand(userId, text));
                return Ok(post);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPut("post/update")]
        public async Task<IActionResult> UpdatePost([FromBody] JsonElement jsonElement)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string text = jsonElement.GetProperty("text").GetString();
            string id = jsonElement.GetProperty("id").GetString();

            try
            {
                var updated = await _mediator.Send(new UpdatePostCommand(userId, id, text));
                if (updated != null)
                {
                    return NoContent();
                }
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}
```

**FriendEventConsumer.cs**
```csharp
using MassTransit;
using MediatR;
using EventBus.Events;
using System.Threading.Tasks;

namespace Core.Api.EventConsumers;

public class FriendEventConsumer : 
    IConsumer<FriendAddedEvent>,
    IConsumer<FriendDeletedEvent>
{
    private readonly IMediator _mediator;

    public FriendEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<FriendAddedEvent> context)
    {
        await _mediator.Publish(context.Message);
    }

    public async Task Consume(ConsumeContext<FriendDeletedEvent> context)
    {
        await _mediator.Publish(context.Message);
    }
}
```

**PostEventConsumer.cs**
```csharp
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using EventBus.Events;
using System.Threading.Tasks;
using Core.Api.Hubs;
using MediatR;

namespace Core.Api.EventConsumers
{
    public class PostEventConsumer :
        IConsumer<PostDeletedEvent>,
        IConsumer<PostCreatedEvent>,
        IConsumer<PostUpdatedEvent>
    {
        private readonly IHubContext<PostHub> _hubContext;
        private readonly IMediator _mediator;

        public PostEventConsumer(IHubContext<PostHub> hubContext, IMediator mediator)
        {
            _hubContext = hubContext;
            _mediator = mediator;
            
        }

        public async Task Consume(ConsumeContext<PostDeletedEvent> context)
        {
            var postDeletedEvent = context.Message;
            string group = postDeletedEvent.UserId;
            await _hubContext.Clients.Group(group)
                                      .SendAsync("PostDeletedEvent", postDeletedEvent);
        }

        public async Task Consume(ConsumeContext<PostCreatedEvent> context)
        {
            var postcreatedEvent = context.Message;
            string group = postcreatedEvent.UserId;
            await _mediator.Publish(postcreatedEvent);
            await _hubContext.Clients.Group($"{postcreatedEvent.UserId}")
                                      .SendAsync("PostCreatedEvent", postcreatedEvent);
        }

        public async Task Consume(ConsumeContext<PostUpdatedEvent> context)
        {
            var postUpdatedEvent = context.Message;
            string group = postUpdatedEvent.UserId;
            await _hubContext.Clients.Group($"{postUpdatedEvent.UserId}")
                                      .SendAsync("PostUpdatedEvent", postUpdatedEvent);
        }
    }
}
```

**UserEventConsumer.cs**
```csharp
using MassTransit;
using MediatR;
using EventBus.Events;
using System.Threading.Tasks;

namespace Core.Api.EventConsumers;

public class UserEventConsumer : 
    IConsumer<UserLoggedInEvent>
{
    private readonly IMediator _mediator;

    public UserEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        await _mediator.Publish(context.Message);
    }
}
```

**PostHub.cs**
```csharp
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Core.Application.Friends.DTO;
using Core.Application.Friends.Queries.ListFriends;
using System.Security.Claims;
using MediatR;
using EventBus.Events;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Core.Api.Hubs
{
    [Authorize]
    public class PostHub : Hub
    {
        private readonly IMediator _mediator;

        public PostHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await base.OnConnectedAsync();
                return;
            }

            List<FriendDTO> friends = await _mediator.Send(new ListFriendsQuery(userId));
            var groupNames = new List<string>();

            foreach (var friend in friends)
            {
                var groupName = friend.FriendId.ToString();
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                groupNames.Add(groupName);
            }

            var formattedGroupNames = string.Join(", ", groupNames);

            await Clients.Caller.SendAsync("ReceiveGroupInfo", formattedGroupNames);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            List<FriendDTO> friends = await _mediator.Send(new ListFriendsQuery(userId));
            var groupNames = new List<string>();

            foreach (var friend in friends)
            {
                var groupName = friend.FriendId.ToString();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                groupNames.Add(groupName);
            }

            var formattedGroupNames = string.Join(", ", groupNames);

            await Clients.Caller.SendAsync("Добавлен к группам:", formattedGroupNames);

            await base.OnDisconnectedAsync(exception);
        }
    }
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
using Core.Application.Posts.DTO;

namespace Core.Application.Mapping
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<Friendship, FriendDTO>();
            CreateMap<Post, PostDTO>();
        }
    }
}
```