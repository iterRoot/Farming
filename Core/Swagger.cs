using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace FarmingApi.Core;

internal static class SwaggerExtension
{
    public static void AddMySwagger(this IServiceCollection services)
    {
        services.AddOpenApiDocument(document =>
        {
            document.AddSecurity("bearer", Enumerable.Empty<string>(),
                new OpenApiSecurityScheme()
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Copy this into the value field: Bearer {token}"
                });

            // Try to set GenerateEnumMappingDescription if the option exists on the document or its SchemaSettings.
            TrySetGenerateEnumMappingDescription(document, true);

            document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
        });
    }

    /// <summary>
    /// Attempts to set GenerateEnumMappingDescription on the document (or its SchemaSettings) using reflection.
    /// This keeps compatibility across different NSwag versions where the property may have moved or been removed.
    /// </summary>
    private static void TrySetGenerateEnumMappingDescription(object document, bool value)
    {
        if (document == null) return;

        var docType = document.GetType();

        // 1) Try direct property on the document (older NSwag)
        var directProp = docType.GetProperty("GenerateEnumMappingDescription", BindingFlags.Public | BindingFlags.Instance);
        if (directProp != null && directProp.CanWrite && directProp.PropertyType == typeof(bool))
        {
            directProp.SetValue(document, value);
            return;
        }

        // 2) Try SchemaSettings.GenerateEnumMappingDescription (common in some versions)
        var schemaProp = docType.GetProperty("SchemaSettings", BindingFlags.Public | BindingFlags.Instance);
        if (schemaProp != null)
        {
            var schemaObj = schemaProp.GetValue(document);
            if (schemaObj != null)
            {
                var schemaType = schemaObj.GetType();
                var enumProp = schemaType.GetProperty("GenerateEnumMappingDescription", BindingFlags.Public | BindingFlags.Instance);
                if (enumProp != null && enumProp.CanWrite && enumProp.PropertyType == typeof(bool))
                {
                    enumProp.SetValue(schemaObj, value);
                    return;
                }
            }
        }

        // 3) As a last resort, try to find any boolean property with that name anywhere on the object graph
        var fallback = docType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => string.Equals(p.Name, "GenerateEnumMappingDescription", StringComparison.OrdinalIgnoreCase)
                                 && p.CanWrite && p.PropertyType == typeof(bool));
        if (fallback != null)
        {
            fallback.SetValue(document, value);
        }
    }
}
