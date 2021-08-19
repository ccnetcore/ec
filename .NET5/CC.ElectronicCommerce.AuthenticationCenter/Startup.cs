using CC.ElectronicCommerce.AuthenticationCenter.Utility;
using CC.ElectronicCommerce.Core.ConsulExtend;
using CC.ElectronicCommerce.WebCore.FilterExtend;
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

namespace CC.ElectronicCommerce.AuthenticationCenter
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
            #region ·þÎñ×¢²á
            services.AddTransient<ICustomJWTService, CustomHSJWTService>();
            services.Configure<JWTTokenOptions>(Configuration.GetSection("JWTTokenOptions"));
            services.AddTransient<HttpHelperService>();
            #endregion
            services.AddControllers(option => {
                option.Filters.Add<CustomExceptionFilterAttribute>();
                option.Filters.Add(typeof(LogActionFilterAttribute));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CC.ElectronicCommerce.AuthenticationCenter", Version = "v1" });
            });
            #region Consul
            services.Configure<ConsulClientOption>(Configuration.GetSection("ConsulClientOption"));
            services.AddTransient<AbstractConsulDispatcher, PollingDispatcher>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CC.ElectronicCommerce.AuthenticationCenter v1"));
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
            #region Consul×¢²á
            app.UseConsulConfiguration(this.Configuration).Wait();
            #endregion
        }
    }
}
