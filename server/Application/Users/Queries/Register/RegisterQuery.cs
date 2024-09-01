using Application.Users.DTO;
using MediatR;

namespace Application.Users.Queries.Register;

public record RegisterQuery(string FirstName, string SecondName, string Birthdate, string Biography, string City, string Password) : IRequest<UserDTO>;