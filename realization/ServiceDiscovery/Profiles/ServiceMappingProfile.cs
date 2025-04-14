using API.Features.Services.AddServiceArea;
using API.Features.Services.GetAllServicesArea;
using API.Models;
using AutoMapper;

namespace API.Profiles;

public class ServiceMappingProfile : Profile
{
    public ServiceMappingProfile()
    {
        CreateMap<AddServiceRequest, ServiceInfo>()
            .ForMember(x => x.RegisteredAt, y => y.MapFrom(_ => DateTime.UtcNow));

        CreateMap<ServiceInfo, ServiceInfoResponse>();

        CreateMap<ICollection<ServiceInfo>, GetAllServicesResponse>()
            .ForMember(
                x => x.Services,
                x => x.MapFrom(y => y));
    }
}
