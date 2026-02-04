using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Server.Infrastructure;

public sealed class ApiVersionResponseFilter : IResultFilter
{
    private readonly VersionOptions _options;

    public ApiVersionResponseFilter(IOptions<VersionOptions> options)
    {
        _options = options.Value;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult objectResult)
        {
            return;
        }

        if (!TryToDictionary(objectResult.Value, out var payload))
        {
            payload = new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        if (!payload.ContainsKey("appVersion"))
        {
            payload["appVersion"] = _options.AppVersion;
        }

        if (!payload.ContainsKey("assetVersion"))
        {
            payload["assetVersion"] = _options.AssetVersion;
        }

        objectResult.Value = payload;
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool TryToDictionary(object? value, out Dictionary<string, object?> result)
    {
        if (value is null)
        {
            result = new Dictionary<string, object?>(StringComparer.Ordinal);
            return true;
        }

        if (value is Dictionary<string, object?> dictionary)
        {
            result = new Dictionary<string, object?>(dictionary, StringComparer.Ordinal);
            return true;
        }

        if (value is IDictionary<string, object?> dictionaryWithNulls)
        {
            result = new Dictionary<string, object?>(dictionaryWithNulls, StringComparer.Ordinal);
            return true;
        }

        if (value is IDictionary<string, object> dictionaryObjects)
        {
            result = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var item in dictionaryObjects)
            {
                result[item.Key] = item.Value;
            }
            return true;
        }

        var element = JsonSerializer.SerializeToElement(value);
        if (element.ValueKind == JsonValueKind.Object)
        {
            result = ConvertToDictionary(element);
            return true;
        }

        result = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["value"] = ConvertElement(element)
        };
        return true;
    }

    private static Dictionary<string, object?> ConvertToDictionary(JsonElement element)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ConvertElement(property.Value);
        }

        return result;
    }

    private static object? ConvertElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertToDictionary(element),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var number) ? number : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }
}
