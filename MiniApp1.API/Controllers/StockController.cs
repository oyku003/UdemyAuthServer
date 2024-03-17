using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace MiniApp1.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        [Authorize(Policy = "AgePolicy")]
        [Authorize(Roles = "admin", Policy = "AnkaraPolicy")]//admin rolunde gelmeyen token için 403 döner api.//claim bazlı doğrulama için policy ekledik
        [HttpGet]
        public IActionResult GetStock()
        {
            var userName = HttpContext.User.Identity.Name;//tokendan usernamei aldık
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                //ver'tabaninda userid veya username alanlari ]zer'nden gerekl' datalari cek
                //stockId stockQuantaty Category Userd/UserName

                return  Ok($"Stock İşlemleri -> UserName:{userName}- UserId:{userIdClaim.Value}");
        }
    }
}
