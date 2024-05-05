using ComicAddressBook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Specialized;
using System.Data;
using System;

namespace ComicAddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ChapterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = "Select chapID, chapName, comicID, chapLink, updateTime from Chapter";
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
        public JsonResult Post(Chapter newChapter)
        {
            string query = @"Insert into Chapter (chapName, comicID, chapLink, updateTime) values ('" +
                            newChapter.chapName + "','" + newChapter.comicID + "','" + newChapter.chapLink + "','" + newChapter.updateTime.ToString("yyyy-MM-dd HH:mm:ss") + "')";

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
            return new JsonResult("Thêm chương mới thành công");
        }

        [HttpPut]
        public JsonResult Put(Chapter modifiedChapter)
        {
            string query = @"UPDATE Chapter SET 
                            chapName = '" + modifiedChapter.chapName + "',comicID = '" + modifiedChapter.comicID + "',chapLink = '" + modifiedChapter.chapLink + "',updateTime = '" + 
                            modifiedChapter.updateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'WHERE chapID = " + modifiedChapter.chapID;

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
            return new JsonResult("Cập nhật chương thành công");
        }

        [HttpDelete]
        public JsonResult Delete(Chapter deleteChapter)
        {
            string query = "Delete from Chapter where chapID = " + deleteChapter.chapID;
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
            return new JsonResult("Xóa chương thành công");
        }
    }
}
