using AutoMapper;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Util.Extensions;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.AutoMapper
{
    public class DefaultMapperConfig : Profile
    {
        public DefaultMapperConfig()
        {
            CreateMap<Roles, RoleItemResponse>()
                .ForMember(dest => dest.OrganizeName, opts => opts.MapFrom(src => src.Organize == null ? null : src.Organize.Name));

            CreateMap<CreateOrganizeRequest, Organizes>()
                .ForMember(dest => dest.DepartLeaders, opts => opts.Ignore())
                .ForMember(dest => dest.HeadLeaders, opts => opts.Ignore());
        }
    }
}
