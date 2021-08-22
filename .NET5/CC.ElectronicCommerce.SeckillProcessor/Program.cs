using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Core;
using CC.ElectronicCommerce.Model;
using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Enums;
using Com.Ctrip.Framework.Apollo.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Zhaoxi.MSACormmerce.SeckillProcessor;

namespace CC.ElectronicCommerce.SeckillProcessor
{
    //dotnet CC.ElectronicCommerce.SeckillProcessor.dll
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    //.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                    //{
                    //    LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
                    //    configurationBuilder
                    //    .AddApollo(configurationBuilder.Build().GetSection("apollo"))
                    //    .AddDefault()
                    //    .AddNamespace("CCECJson", ConfigFileFormat.Json)//自定义的private     NameSpace
                    //    .AddNamespace(ConfigConsts.NamespaceApplication);//Apollo中默认       NameSpace的名称
                    //    ;
                    //})
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<RabbitMQInvoker>();
                        IConfiguration Configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                        services.Configure<RabbitMQOptions>(Configuration.GetSection("RabbitMQOptions"));

                        #region 服务注入
                        services.AddTransient<OrangeContext>();
                        services.AddTransient<CacheClientDB>();
                        #endregion

                        #region 配置文件注入
                        services.Configure<MySqlConnOptions>(Configuration.GetSection("MysqlConn"));
                        services.Configure<RedisConnOptions>(Configuration.GetSection("RedisConn"));
                        #endregion

                        services.AddHostedService<SeckillOrderWorker>();
                        services.AddHostedService<Worker>();
                    });
        }
    }
}
