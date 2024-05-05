using ComicAddressBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ComicAddressBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomePageController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<ComicTag>> GetLatestChapters()
        {
            // Kiểm tra nếu danh sách rỗng
            if (GlobalVariables.listComicTags == null || GlobalVariables.listComicTags.Count == 0)
            {
                return NotFound("Không có chapter nào được tìm thấy.");
            }

            // Chuyển đổi danh sách LinkedList thành mảng để trả về
            ComicTag[] tagsArray = new ComicTag[GlobalVariables.listComicTags.Count];
            GlobalVariables.listComicTags.CopyTo(tagsArray, 0);

            return Ok(tagsArray);
        }
    }
}
