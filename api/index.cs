using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Api
{
    public class Index
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult("API is working");
        }
    }
} 