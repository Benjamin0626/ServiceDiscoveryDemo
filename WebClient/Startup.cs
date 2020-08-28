using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebClient
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
            services.AddHttpClient();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            #region
            Task.Run(() =>
            {
                ConsulClient _consulClient = new ConsulClient(c =>
                {
                    //consul��ַ
                    c.Address = new Uri(Configuration["ConsulSetting:ConsulAddress"]);
                });

                //WaitTimeĬ��Ϊ5����
                var queryOptions = new QueryOptions { WaitTime = TimeSpan.FromMinutes(10) };
                while (true)
                {
                    var res = _consulClient.Health.Service("APIService", null, true, queryOptions).Result;


                    //�汾�Ų�һ�� ˵�������б����˱仯
                    if (queryOptions.WaitIndex != res.LastIndex)
                    {
                        //����̨��ӡһ�»�ȡ�����б����Ӧʱ�����Ϣ
                        Console.WriteLine($"{DateTime.Now}��ȡ APIService ��queryOptions.WaitIndex��{queryOptions.WaitIndex}  LastIndex��{res.LastIndex}");

                        queryOptions.WaitIndex = res.LastIndex;

                        //����̨��ӡһ�»�ȡ�����б����Ӧʱ�����Ϣ
                        Console.WriteLine($"{DateTime.Now}���� APIService ��queryOptions.WaitIndex��{queryOptions.WaitIndex}  LastIndex��{res.LastIndex}");


                        //�����ַ�б�
                        var serviceUrls = res.Response.Select(p => $"http://{p.Service.Address + ":" + p.Service.Port}").ToList();

                        ServicesList.Urls = serviceUrls;
                    }
                }
            });
            #endregion
        }
    }
}
