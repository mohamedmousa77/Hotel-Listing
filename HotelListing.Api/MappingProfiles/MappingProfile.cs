using AutoMapper;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Hotel;

namespace HotelListing.Api.MappingProfiles;

public class HotelMappingProfile: Profile
{
    public HotelMappingProfile()
    {
        CreateMap<Hotel, GetHotelDto>()
       .ForMember(d => d.Country, cfg => cfg.MapFrom<CountryNameResolver>());

        CreateMap<CreateHotelDto, Hotel>();
        CreateMap<UpdateHotelDto, Hotel>();
    }
}

public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        CreateMap<Hotel, GetHotelDto>()
       .ForMember(d => d.Country, cfg => cfg.MapFrom<CountryNameResolver>());

        CreateMap<CreateHotelDto, Hotel>();
    }
}

public class CountryNameResolver : IValueResolver<Hotel, GetHotelDto, string>
{
    public string Resolve(Hotel source, GetHotelDto destination, string destMember, ResolutionContext context)
    {
        return source.Country?.Name ?? string.Empty;
    }
}