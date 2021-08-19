using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using Zhaoxi.MSACommerce.Service;
using CC.ElectronicCommerce.Service;
using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Enums;
using Com.Ctrip.Framework.Apollo.Logging;

namespace CC.ElectronicCommerce.OrderProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                            {
                                configurationBuilder.AddCommandLine(args);
                                LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
                                configurationBuilder
                                    .AddApollo(configurationBuilder.Build().GetSection("apollo"))
                                    .AddDefault()
                                    .AddNamespace("CCECJson", ConfigFileFormat.Json)//自定义的private NameSpace
                                    .AddNamespace(ConfigConsts.NamespaceApplication);//Apollo中默认NameSpace的名称
                            })
                            .ConfigureLogging(loggingBuilder =>
                            {
                                loggingBuilder.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
                                loggingBuilder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
                                loggingBuilder.AddLog4Net();
                            })
             .ConfigureServices((hostContext, services) =>
             {
                 IConfiguration Configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                 services.Configure<RabbitMQOptions>(Configuration.GetSection("RabbitMQOptions"));
                 services.Configure<ElasticSearchOptions>(Configuration.GetSection("ESConn"));
                 services.Configure<RedisConnOptions>(Configuration.GetSection("RedisConn"));

                 #region 服务注入
                 services.AddSingleton<RabbitMQInvoker>();
                 services.AddSingleton<CacheClientDB>();

                 services.AddTransient<OrangeContext>();
                 services.AddTransient<IGoodsService, GoodsService>();
                 services.AddTransient<IOrderService, OrderService>();
                 services.AddTransient<IBrandService, BrandService>();
                 services.AddTransient<ICartService, CartService>();
                 services.AddTransient<ISearchService, SearchService>();
                 services.AddTransient<IElasticSearchService, ElasticSearchService>();
                 services.AddTransient<ICategoryService, CategoryService>();
                 services.AddTransient<ISpecService, SpecService>();
                 services.AddTransient<IPageDetailService, PageDetailService>();

                 //services.AddWechatPay();
                 #endregion

                 #region 配置文件注入
                 services.Configure<MySqlConnOptions>(Configuration.GetSection("MysqlConn"));
                 #endregion

                 services.AddHostedService<CleanCartWorker>();
                 services.AddHostedService<CancelOrderWorker>();
             });
    }
}
