using Microsoft.OpenApi.Models;
using OnceMi.Framework.Model.Dto;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace OnceMi.Framework.Extension.Filters
{
    public class SwaggerParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //移除名称为version的参数
            var versionParameter = operation.Parameters?.FirstOrDefault(p => p.Name.ToLower() == "version");
            if (versionParameter != null)
            {
                operation.Parameters.Remove(versionParameter);
            }

            //移除标记有IgnoreDataMember的属性
            var ignoredProperties = context.MethodInfo?.GetParameters()
                ?.SelectMany(p => p.ParameterType?.GetProperties()
                    .Where(prop => prop.GetCustomAttributes<IgnoreDataMemberAttribute>()?.Any() == true && (prop.DeclaringType == typeof(IPageRequest) || (prop == typeof(IPageRequest)))))
                ?.ToList();
            if (ignoredProperties != null && ignoredProperties.Any())
            {
                foreach (var property in ignoredProperties)
                {
                    var removeItem = operation.Parameters.FirstOrDefault(p => p.Name == property.Name);
                    if (removeItem != null)
                    {
                        operation.Parameters.Remove(removeItem);
                    }
                }
            }
        }
    }
}
