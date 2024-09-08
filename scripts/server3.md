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