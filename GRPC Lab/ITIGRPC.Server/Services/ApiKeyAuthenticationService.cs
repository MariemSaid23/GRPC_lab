namespace ITIGRPC.Server.Services
{
    public class ApiKeyAuthenticationService: IApiKeyAuthenticationService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;

        public ApiKeyAuthenticationService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }
        public bool Authenticate()
        {
            var context = httpContextAccessor.HttpContext;

            if (!context.Request.Headers.TryGetValue(Consts.ApiKeyHeaderName, out var apiKey))
                return false;

            var apiKeySettings = configuration.GetSection(Consts.ApiKeySettingsKey).Value;

            return apiKey == apiKeySettings;
        }
    }
}
