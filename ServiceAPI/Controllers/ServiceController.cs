using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ServiceAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        public ServiceController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        public ActionResult<string> Show()
        {
            return Configuration["Msg"];
        }
    }
}
