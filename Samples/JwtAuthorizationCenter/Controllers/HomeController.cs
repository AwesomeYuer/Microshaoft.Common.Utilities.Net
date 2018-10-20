namespace JwtAuthorizationCenterServer.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return Ok(ControllerContext.HttpContext.User.Identity.Name);
        }
    }
}