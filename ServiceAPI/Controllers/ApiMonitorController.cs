using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServiceAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiMonitorController : ControllerBase
    {
        /// <summary>
        /// 健康检查接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}
