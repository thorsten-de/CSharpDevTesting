using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Config;
using ShoppingCartService.DataAccess;

namespace ShoppingCartService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ShoppingCartDatabaseSettings>(
                Configuration.GetSection(nameof(ShoppingCartDatabaseSettings)));

            services.AddSingleton<IShoppingCartDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ShoppingCartDatabaseSettings>>().Value);

            services.AddSingleton<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddSingleton<ICouponRepository, CouponRepository>();
            services.AddSingleton<IAddressValidator, AddressValidator>();
            services.AddSingleton<IShippingCalculator, ShippingCalculator>();
            services.AddSingleton<ICheckOutEngine, CheckOutEngine>();
            services.AddSingleton<ShoppingCartManager>();
            services.AddSingleton<CouponManager>();

            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddAutoMapper(typeof(Startup).Assembly);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Shopping cart service", Version = "v1"});
                var filePath = Path.Combine(AppContext.BaseDirectory, "ShoppingCartService.xml");
                c.IncludeXmlComments(filePath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShoppingCartService v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}