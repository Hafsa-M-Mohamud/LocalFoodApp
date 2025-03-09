using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Assignment3BAD.Filters
{
    public class CustomValidationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var relativePath = context.ApiDescription.RelativePath;

            if (context.ApiDescription.HttpMethod == "PUT" &&
                relativePath != null &&
                relativePath.Contains("dishes"))
            {
                // description for Price
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Price",
                    In = ParameterLocation.Query,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "number",
                        Format = "decimal",
                        Description = "Price must be a non-negative value."
                    }
                });
            }
        }
    }
}