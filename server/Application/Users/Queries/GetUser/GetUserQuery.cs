using Application.Users.DTO;
using Domain.Entities;
using MediatR;

namespace Application.Users.Queries.GetUser;

public record GetUserQuery(string Id) : IRequest<UserDTO>;