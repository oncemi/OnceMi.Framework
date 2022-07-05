using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.IService.Article;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exceptions;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Article
{
    public class ArticleService : BaseService<ArticleInfo, long>, IArticleService
    {
        private readonly IArticleRepository _repository;
        private readonly ILogger<ArticleService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _accessor;
        private readonly IUserService _userService;

        public ArticleService(IArticleRepository repository
            , ILogger<ArticleService> logger
            , IIdGeneratorService idGenerator
            , IMapper mapper
            , IHttpContextAccessor accessor
            , IUserService userService) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accessor = accessor;
        }

        public async Task<IPageResponse<ArticleResponse>> Query(QueryArticlePageRequest request)
        {
            IPageResponse<ArticleResponse> response = new IPageResponse<ArticleResponse>();
            Expression<Func<Entity.Article.ArticleInfo, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                exp = exp.And(p => p.Title.Contains(request.Search) || p.SubTitle.Contains(request.Search));
            }
            if (request.IsDraw != null)
            {
                exp = exp.And(p => p.IsDraw == request.IsDraw.Value);
            }
            //默认排序
            if (request.OrderByParams == null || request.OrderByParams.Count == 0)
            {
                request.OrderBy = new string[] { $"{nameof(Entity.Article.ArticleInfo.CreatedTime)},desc" };
            }
            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<ArticleInfo> articles = await _repository.Where(exp)
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByParams)
                .IncludeMany(u => u.ArticleCategories)
                .IncludeMany(u => u.ArticleTags)
                .IncludeMany(u => u.ArticleCovers)
                .Include(u => u.CreateUser)
                .NoTracking()
                .ToListAsync(u => new ArticleInfo()
                {
                    Id = u.Id,
                    Title = u.Title,
                    SubTitle = u.SubTitle,
                    ViewNum = u.ViewNum,
                    IsAllowComment = u.IsAllowComment,
                    IsDraw = u.IsDraw,
                    IsTop = u.IsTop,
                    CommentNum = u.CommentNum,
                    CreatedTime = u.CreatedTime,
                    UpdatedTime = u.UpdatedTime,
                    ArticleCategories = u.ArticleCategories,
                    ArticleTags = u.ArticleTags,
                    ArticleCovers = u.ArticleCovers,
                    CreateUser = u.CreateUser,
                });
            if (articles == null || articles.Count == 0)
            {
                return new IPageResponse<ArticleResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<ArticleResponse>(),
                };
            }
            return new IPageResponse<ArticleResponse>()
            {
                Page = request.Page,
                Size = articles.Count,
                Count = count,
                PageData = _mapper.Map<List<ArticleResponse>>(articles),
            };
        }

        public async Task<ArticleResponse> Query(long id)
        {
            Expression<Func<Entity.Article.ArticleInfo, bool>> exp = p => !p.IsDeleted && p.Id == id;
            //查询
            Entity.Article.ArticleInfo article = await _repository.Where(exp)
                .IncludeMany(u => u.ArticleCategories)
                .IncludeMany(u => u.ArticleTags)
                .IncludeMany(u => u.ArticleCovers)
                .Include(u => u.CreateUser)
                .NoTracking()
                .ToOneAsync();
            if (article == null)
                throw new BusException(ResultCode.ARTICLE_QUERY_NOT_EXIST, $"未找到Id为{id}的文章");
            return _mapper.Map<ArticleResponse>(article);
        }

        [Transaction]
        public async Task<ArticleResponse> Insert(CreateOrUpdateArticleRequest request)
        {
            Entity.Article.ArticleInfo article = new Entity.Article.ArticleInfo()
            {
                Id = _idGenerator.NewId(),
                Title = request.Title,
                SubTitle = request.SubTitle,
                Content = request.Content,
                Summary = request.Summary,
                IsDraw = request.IsDraft,
                IsAllowComment = request.IsAllowComment,
                CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                CreatedTime = DateTime.Now
            };
            //保存
            var result = await _repository.InsertAsync(article);
            //创建分类
            if (request.Categories == null || request.Categories.Count == 0)
            {
                throw new BusException(ResultCode.ARTICLE_CATEGORY_CANNOT_NULL, "文章分类不能为空");
            }
            //查询出全部分类
            var allCategories = await _repository.Orm.Select<ArticleCategory>().ToListAsync();
            if (allCategories == null || allCategories.Count == 0)
            {
                throw new BusException(ResultCode.ARTICLE_NO_CATEGORY_IN_DB, "数据库中无可用文章分类");
            }
            request.Categories = request.Categories
                .GroupBy(p => p)
                .Select(p => p.Key)
                .ToList();
            List<ArticleBelongCategory> articleCategories = new List<ArticleBelongCategory>();
            foreach (var item in request.Categories)
            {
                var categoryItem = allCategories.Where(p => p.Id == item).FirstOrDefault();
                if (categoryItem == null || categoryItem.IsDeleted)
                {
                    throw new BusException(ResultCode.ARTICLE_CATEGORY_NOT_EXIST, $"所选文章分类不存在，文章分类Id：{item}");
                }
                if (!categoryItem.IsEnabled)
                {
                    throw new BusException(ResultCode.ARTICLE_CATEGORY_DISABLED, $"所选文章分类【{categoryItem.Name}】已经被禁用");
                }
                articleCategories.Add(new ArticleBelongCategory()
                {
                    Id = _idGenerator.NewId(),
                    CategoryId = item,
                    ArticleId = article.Id,
                    IsDeleted = false,
                    CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                    CreatedTime = DateTime.Now
                });
            }
            //插入
            await _repository.Orm.Insert(articleCategories)
                .ExecuteAffrowsAsync();
            //标签
            if (request.Tags != null && request.Tags.Count > 0)
            {
                request.Tags = request.Tags.GroupBy(p => p).Select(p => p.Key).ToList();
                List<ArticleTag> articleTags = new List<ArticleTag>();
                foreach (var item in request.Tags)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        throw new BusException(ResultCode.ARTICLE_ILLEGAL_TAG, $"标签中包含非法字符或标签为空");
                    }
                    if (item.Length > 20)
                    {
                        throw new BusException(ResultCode.ARTICLE_TAG_TOO_LONG, $"标签长度不能大于20");
                    }
                    articleTags.Add(new ArticleTag()
                    {
                        Id = _idGenerator.NewId(),
                        ArticleId = article.Id,
                        Tag = item,
                        IsDeleted = false,
                        CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                        CreatedTime = DateTime.Now
                    });
                }
                //插入
                await _repository.Orm.Insert(articleTags)
                    .ExecuteAffrowsAsync();
            }
            //封面
            if (request.Covers != null && request.Covers.Count > 0)
            {
                List<ArticleCover> articleCovers = new List<ArticleCover>();
                foreach (var item in request.Covers)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    articleCovers.Add(new ArticleCover()
                    {
                        Id = _idGenerator.NewId(),
                        ArticleId = article.Id,
                        Url = item,
                        IsDeleted = false,
                        CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                        CreatedTime = DateTime.Now
                    });
                }
                //插入
                await _repository.Orm.Insert(articleCovers)
                    .ExecuteAffrowsAsync();
            }
            return await Query(article.Id);
        }

        [Transaction]
        public async Task Update(CreateOrUpdateArticleRequest request)
        {
            if (request.Id == null || request.Id == 0)
            {
                throw new BusException(ResultCode.ARTICLE_ID_CANNOT_NULL, "更新文章时文章Id不能为空");
            }
            Entity.Article.ArticleInfo article = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (article == null)
            {
                throw new BusException(ResultCode.ARTICLE_UPDATE_NOT_EXIST, "更新文章不存在");
            }
            article.Title = request.Title;
            article.SubTitle = request.SubTitle;
            article.Content = request.Content;
            article.Summary = request.Summary;
            article.IsDraw = request.IsDraft;
            article.IsAllowComment = request.IsAllowComment;
            article.IsTop = request.IsTop;
            //保存
            var result = await _repository.UpdateAsync(article);

            //更新分类
            if (request.Categories == null || request.Categories.Count == 0)
            {
                throw new BusException(ResultCode.ARTICLE_CATEGORY_CANNOT_NULL, "文章分类不能为空");
            }
            //查询出全部分类
            var allCategories = await _repository.Orm.Select<ArticleCategory>().ToListAsync();
            if (allCategories == null || allCategories.Count == 0)
            {
                throw new BusException(ResultCode.ARTICLE_NO_CATEGORY_IN_DB, "数据库中无可用文章分类");
            }
            request.Categories = request.Categories
                .GroupBy(p => p)
                .Select(p => p.Key)
                .ToList();
            List<ArticleBelongCategory> articleCategories = new List<ArticleBelongCategory>();
            foreach (var item in request.Categories)
            {
                var categoryItem = allCategories.Where(p => p.Id == item).FirstOrDefault();
                if (categoryItem == null || categoryItem.IsDeleted)
                {
                    throw new BusException(ResultCode.ARTICLE_CATEGORY_NOT_EXIST, $"所选文章分类不存在，文章分类Id：{item}");
                }
                if (!categoryItem.IsEnabled)
                {
                    throw new BusException(ResultCode.ARTICLE_CATEGORY_DISABLED, $"所选文章分类【{categoryItem.Name}】已经被禁用");
                }
                articleCategories.Add(new ArticleBelongCategory()
                {
                    Id = _idGenerator.NewId(),
                    CategoryId = item,
                    ArticleId = article.Id,
                    IsDeleted = false,
                    CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                    CreatedTime = DateTime.Now
                });
            }
            //先删除原有的分类
            await _repository.Orm.Delete<ArticleBelongCategory>()
                .Where(p => p.ArticleId == article.Id)
                .ExecuteAffrowsAsync();
            //重新插入
            await _repository.Orm.Insert(articleCategories)
                .ExecuteAffrowsAsync();
            //标签
            if (request.Tags != null && request.Tags.Count > 0)
            {
                request.Tags = request.Tags.GroupBy(p => p).Select(p => p.Key).ToList();
                List<ArticleTag> articleTags = new List<ArticleTag>();
                foreach (var item in request.Tags)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        throw new BusException(ResultCode.ARTICLE_ILLEGAL_TAG, $"标签中包含非法字符或标签为空");
                    }
                    if (item.Length > 20)
                    {
                        throw new BusException(ResultCode.ARTICLE_TAG_TOO_LONG, $"标签长度不能大于20");
                    }
                    articleTags.Add(new ArticleTag()
                    {
                        Id = _idGenerator.NewId(),
                        ArticleId = article.Id,
                        Tag = item,
                        IsDeleted = false,
                        CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                        CreatedTime = DateTime.Now
                    });
                }
                //先删除原有的标签
                await _repository.Orm.Delete<ArticleTag>()
                    .Where(p => p.ArticleId == article.Id)
                    .ExecuteAffrowsAsync();
                //重新插入
                await _repository.Orm.Insert(articleTags)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                //删除原有的标签
                await _repository.Orm.Delete<ArticleTag>()
                    .Where(p => p.ArticleId == article.Id)
                    .ExecuteAffrowsAsync();
            }
            //封面
            if (request.Covers != null && request.Covers.Count > 0)
            {
                List<ArticleCover> articleCovers = new List<ArticleCover>();
                foreach (var item in request.Covers)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    articleCovers.Add(new ArticleCover()
                    {
                        Id = _idGenerator.NewId(),
                        ArticleId = article.Id,
                        Url = item,
                        IsDeleted = false,
                        CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                        CreatedTime = DateTime.Now
                    });
                }
                //先删除原有的封面
                await _repository.Orm.Delete<ArticleCover>()
                    .Where(p => p.ArticleId == article.Id)
                    .ExecuteAffrowsAsync();
                //重新插入
                await _repository.Orm.Insert(articleCovers)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                //删除原有的封面
                await _repository.Orm.Delete<ArticleCover>()
                    .Where(p => p.ArticleId == article.Id)
                    .ExecuteAffrowsAsync();
            }
        }

        [Transaction]
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(ResultCode.ARTICLE_DELETE_NOT_EXIST, "没有要删除的条目");
            }
            List<Entity.Article.ArticleInfo> articles = await _repository.Where(p => ids.Contains(p.Id)).ToListAsync();
            if (articles == null || articles.Count == 0)
            {
                return;
            }
            foreach (var item in articles)
            {
                //删除文章
                await _repository.Orm.Delete<ArticleInfo>().Where(p => p.Id == item.Id).ExecuteAffrowsAsync();
                //删除文章分类数据
                await _repository.Orm.Delete<ArticleBelongCategory>().Where(p => p.ArticleId == item.Id).ExecuteAffrowsAsync();
                //删除文章标签
                await _repository.Orm.Delete<ArticleTag>().Where(p => p.ArticleId == item.Id).ExecuteAffrowsAsync();
                //删除文章封面
                await _repository.Orm.Delete<ArticleCover>().Where(p => p.ArticleId == item.Id).ExecuteAffrowsAsync();
            }
        }
    }
}
