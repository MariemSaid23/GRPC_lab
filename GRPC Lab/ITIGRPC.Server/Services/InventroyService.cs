using Microsoft.AspNetCore.Authorization;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using static ITIGRPC.Protos.InventoryServiceProto;
using ITIGRPC.Protos;

namespace ITIGRPC.Server.Services
{
    public class InventroyService: InventoryServiceProtoBase
    {
        public static List<Product> Products { get; set; }

        public InventroyService()
        {
            Products = new List<Product>()
        {
            new Product{Id=1,Name="TV",Price=4999,Quantity=10 , Category= ProductCategory.Electronics ,ExpiryDate=Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5))},
            new Product{Id=2,Name="HeadPhone",Price=299,Quantity=10,  Category=ProductCategory.Electronics ,ExpiryDate=Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5))},
            new Product{Id=3,Name="Phone",Price=5999,Quantity=10 ,  Category=ProductCategory.Electronics,ExpiryDate=Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5))},
            new Product{Id=4,Name="Blazer",Price=399,Quantity=10 , Category= ProductCategory.Clothes ,ExpiryDate=Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5))},
            new Product{Id=5,Name="T-Shirt",Price=299,Quantity=10,  Category=ProductCategory.Clothes ,ExpiryDate=Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5))},
            new Product{Id=6,Name="Trousers",Price=499,Quantity=10 ,  Category=ProductCategory.Clothes,ExpiryDate=Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5))}
        };

        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<Products> GetAll(Empty request, ServerCallContext context)
        {
            var products = new Products();

            products.Products_.AddRange(Products);

            return await Task.FromResult(products); 
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<IsExisted> GetProductById(Id request, ServerCallContext context)
        {
            var product = Products.FirstOrDefault(i => i.Id == request.Id_);

            if (product != null)
            {
                return await Task.FromResult(new IsExisted
                {
                    IsExisted_ = true
                }); 
            }

            return await Task.FromResult(new IsExisted
            {
                IsExisted_ = false
            }); 
        }


        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<Product> AddProduct(Product request, ServerCallContext context)
        {
            Products.Add(request);

            return await Task.FromResult(request);
        }


        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<Product> UpdateProduct(Product request, ServerCallContext context)
        {
            var product = Products.FirstOrDefault(i => i.Id == request.Id);

            if (product != null)
            {
                product.Name = request.Name;
                product.Price = request.Price;
                product.Quantity = request.Quantity;
            }

            return await Task.FromResult(product);
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<productsNumber> AddBulkProducts(IAsyncStreamReader<Product> requestStream, ServerCallContext context)
        {
            int count = 0;

            await foreach (var request in requestStream.ReadAllAsync())
            {
                Products.Add(request);
                ++count;
            }

            return await Task.FromResult(new productsNumber { Count = count });
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task GetProductReport(Empty request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
        {
            foreach (var item in Products)
            {
                await responseStream.WriteAsync(item);
            }

            await Task.CompletedTask;
        }
    }
}
