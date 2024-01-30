using SDK;

namespace API.Idempotency;

public class HttpContextIdempotencyKeyProvider : IIdempotencyKeyProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContextIdempotencyKeyProviderOptions _options;

    public HttpContextIdempotencyKeyProvider(HttpContextIdempotencyKeyProviderOptions options,
        IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string?> ResolveAsync()
    {
        if (_httpContextAccessor.HttpContext != null &&
            _httpContextAccessor.HttpContext.Request.Query.ContainsKey(_options.HeaderKey))
            return Task.FromResult(_httpContextAccessor.HttpContext.Request.Query[_options.HeaderKey]
                .FirstOrDefault<string>());
        if (_httpContextAccessor.HttpContext != null &&
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(_options.HeaderKey, out var header))
            return Task.FromResult(header.FirstOrDefault<string>());
        return Task.FromResult<string?>(null);
    }
}