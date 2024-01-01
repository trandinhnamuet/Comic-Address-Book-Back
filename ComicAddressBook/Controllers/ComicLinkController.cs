using ComicAddressBook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComicAddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComicLinkController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ComicLinkController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = "Select comicID, comicName, comicLink from ComicLink";
            DataTable table = new DataTable();
            String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }

        [HttpPost]
        [HttpPost]
        public JsonResult Post(ComicLink newComicLink)
        {
            string query = @"Insert into ComicLink values ('" + newComicLink.comicName + "','" + newComicLink.comicLink + "')";
            DataTable table = new DataTable();
            String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Thêm link mới thành công");
        }

        [HttpPut]
        public JsonResult Put(ComicLink modifiedComicLink)
        {
            string query = "Update ComicLink set ComicLinkName = '" + modifiedComicLink.comicName + "' " + "where ComicLinkID = " + modifiedComicLink.comicID;
            DataTable table = new DataTable();
            String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Cập nhật link thành công");
        }

        [HttpDelete]
        public JsonResult Delete(ComicLink deleteComicLink)
        {
            string query = "Delete from ComicLink where ComicLinkID = " + deleteComicLink.comicID;
            DataTable table = new DataTable();
            String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Xóa bỏ thành công");
        }
    }
}
