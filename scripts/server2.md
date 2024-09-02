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