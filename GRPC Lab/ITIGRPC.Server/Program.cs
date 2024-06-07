using ITIGRPC.Server.Handler;
using ITIGRPC.Server.Services;
using Microsoft.AspNetCore.Authentication;

namespace ITIGRPC.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddGrpc();

            builder.Services.AddScoped<IApiKeyAuthenticationService, ApiKeyAuthenticationService>();

            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = Consts.ApiKeySchemeName;
            }).AddScheme<AuthenticationSchemeOptions,
                        ApiKeyAuthenticationHandler>(Consts.ApiKeySchemeName, configureOptions => { });

            // Add services to the container.
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<InventroyService>();

            app.Run();
        }
    }
}
