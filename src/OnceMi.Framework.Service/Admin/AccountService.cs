using AutoMapper;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Config;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Date;
using OnceMi.Framework.Util.Http;
using OnceMi.IdentityServer4.User;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IUsersService _userService;
        private readonly IUsersRepository _repository;
        private readonly IIdGeneratorService _idGenerator;
        private readonly ConfigManager _config;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _accessor;

        public AccountService(ILogger<AccountService> logger
            , IUsersService userService
            , IUsersRepository repository
            , IIdGeneratorService idGenerator
            , ConfigManager config
            , IMapper mapper
            , IHttpContextAccessor accessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<CacheService>));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        [Transaction]
        public async Task<LoginResponse> Authenticate(LoginRequest request)
        {
            if (_config.IdentityServer.IsEnabledIdentityServer)
            {
                throw new Exception("当前应用启用了IdentityServer认证中心，此功能被禁用");
            }
            Users user = await _userService.Query(request.Username, true);
            if (user == null)
            {
                throw new BusException(-1, "用户名或密码错误");
            }
            if (!user.Authenticate(request.Password))
            {
                throw new BusException(-1, "用户名或密码错误");
            }
            //build response
            var response = await BuildJwtToken(user);
            //写登录日志
            await WriteLoginHistory(response);

            return response;
        }

        [Transaction]
        public async Task<LoginResponse> RefreshToken(RefeshTokenRequest request)
        {
            UserToken userToken = await _repository.Orm.Select<UserToken>()
                .Where(p => p.RefeshToken == request.Token && p.RefeshTokenExpiration >= DateTime.Now && !p.IsDeleted)
                .NoTracking()
                .ToOneAsync();
            if (userToken == null)
            {
                throw new BusException(-1, "Invalid_grant");
            }
            Users user = await _userService.Query(userToken.UserId.ToString(), true);
            if (user == null)
            {
                throw new BusException(-1, "User deleted or disabled");
            }
            LoginResponse response = await BuildJwtToken(user);
            return response;
        }

        public async Task RevokeToken(RevokeTokenRequest request)
        {
            await _repository.Orm.Select<UserToken>()
                .Where(p => p.RefeshToken == request.Token && !p.IsDeleted)
                .ToUpdate()
                .Set(p => p.IsDeleted, true)
                .ExecuteAffrowsAsync();
        }

        private async Task<LoginResponse> BuildJwtToken(Users user)
        {
            long issuedAt = TimeUtil.Timestamp();
            DateTime expiresTime = TimeUtil.UnixTimeStampToDateTime(issuedAt + _config.TokenManagement.AccessExpiration * 60);

            var claims = new List<Claim>
            {
                //NotBefore指示在这之前此token无效
                new Claim(JwtClaimTypes.NotBefore, issuedAt.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.Expiration,( issuedAt + _config.TokenManagement.AccessExpiration * 60).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.ClientId, _config.AppSettings.AppId.ToString()),
                new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
                new Claim(JwtClaimTypes.AuthenticationTime, issuedAt.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.IdentityProvider, "local"),
                new Claim(JwtClaimTypes.Name, user.UserName),
                new Claim(JwtClaimTypes.NickName, user.NickName),
                new Claim(JwtClaimTypes.SessionId, Guid.NewGuid().ToString()),
                new Claim(JwtClaimTypes.IssuedAt, issuedAt.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.AuthenticationMethod, "pwd"),
            };
            foreach (var item in user.Roles)
            {
                claims.Add(new Claim(JwtClaimTypes.Role, item.Id.ToString(), ClaimValueTypes.Integer64));
            }
            foreach (var item in user.Organizes)
            {
                claims.Add(new Claim("organize", item.Id.ToString(), ClaimValueTypes.Integer64));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.TokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(issuer: _config.TokenManagement.Issuer
                , audience: _config.TokenManagement.Audience
                , claims: claims
                , expires: expiresTime
                , signingCredentials: credentials);
            string token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Create user token failed.");
            }
            string refeshToken = await GenerateRefreshToken(user, token);
            LoginResponse response = new LoginResponse()
            {
                AccessToken = token,
                RefreshToken = refeshToken,
                Profile = _mapper.Map<UserItemResponse>(user),
                ExpiresAt = issuedAt + _config.TokenManagement.AccessExpiration * 60,
            };
            return response;
        }

        private async Task<string> GenerateRefreshToken(Users user, string token)
        {
            string refeshToken = Guid.NewGuid().ToString("N");
            UserToken userToken = await _repository.Orm.Select<UserToken>().Where(p => p.UserId == user.Id).ToOneAsync();
            if (userToken == null)
            {
                userToken = new UserToken()
                {
                    Id = _idGenerator.NewId(),
                    UserId = user.Id,
                    Token = token,
                    RefeshToken = refeshToken,
                    RefeshTokenExpiration = DateTime.Now.AddDays(7),
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now,
                    IsDeleted = false,
                };
                int result = await _repository.Orm.Insert(userToken).ExecuteAffrowsAsync();
                if (result <= 0)
                {
                    throw new Exception("Save user refesh token failed.");
                }

            }
            else
            {
                int result = await _repository.Orm
                    .Select<UserToken>()
                    .Where(p => p.UserId == user.Id)
                    .ToUpdate()
                    .Set(p => p.Token, token)
                    .Set(p => p.RefeshToken, refeshToken)
                    .Set(p => p.RefeshTokenExpiration, DateTime.Now.AddDays(7))
                    .Set(p => p.IsDeleted, false)
                    .ExecuteAffrowsAsync();
                if (result <= 0)
                {
                    throw new Exception("Save user refesh token failed.");
                }
            }
            return refeshToken;
        }

        private async Task WriteLoginHistory(LoginResponse response)
        {
            if (response == null || response.Profile == null)
                return;

            //这个RequestHelper非常耗时，有待优化

            //RequestHelper requestHelper = new RequestHelper(_accessor.HttpContext);
            //var history = new LoginHistory()
            //{
            //    Id = _idGenerator.NewId(),
            //    UserId = response.Profile.Id,
            //    IP = requestHelper.GetRequestIpAddress(),
            //    Browser = requestHelper.GetBrowser(),
            //    OS = requestHelper.GetOSName(),
            //    Device = requestHelper.GetDevice(),
            //    UserAgent = _accessor.HttpContext.Request.Headers["User-Agent"],
            //    Type = LoginHistoryType.Login,
            //    Status = true,
            //    Message = "登录成功",
            //};

            var history = new LoginHistory()
            {
                Id = _idGenerator.NewId(),
                UserId = response.Profile.Id,
                IP = "",
                Browser = "",
                OS = "",
                Device = "",
                UserAgent = _accessor.HttpContext.Request.Headers["User-Agent"],
                Type = LoginHistoryType.Login,
                Status = true,
                Message = "登录成功",
            };
            await _repository.Orm.Insert(history).ExecuteAffrowsAsync();
        }
    }
}
