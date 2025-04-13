using API.Features.Services.AddServiceArea;
using API.Models;
using AutoMapper;

namespace API.Profiles;

public class ServiceMappingProfile : Profile
{
    public ServiceMappingProfile()
    {
        CreateMap<AddServiceRequest, ServiceInfo>()
            .ForMember(x => x.RegisteredAt, 
                y => y.MapFrom(_ => DateTime.UtcNow));
    }
}