using AutoMapper;
using HouseCom.Models;
using HouseCom.Models.DTO;

namespace HouseCom
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<House,HouseDTO>().ReverseMap();
            CreateMap<House, HouseCreateDTO>().ReverseMap();
            
        }
    }
}
