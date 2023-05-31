using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace Diplom.Cookies
{
    public class CookiesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Перевірка, чи є відповідна кука в запиті
            if (context.ApiDescription.ActionDescriptor.EndpointMetadata
                .Any(m => m.GetType() == typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute)))
            {
                // Якщо атрибут [AllowAnonymous] присутній, то не перевіряємо куки
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Cookie",
                In = ParameterLocation.Cookie,
                Description = "Authentication cookie",
                Schema = new OpenApiSchema { Type = "string" },
                Required = true
            });
        }
    }

}
