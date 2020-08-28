using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServiceAPI
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region 注册服务

            var consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri(Configuration["ConsulSetting:ConsulAddress"]);
            });
            var registration = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),//服务实例唯一标识
                Name = Configuration["ConsulSetting:ServiceName"],//服务名
                Address = Configuration["ConsulSetting:ServiceIP"], //服务IP
                Port = int.Parse(Configuration["ConsulSetting:ServicePort"]),//服务端口  
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
                    HTTP = $"http://{Configuration["ConsulSetting:ServiceIP"]}:{Configuration["ConsulSetting:ServicePort"]}{Configuration["ConsulSetting:ServiceHealthCheck"]}",//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)//超时时间
                }
            };
            //服务注册
            consulClient.Agent.ServiceRegister(registration).Wait();
            //应用程序终止时，取消注册
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            });

            #endregion
        }
    }
}
