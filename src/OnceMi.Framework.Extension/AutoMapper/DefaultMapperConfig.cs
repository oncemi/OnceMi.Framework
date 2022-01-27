using AutoMapper;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;

namespace OnceMi.Framework.Extension.AutoMapper
{
    public class DefaultMapperConfig : Profile
    {
        public DefaultMapperConfig()
        {
            CreateMap<Role, RoleItemResponse>()
                .ForMember(dest => dest.OrganizeName, opts => opts.MapFrom(src => src.Organize == null ? null : src.Organize.Name));

            CreateMap<CreateOrganizeRequest, Organize>()
                .ForMember(dest => dest.DepartLeaders, opts => opts.Ignore())
                .ForMember(dest => dest.HeadLeaders, opts => opts.Ignore());

            CreateMap<ArticleInfo, ArticleResponse>()
                .ForMember(dest => dest.Categories, opts => opts.MapFrom(src => src.ArticleCategories == null ? null : src.ArticleCategories))
                .ForMember(dest => dest.Comments, opts => opts.MapFrom(src => src.ArticleComments == null ? null : src.ArticleComments))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.ArticleTags == null ? null : src.ArticleTags))
                .ForMember(dest => dest.Covers, opts => opts.MapFrom(src => src.ArticleCovers == null ? null : src.ArticleCovers))
                .ForMember(dest => dest.Author, opts => opts.MapFrom(src => src.CreateUser == null ? null : src.CreateUser.NickName));
        }
    }
}
