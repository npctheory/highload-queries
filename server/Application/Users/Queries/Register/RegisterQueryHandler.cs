using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Users.DTO;
using Application.Abstractions;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Users.Queries.Register
{
    public class RegisterQueryHandler : IRequestHandler<RegisterQuery, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterQueryHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IMapper mapper,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<UserDTO> Handle(RegisterQuery request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                Birthdate = DateTime.Parse(request.Birthdate),
                Biography = request.Biography,
                City = request.City,
                PasswordHash = _passwordHasher.HashPassword(request.Password)
            };

            await _userRepository.CreateUserAsync(user);

            return _mapper.Map<UserDTO>(user);
        }
    }
}
