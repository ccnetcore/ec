using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CC.ElectronicCommerce.ElasticSearchProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             //.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
             //{
             //    LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
             //    configurationBuilder
             //        .AddApollo(configurationBuilder.Build().GetSection("apollo"))
             //        .AddDefault()
             //        .AddNamespace("ZhaoxiMSAPrivateJson", ConfigFileFormat.Json)//自定义的private     NameSpace
             //        .AddNamespace(ConfigConsts.NamespaceApplication);//Apollo中默认       NameSpace的名称
             //    ;
             //})
             //   .ConfigureLogging(loggingBuilder =>
             //   {
             //       loggingBuilder.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
             //       loggingBuilder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
             //       loggingBuilder.AddLog4Net();
             //   })
             .ConfigureServices((hostContext, services) =>
             {
                 IConfiguration Configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                 services.Configure<ElasticSearchOptions>(Configuration.GetSection("ESConn"));
                 services.Configure<RabbitMQOptions>(Configuration.GetSection("RabbitMQOptions"));
                 
                 services.Configure<RedisConnOptions>(Configuration.GetSection("RedisConn"));
                 services.AddSingleton<RabbitMQInvoker>();


                 services.AddHostedService<InitESIndexWorker>();

                 #region 服务注入
                 services.AddTransient<CacheClientDB, CacheClientDB>();


                 services.AddTransient<OrangeContext, OrangeContext>();
                 services.AddTransient<IGoodsService, GoodsService>();
                 services.AddTransient<ISearchService, SearchService>();
                 services.AddTransient<IElasticSearchService, ElasticSearchService>();
                 services.AddTransient<IBrandService, BrandService>();
                 services.AddTransient<ICategoryService, CategoryService>();
                 services.AddTransient<ISpecService, SpecService>();
                 services.AddTransient<IPageDetailService, PageDetailService>();
                 #endregion




                 #region 配置文件注入
                 services.Configure<MySqlConnOptions>(Configuration.GetSection("MysqlConn"));
                 #endregion
                 services.AddHostedService<WarmupESIndexWorker>();

                 #region Worker
                 services.AddHostedService<Worker>();
                 #endregion
             });
    }
}
