using AutoMapper;
using Domain.Entities;
using Infrastructure.Snapshots;

namespace Infrastructure.Mapping
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<UserSnapshot, User>();
        }
    }
}