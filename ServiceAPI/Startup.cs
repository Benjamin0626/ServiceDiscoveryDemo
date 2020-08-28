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

            #region ע�����

            var consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri(Configuration["ConsulSetting:ConsulAddress"]);
            });
            var registration = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),//����ʵ��Ψһ��ʶ
                Name = Configuration["ConsulSetting:ServiceName"],//������
                Address = Configuration["ConsulSetting:ServiceIP"], //����IP
                Port = int.Parse(Configuration["ConsulSetting:ServicePort"]),//����˿�  
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//����������ú�ע��
                    Interval = TimeSpan.FromSeconds(10),//�������ʱ����
                    HTTP = $"http://{Configuration["ConsulSetting:ServiceIP"]}:{Configuration["ConsulSetting:ServicePort"]}{Configuration["ConsulSetting:ServiceHealthCheck"]}",//��������ַ
                    Timeout = TimeSpan.FromSeconds(5)//��ʱʱ��
                }
            };
            //����ע��
            consulClient.Agent.ServiceRegister(registration).Wait();
            //Ӧ�ó�����ֹʱ��ȡ��ע��
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            });

            #endregion
        }
    }
}
