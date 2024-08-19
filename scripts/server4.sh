mkdir -p server/Api/Middleware
mkdir -p server/Application/Commands/CreatePost
mkdir -p server/Application/Commands/RegisterUser
mkdir -p server/Application/Commands/UpdatePost
mkdir -p server/Application/Commands/DeletePost
mkdir -p server/Application/Commands/SetFriend
mkdir -p server/Application/Queries/GetPost
mkdir -p server/Application/Services
mkdir -p server/Application/Validators
mkdir -p server/Domain/ValueObjects





touch server/Infrastructure/Configurations/ServiceCollectionExtensions.cs

touch server/Api/Controllers/DialogController.cs
touch server/Application/DTOs/DialogMessageDto.cs
touch server/Domain/Entities/DialogMessage.cs



dotnet add server/Api/ package StackExchange.Redis
dotnet add server/Api/ package RabbitMQ.Client
dotnet add server/Api/ package MassTransit
