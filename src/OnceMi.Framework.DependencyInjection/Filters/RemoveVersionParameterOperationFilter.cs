using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnceMi.Framework.DependencyInjection.Filters
{
    public class RemoveVersionParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Remove version parameter from all Operations
            var versionParameter = operation.Parameters?.FirstOrDefault(p => p.Name == "version");
            if (versionParameter == null)
                return;
            operation.Parameters.Remove(versionParameter);
        }
    }
}
