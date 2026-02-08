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

        if (objectResult.Value is IDictionary<string, object?> dictionary &&
            dictionary.ContainsKey("response"))
        {
            if (!dictionary.ContainsKey("appVersion"))
            {
                dictionary["appVersion"] = _options.AppVersion;
            }

            if (!dictionary.ContainsKey("assetVersion"))
            {
                dictionary["assetVersion"] = _options.AssetVersion;
            }

            objectResult.Value = dictionary;
            return;
        }

        if (objectResult.Value is IDictionary<string, object> dictionaryObjects &&
            dictionaryObjects.ContainsKey("response"))
        {
#pragma warning disable CS8620 // 参照型の NULL 値の許容の違いにより、パラメーターに引数を使用できません。
            var payload = new Dictionary<string, object?>(dictionaryObjects, StringComparer.Ordinal);
#pragma warning restore CS8620 // 参照型の NULL 値の許容の違いにより、パラメーターに引数を使用できません。
            if (!payload.ContainsKey("appVersion"))
            {
                payload["appVersion"] = _options.AppVersion;
            }

            if (!payload.ContainsKey("assetVersion"))
            {
                payload["assetVersion"] = _options.AssetVersion;
            }

            objectResult.Value = payload;
            return;
        }

        objectResult.Value = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["response"] = objectResult.Value,
            ["appVersion"] = _options.AppVersion,
            ["assetVersion"] = _options.AssetVersion
        };
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
