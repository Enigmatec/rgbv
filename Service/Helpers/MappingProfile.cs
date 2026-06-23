using AutoMapper;
using Core.Entities;
using Newtonsoft.Json;
using Service.Models;

namespace Service.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            Config();
        }

        private void Config()
        {
            CreateMap<FileUpload, FileUploadDto>()
                .ForMember(dest => dest.CreatedBy,
                    opt => opt.MapFrom(src => src.CreatedBy.FullName))
                .ForMember(dest => dest.MetaData,
                    opt => opt.MapFrom(src => JsonConvert.DeserializeObject<FileUploadMetaData>(src.MetaData)));

            CreateMap<ServiceProvided, ServiceProvidedDto>()
                .ForMember(dest => dest.CreatedByName,
                    opt => opt.MapFrom(src => src.CreatedBy == null ? string.Empty : src.CreatedBy.FullName))
                .ForMember(dest => dest.SpOrCsoApprovalByName,
                    opt => opt.MapFrom(src => src.SpOrCsoApprovalBy == null ? string.Empty : src.SpOrCsoApprovalBy.FullName))
                .ForMember(dest => dest.StateApprovalByName,
                    opt => opt.MapFrom(src => src.StateApprovalBy == null ? string.Empty : src.StateApprovalBy.FullName))
                .ForMember(dest => dest.StateName,
                    opt => opt.MapFrom(src => src.State == null ? string.Empty : src.State.Name))
                .ForMember(dest => dest.OrganisationName,
                    opt => opt.MapFrom(src => src.Organisation == null ? string.Empty : src.Organisation.Name));

            CreateMap<Case, AdminDashboardProjectModel>();

            CreateMap<CaseCategory, CaseCategoryProjection>();

            CreateMap<FollowUp, FollowUpProjection>();

            CreateMap<Case, HomePageMetricsProjectionModel>();

            CreateMap<ComplaintForm, ComplaintFormDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User == null ? string.Empty : src.User.FullName))
                .ForMember(dest => dest.UserOrganisation,
                    opt => opt.MapFrom(src => src.User == null && src.User.Organisation == null ? string.Empty : src.User.Organisation.Name))
                .ForMember(dest => dest.UserState,
                    opt => opt.MapFrom(src => src.User == null && src.User.State == null ? string.Empty : src.User.State.Name))
                .ForMember(dest => dest.UserType,
                    opt => opt.MapFrom(src => src.User == null ? string.Empty : src.User.Type))
                .ForMember(dest => dest.ResolvedByUserName,
                    opt => opt.MapFrom(src => src.ResolvedByUser == null ? string.Empty : src.ResolvedByUser.FullName))
                  .ForMember(d => d.Attachements, opt => opt.ExplicitExpansion()); ;
        }
    }
}