using Application.Users.DTO;
using Application.Users.Queries.GetUser;
using Application.Users.Queries.SearchUsers;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<User, UserDTO>();
        }
    }
}