using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class JobHistoryService : BaseService<JobHistory, long>, IJobHistoryService
    {
        private readonly IJobHistoryRepository _repository;
        private readonly ILogger<JobHistoryService> _logger;
        private readonly IMapper _mapper;

        public JobHistoryService(IJobHistoryRepository repository
            , ILogger<JobHistoryService> logger
            , IMapper mapper) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IPageResponse<JobHistoryItemResponse>> Query(JobHistoryPageRequest request)
        {
            IPageResponse<JobHistoryItemResponse> response = new IPageResponse<JobHistoryItemResponse>();
            if (request.OrderByModels.Count == 0)
            {
                request.OrderBy = new string[] { $"{nameof(JobHistory.Id)},desc" };
            }
            Expression<Func<JobHistory, bool>> exp = p => !p.IsDeleted && p.JobId == request.JobId;
            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<JobHistory> allParents = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allParents == null || allParents.Count == 0)
            {
                return new IPageResponse<JobHistoryItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<JobHistoryItemResponse>(),
                };
            }
            return new IPageResponse<JobHistoryItemResponse>()
            {
                Page = request.Page,
                Size = allParents.Count,
                Count = count,
                PageData = _mapper.Map<List<JobHistoryItemResponse>>(allParents),
            };
        }
    }
}
