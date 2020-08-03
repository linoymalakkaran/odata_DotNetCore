using AutoMapper;
using Models = MGSurvey.Business.Models;
using Entities = MGSurvey.Domain.Entities;
namespace MGSurvey.Data.Mapper
{
    public class FormMapperProfile : Profile
    {
        public FormMapperProfile()
        {
            CreateMap<Entities.Form, Models.Form>(MemberList.Destination)
                .ReverseMap();
            CreateMap<Entities.ValidationSchema, Models.Form>(MemberList.Destination)
                            .ReverseMap();
        }
    }
}
