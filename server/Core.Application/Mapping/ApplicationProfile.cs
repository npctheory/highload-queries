using Core.Application.Users.DTO;
using Core.Application.Users.Queries.GetUser;
using Core.Application.Users.Queries.SearchUsers;
using AutoMapper;
using Core.Domain.Entities;

namespace Core.Application.Mapping
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<User, UserDTO>();
        }
    }
}