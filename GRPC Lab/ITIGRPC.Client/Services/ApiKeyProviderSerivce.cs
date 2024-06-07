
namespace ITIGRPC.Client.Services
{
    public class ApiKeyProviderSerivce: IApiKeyProviderService
    {
        private readonly IConfiguration _configuration;

        public ApiKeyProviderSerivce(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetApiKey()
        {
            return _configuration.GetSection(Consts.ApiKeySettingName).Value;
        }
    }
}
