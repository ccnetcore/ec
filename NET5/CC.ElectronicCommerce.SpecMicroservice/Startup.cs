using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Core.ConsulExtend;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.SpecMicroservice
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CC.ElectronicCommerce.SpecMicroservice", Version = "v1" });
            });
            #region 服务注入
            services.AddTransient<OrangeContext, OrangeContext>();
            services.AddTransient<ISpecService, SpecService>();
            #endregion

            #region 配置文件注入
            services.Configure<MySqlConnOptions>(this.Configuration.GetSection("MysqlConn"));
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CC.ElectronicCommerce.SpecMicroservice v1"));
            }
            #region Consul-HealthCheck
            app.UseHealthCheckMiddleware("/Health");
            #endregion
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            #region Consul注册
            app.UseConsulConfiguration(this.Configuration).Wait();
            #endregion
        }
    }
}
