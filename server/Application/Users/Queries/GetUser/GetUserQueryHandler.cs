using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Users.Queries.GetUser;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using MediatR;
using Application.Users.DTO;

namespace Application.Users.Queries.GetUser
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDTO> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            User user = await _userRepository.GetUserByIdAsync(request.Id);
            return _mapper.Map<UserDTO>(user);
        }
    }
}