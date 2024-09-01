using Application.Users.DTO;
using MediatR;

namespace Application.Users.Queries.Login;

public record LoginQuery(string Id, string Password) : IRequest<TokenDTO>;