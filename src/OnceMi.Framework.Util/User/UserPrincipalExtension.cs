using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.User
{
    public static class UserPrincipalExtension
    {
        public static (string idStr, long? id) GetSubject(this ClaimsPrincipal principal)
        {
            if (principal == null || principal.Claims == null || principal.Claims.Count() == 0)
                return (null, null);
            var subjectClaim = principal.Claims
                .Where(a => (a.Type == ClaimTypes.NameIdentifier || a.Type == JwtClaimTypes.Subject) && !string.IsNullOrEmpty(a.Value))
                .FirstOrDefault();
            if (subjectClaim == null)
                return (null, null);
            string idStr = subjectClaim.Value;
            if (long.TryParse(idStr, out long id))
            {
                return (idStr, id);
            }
            else
            {
                return (idStr, null);
            }
        }

        public static (string idStr, long? id) GetSubject(this IEnumerable<Claim> claims)
        {
            if (claims == null || claims.Count() == 0)
                return (null, null);
            var subjectClaim = claims.Where(a => (a.Type == ClaimTypes.NameIdentifier || a.Type == JwtClaimTypes.Subject) && !string.IsNullOrEmpty(a.Value))
                .FirstOrDefault();
            if (subjectClaim == null)
                return (null, null);
            string idStr = subjectClaim.Value;
            if (long.TryParse(idStr, out long id))
            {
                return (idStr, id);
            }
            else
            {
                return (idStr, null);
            }
        }

        public static List<long> GetRoles(this ClaimsPrincipal principal)
        {
            if (principal == null || principal.Claims == null || principal.Claims.Count() == 0)
                return null;
            var roles = principal.Claims
                .Where(a => (a.Type == ClaimTypes.Role || a.Type == JwtClaimTypes.Role) && !string.IsNullOrEmpty(a.Value))
                .Select(p => p.Value)
                .ToList();
            if (roles == null || roles.Count == 0)
                return null;
            List<long> result = new List<long>();
            foreach (var item in roles)
            {
                if (long.TryParse(item, out long id))
                {
                    result.Add(id);
                }
            }
            return result;
        }

        public static string GetName(this ClaimsPrincipal principal)
        {
            if (principal == null || principal.Claims == null || principal.Claims.Count() == 0)
                return null;
            if (principal.Identity == null || !principal.Identity.IsAuthenticated)
                return null;
            if (!string.IsNullOrEmpty(principal.Identity.Name))
                return principal.Identity.Name;
            var nameClaim = principal.Claims
                .Where(a => (a.Type == ClaimTypes.Name || a.Type == JwtClaimTypes.Name) && !string.IsNullOrEmpty(a.Value))
                .FirstOrDefault();
            if (nameClaim == null)
                return null;
            return nameClaim.Value;
        }
    }
}
