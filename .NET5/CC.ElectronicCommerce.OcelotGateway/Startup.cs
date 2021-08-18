using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocelot.Cache;
using Ocelot.Cache.CacheManager;
using Ocelot.Provider.Polly;

namespace CC.ElectronicCommerce.OcelotGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CC.ElectronicCommerce.OcelotGateway", Version = "v1" });

              
            });
            services.AddOcelot()//Ocelot如何处理
             .AddConsul()//支持Consul
             .AddCacheManager(x =>
             {
                 x.WithDictionaryHandle();//默认字典存储
                })
             .AddPolly()
             ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "鉴权 API V1");
                    c.SwaggerEndpoint("/user/swagger/v1/swagger.json", "用户 API V1");
                    c.SwaggerEndpoint("/search/swagger/v1/swagger.json", "搜索 API V1");
                    c.SwaggerEndpoint("/category/swagger/v1/swagger.json", "类别 API V1");
                    c.SwaggerEndpoint("/cart/swagger/v1/swagger.json", "购物车 API V1");
                    c.SwaggerEndpoint("/brand/swagger/v1/swagger.json", "品牌 API V1");
                    c.SwaggerEndpoint("/order/swagger/v1/swagger.json", "订单 API V1");
                    c.SwaggerEndpoint("/pay/swagger/v1/swagger.json", "支付 API V1");
                    c.SwaggerEndpoint("/stock/swagger/v1/swagger.json", "库存 API V1");
                });
            //}

            app.UseOcelot();
            //app.UseHttpsRedirection();

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
