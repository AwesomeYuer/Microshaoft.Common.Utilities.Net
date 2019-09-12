using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.ASPNetCore.StoreProcedureExecutor.Controllers
{
    public class DefaultController : Controller
    {
        [ResponseCache(Duration = 10)]
        public IActionResult Index()
        {
            return View();
        }
    }
}