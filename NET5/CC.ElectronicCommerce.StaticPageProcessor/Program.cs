using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Core.ConsulExtend;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CC.ElectronicCommerce.StaticPageProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                // {
                //     LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
                //     configurationBuilder
                //         .AddApollo(configurationBuilder.Build().GetSection("apollo"))
                //         .AddDefault()
                //         .AddNamespace("ZhaoxiMSAPrivateJson", ConfigFileFormat.Json)//自定义的private     NameSpace
                //         .AddNamespace(ConfigConsts.NamespaceApplication);//Apollo中默认       NameSpace的名称
                //     ;
                // })
                //.ConfigureLogging(loggingBuilder =>
                //{
                //    loggingBuilder.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
                //    loggingBuilder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
                //    loggingBuilder.AddLog4Net();
                //})
                .ConfigureServices((hostContext, services) =>
                {

                    IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                    services.AddHostedService<Worker>();
                    services.AddHostedService<InitPageWorker>();

                    #region 服务注入
                    services.AddTransient<OrangeContext, OrangeContext>();
                    services.Configure<MySqlConnOptions>(configuration.GetSection("MysqlConn"));

                    services.AddTransient<IGoodsService, GoodsService>();
                    services.AddTransient<CacheClientDB, CacheClientDB>();
                    services.Configure<RedisConnOptions>(configuration.GetSection("RedisConn"));

                    services.AddSingleton<RabbitMQInvoker>();
                    services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQOptions"));
                    #endregion

                    #region Consul
                    services.Configure<ConsulClientOption>(configuration.GetSection("ConsulClientOption"));
                    services.AddTransient<AbstractConsulDispatcher, PollingDispatcher>();
                    #endregion

                    services.AddHostedService<WarmupPageWorker>();
                });
    }
    }
