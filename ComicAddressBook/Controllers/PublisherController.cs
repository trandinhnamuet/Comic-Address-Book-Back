using ComicAddressBook.Models;
using ComicAddressBook.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Specialized;
using System.Data;

namespace ComicAddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        DataService dataService = new DataService();
        // Hàm nhận accountID, trả về danh sách ComicLink có accountID là accountID, truy vấn bảng ComicLink
        [HttpGet("{accountID}")]
        public async Task<ActionResult<IEnumerable<ComicLink>>> GetComicLinksByAccountId(int accountID)
        {
            var comicLinks = new List<ComicLink>();

            using (var connection = new SqlConnection(dataService.getConnectionString()))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM ComicLink WHERE accountID = @accountID";

                using (var command = new SqlCommand(query, connection))
                {
                    // Thêm tham số vào truy vấn để tránh SQL Injection
                    command.Parameters.AddWithValue("@accountID", accountID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            comicLinks.Add(new ComicLink
                            {
                                comicID = reader.GetInt32(reader.GetOrdinal("comicID")),
                                comicName = reader.GetString(reader.GetOrdinal("comicName")),
                                comicLink = reader.GetString(reader.GetOrdinal("comicLink")),
                                translator = reader.GetString(reader.GetOrdinal("translator")),
                                accountID = reader.GetInt32(reader.GetOrdinal("accountID")),
                                alternativeName = reader.IsDBNull(reader.GetOrdinal("alternativeName")) ? null : reader.GetString(reader.GetOrdinal("alternativeName")),
                                author = reader.GetString(reader.GetOrdinal("author")),
                                description = reader.GetString(reader.GetOrdinal("description")),
                                latestChapID = reader.GetInt32(reader.GetOrdinal("latestChapID")),
                                latestUpdateTime = reader.GetDateTime(reader.GetOrdinal("latestUpdateTime")),
                                avatarLink = reader.GetString(reader.GetOrdinal("avatarLink")),
                            });
                        }
                    }
                }
            }

            if (comicLinks.Count == 0)
            {
                return NotFound("Không tìm thấy comic nào cho accountID này.");
            }

            return comicLinks;
        }
    }
}
