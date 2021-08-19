using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Core.ConsulExtend;
using CC.ElectronicCommerce.StockInterface;
using CC.ElectronicCommerce.StockModel;
using CC.ElectronicCommerce.StockServiceService;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using DotNetCore.CAP.Messages;
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

namespace CC.ElectronicCommerce.StockMicroservice
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CC.ElectronicCommerce.StockMicroservice", Version = "v1" });
            });
            #region ����ע��
            services.AddTransient<OrangeStockContext>();
            services.AddTransient<IStockService, StockService>();
            services.AddTransient<IStockManagerService, StockManagerService>();
            services.AddTransient<CacheClientDB>();
            #endregion
            #region �����ļ�ע��
            services.Configure<MySqlConnOptions>(this.Configuration.GetSection("MysqlConn"));
            services.Configure<RedisConnOptions>(this.Configuration.GetSection("RedisConn"));
            services.Configure<RabbitMQOptions>(this.Configuration.GetSection("RabbitMQOptions"));
            #endregion

            #region CAP����
            string mysqlConn = this.Configuration["MysqlConn:url"];//���ݿ�����
            string rabbitMQHost = this.Configuration["RabbitMQOptions:HostName"];//RabbitMQ����

            services.AddOptions<DotNetCore.CAP.MySqlOptions>().Configure(o =>
            {
                o.ConnectionString = mysqlConn;
            });

            services.AddCap(x =>
            {
                x.UseMySql(mysqlConn);
                x.UseRabbitMQ(rabbitMQHost);
                x.FailedRetryCount = 30;
                x.FailedRetryInterval = 60;//second
                x.FailedThresholdCallback = failed =>
                {
                    var logger = failed.ServiceProvider.GetService<ILogger<Startup>>();
                    logger.LogError($@"MessageType {failed.MessageType} ʧ���ˣ� ������ {x.FailedRetryCount} ��, 
                        ��Ϣ����: {failed.Message.GetName()}");//do anything
                };

                #region ע��Consul���ӻ�
                x.UseDashboard();
                DiscoveryOptions discoveryOptions = new DiscoveryOptions();
                this.Configuration.Bind(discoveryOptions);
                x.UseDiscovery(d =>
                {
                    d.DiscoveryServerHostName = discoveryOptions.DiscoveryServerHostName;
                    d.DiscoveryServerPort = discoveryOptions.DiscoveryServerPort;
                    d.CurrentNodeHostName = discoveryOptions.CurrentNodeHostName;
                    d.CurrentNodePort = discoveryOptions.CurrentNodePort;
                    d.NodeId = discoveryOptions.NodeId;
                    d.NodeName = discoveryOptions.NodeName;
                    d.MatchPath = discoveryOptions.MatchPath;
                });
                #endregion
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CC.ElectronicCommerce.StockMicroservice v1"));
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
        }
    }
}
