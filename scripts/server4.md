dotnet new worker -o server/Worker
dotnet sln server/HighloadSocial.sln add server/Worker/

dotnet add server/Api/ package StackExchange.Redis
dotnet add server/Api/ package RabbitMQ.Client

dotnet add server/Worker package RabbitMQ.Client
dotnet add server/Worker package StackExchange.Redis

dotnet add server/Worker/ reference server/Application/
dotnet add server/Worker/ reference server/Infrastructure/