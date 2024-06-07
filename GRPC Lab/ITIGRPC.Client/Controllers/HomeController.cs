using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Google.Protobuf.WellKnownTypes;
using static ITIGRPC.Protos.InventoryServiceProto;
using ITIGRPC.Protos;

namespace ITIGRPC.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("AddProduct")]
        public async Task<ActionResult> AddProduct(Product product)
        {
            var apiKey = _configuration["ApiKey"];

            var channelOptions = new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), CallCredentials.FromInterceptor((context, metadata) =>
                {
                    metadata.Add("x-api-key", apiKey);
                    return Task.CompletedTask;
                }))
            };

            using var channel = GrpcChannel.ForAddress("https://localhost:7081", channelOptions);
            var client = new InventoryServiceProto.InventoryServiceProtoClient(channel);

            try
            {
                var productRequest = new Id { Id_ = product.Id };
                var isExisted = await client.GetProductByIdAsync(productRequest);

                if (!isExisted.IsExisted_)
                {
                    var addedProduct = await client.AddProductAsync(product);
                    return Created("Product Created", addedProduct);
                }
                else
                {
                    var updatedProduct = await client.UpdateProductAsync(product);
                    return Created("Product Updated", updatedProduct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }


        [HttpPost("AddProducts")]
        public async Task<ActionResult> AddBulkProducts(List<Product> productsToAdd)
        {
            var apiKey = _configuration["ApiKey"];

            var channelOptions = new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), CallCredentials.FromInterceptor((context, metadata) =>
                {
                    metadata.Add("x-api-key", apiKey);
                    return Task.CompletedTask;
                }))
            };

            using var channel = GrpcChannel.ForAddress("https://localhost:7081", channelOptions);
            var client = new InventoryServiceProto.InventoryServiceProtoClient(channel);

            try
            {
                using var call = client.AddBulkProducts();

                foreach (var product in productsToAdd)
                {
                    await call.RequestStream.WriteAsync(product);
                }

                await call.RequestStream.CompleteAsync();
                var response = await call.ResponseAsync;

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }


        [HttpGet("GetReport")]
        public async Task<ActionResult> GetReport()
        {
            var apiKey = _configuration["ApiKey"];

            var channel = GrpcChannel.ForAddress("https://localhost:7081", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.SecureSsl
            });

            var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add("x-api-key", apiKey);
                return Task.CompletedTask;
            });

            var compositeCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
            var client = new InventoryServiceProto.InventoryServiceProtoClient(channel);

            List<Product> AddedProducts = new List<Product>();

            try
            {
                var call = client.GetProductReport(new Google.Protobuf.WellKnownTypes.Empty(), new CallOptions(credentials: callCredentials));

                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    AddedProducts.Add(call.ResponseStream.Current);
                }

                return Ok(AddedProducts);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
    }
}
