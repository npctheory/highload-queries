```bash
mkdir -p server/Core.Application/Friends/DTO/
mkdir -p server/Core.Application/Friends/Queries/ListFriends
mkdir -p server/Core.Application/Friends/Commands/SetFriend
mkdir -p server/Core.Application/Friends/Commands/DeleteFriend
mkdir -p server/Core.Application/Posts/DTO/
mkdir -p server/Core.Application/Posts/Queries/ListPosts/
mkdir -p server/Core.Application/Posts/Queries/GetPostsFeed/
mkdir -p server/Core.Application/Posts/Commands/CreatePost/
mkdir -p server/Core.Application/Posts/Commands/ReadPost/
mkdir -p server/Core.Application/Posts/Commands/UpdatePost/
mkdir -p server/Core.Application/Posts/Commands/DeletePost/

touch server/Core.Domain/Entities/Friendship.cs
touch server/Core.Domain/Entities/Post.cs
touch server/Core.Domain/Interfaces/IFriendshipRepository.cs
touch server/Core.Domain/Interfaces/IPostRepository.cs

touch server/Core.Application/Friends/DTO/FriendDTO.cs
touch server/Core.Application/Friends/Queries/ListFriends/ListFriendsQuery.cs
touch server/Core.Application/Friends/Queries/ListFriends/ListFriendsQueryHandler.cs
touch server/Core.Application/Friends/Commands/SetFriend/SetFriendQuery.cs
touch server/Core.Application/Friends/Commands/SetFriend/SetFriendQueryHandler.cs
touch server/Core.Application/Friends/Commands/DeleteFriend/DeleteFriendQuery.cs
touch server/Core.Application/Friends/Commands/DeleteFriend/DeleteFriendQueryHandler.cs

touch server/Core.Application/Posts/DTO/PostDTO.cs
touch server/Core.Application/Posts/Queries/ListPosts/ListPostsQuery.cs
touch server/Core.Application/Posts/Queries/ListPosts/ListPostsQueryHandler.cs
touch server/Core.Application/Posts/Queries/GetPostsFeed/GetPostsFeedQuery.cs
touch server/Core.Application/Posts/Queries/GetPostsFeed/GetPostsFeedQueryHandler.cs
touch server/Core.Application/Posts/Commands/CreatePost/CreatePostCommand.cs
touch server/Core.Application/Posts/Commands/CreatePost/CreatePostCommandHandler.cs
touch server/Core.Application/Posts/Commands/ReadPost/ReadPostCommand.cs
touch server/Core.Application/Posts/Commands/ReadPost/ReadPostCommandHandler.cs
touch server/Core.Application/Posts/Commands/UpdatePost/UpdatePostCommand.cs
touch server/Core.Application/Posts/Commands/UpdatePost/UpdatePostCommandHandler.cs
touch server/Core.Application/Posts/Commands/DeletePost/DeletePostCommand.cs
touch server/Core.Application/Posts/Commands/DeletePost/DeletePostCommandHandler.cs

touch server/Core.Infrastructure/Snapshots/FriendshipSnapshot.cs
touch server/Core.Infrastructure/Snapshots/PostSnapshot.cs
touch server/Core.Infrastructure/Repositories/FriendshipRepository.cs
touch server/Core.Infrastructure/Repositories/PostRepository.cs

touch server/Core.Api/Controllers/FriendController.cs
touch server/Core.Api/Controllers/PostController.cs
```