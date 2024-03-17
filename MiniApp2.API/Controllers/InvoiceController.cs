using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MiniApp2.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInvoice()
        {
            var userName = HttpContext.User.Identity.Name;
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            //ver'tabaninda userid veya username alanlari ]zer'nden gerekl' datalari cek
            //stockId stockQuantaty Category Userd/UserName

            return Ok($"Invoice işlemleri -> UserName:{userName}- UserId:{userId}");
        }
    }
}
