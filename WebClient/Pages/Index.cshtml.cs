using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        #region
        private readonly ConsulClient _consulClient;
        #endregion
        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            #region
            _consulClient = new ConsulClient(c =>
            {
                //consul地址
                c.Address = new Uri(_configuration["ConsulSetting:ConsulAddress"]);
            });
            #endregion
        }

        public string Message { get; private set; } 
         

        public void OnGet()
        {
            Message = $"{DateTime.Now} {GetService()}";

            #region  通过consul发现服务
            //Message = $"{DateTime.Now} {GetServiceByConsul()}";
            #endregion

            #region  通过consul发现服务
            //Message = $"{DateTime.Now} {GetServiceByConsulBlockingQueries()}";
            #endregion

            #region  通过coelot + consul发现服务
            //Message = $"{DateTime.Now} {GetServiceByOcelot()}";
            #endregion
        }

        /// <summary>
        /// 手动实现服务发现
        /// </summary>
        /// <returns></returns>
        public string GetService()
        {
            try
            {
                string[] serviceUrls = { "http://localhost:9050", "http://localhost:9051" };
                string serviceUrl = serviceUrls[new Random().Next(0, 2)];

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(serviceUrl + "/api/Service/Show");
                return client.GetStringAsync(serviceUrl + "/api/Service/Show").Result;
            }
            catch
            {
                return "bad service";
            }
        } 

        /// <summary>
        /// 使用Consul实现服务发现
        /// </summary>
        /// <returns></returns>
        public string GetServiceByConsul()
        { 
            var consulClient = new ConsulClient(c =>
            {
                //consul地址
                c.Address = new Uri(_configuration["ConsulSetting:ConsulAddress"]);
            }); 
            var services = consulClient.Health.Service("APIService", null, true, null).Result.Response;//健康的服务

            string[] serviceUrls = services.Select(p => $"http://{p.Service.Address + ":" + p.Service.Port}").ToArray();//服务地址列表

            if (!serviceUrls.Any())
            {
                return  "服务列表为空！" ;
            }

            //每次随机访问一个服务实例 
            string serviceUrl = serviceUrls[new Random().Next(0, serviceUrls.Length)];

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(serviceUrl + "/api/Service/Show");
            return client.GetStringAsync(serviceUrl + "/api/Service/Show").Result;
        }

        /// <summary>
        /// 客户端 Consul发现 优化
        /// </summary>
        /// <returns></returns>
        public string GetServiceByConsulBlockingQueries()
        {
            var serviceUrls = ServicesList.Urls;

            if (!serviceUrls.Any())
            {
                return "服务列表为空！";
            }

            //每次随机访问一个服务实例 
            string serviceUrl = serviceUrls[new Random().Next(0, serviceUrls.Count)];

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(serviceUrl + "/api/Service/Show");
            return client.GetStringAsync(serviceUrl + "/api/Service/Show").Result;
        }

        /// <summary>
        /// 网关 
        /// </summary>
        /// <returns></returns>
        public string GetServiceByOcelot()
        {
            try
            {
                var gatewayUrl = "http://localhost:9070"; 
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(gatewayUrl + "/GateWay/api/Service/Show");
                return client.GetStringAsync(gatewayUrl + "/GateWay/api/Service/Show").Result;
            }
            catch
            {
                return "bad service";
            }
        }

         
    }
}
