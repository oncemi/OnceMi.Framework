using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OnceMi.Framework.Model.Dto
{
    public class LoginResponse : IResponse
    {
        public string IdToken { get; set; }

        public string SessionState { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string TokenType { get; set; } = JwtBearerDefaults.AuthenticationScheme;

        public string Scope { get; set; }

        public UserItemResponse Profile { get; set; }

        public long ExpiresAt { get; set; }
    }
}
