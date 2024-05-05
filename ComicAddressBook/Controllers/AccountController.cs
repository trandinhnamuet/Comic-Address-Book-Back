using ComicAddressBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace ComicAddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string query = "SELECT accountID, accountName, password, accountType, showName, accountAvatarLink FROM Account";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
            }
            return Ok(table);
        }

        [HttpGet("{id}")]
        public IActionResult GetAccountById(int id)
        {
            Console.WriteLine("Truy van thong tin tai khoan id" + id);
            string query = @"SELECT accountID, accountName, password, accountType, showName, accountAvatarLink FROM Account WHERE accountID = @accountID";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@accountID", id);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
            }

            if (table.Rows.Count == 0)
            {
                return NotFound("Không tìm thấy Account");
            }

            return Ok(table);
        }


        [HttpPost]
        public IActionResult Post(Account newAccount)
        {
            string query = @"INSERT INTO Account (accountName, password, accountType, showName, accountAvatarLink) 
                             VALUES (@accountName, @password, @accountType, @showName, @accountAvatarLink)";
            string sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@accountName", newAccount.accountName);
                    myCommand.Parameters.AddWithValue("@password", newAccount.password);
                    myCommand.Parameters.AddWithValue("@accountType", newAccount.accountType);
                    myCommand.Parameters.AddWithValue("@showName", newAccount.showName);
                    myCommand.Parameters.AddWithValue("@accountAvatarLink", newAccount.accountAvatarLink);

                    myCon.Open();
                        myCommand.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Thêm tài khoản mới thành công" });
        }

        [HttpPut]
        public IActionResult Put(Account modifiedAccount)
        {
            string query = @"UPDATE Account 
                             SET accountName = @accountName, password = @password, accountType = @accountType, 
                                 showName = @showName, accountAvatarLink = @accountAvatarLink 
                             WHERE accountID = @accountID";
            string sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@accountID", modifiedAccount.accountID);
                    myCommand.Parameters.AddWithValue("@accountName", modifiedAccount.accountName);
                    myCommand.Parameters.AddWithValue("@password", modifiedAccount.password);
                    myCommand.Parameters.AddWithValue("@accountType", modifiedAccount.accountType);
                    myCommand.Parameters.AddWithValue("@showName", modifiedAccount.showName);
                    myCommand.Parameters.AddWithValue("@accountAvatarLink", modifiedAccount.accountAvatarLink);

                    myCon.Open();
                    myCommand.ExecuteNonQuery();
                }
            }
            return Ok("Cập nhật tài khoản thành công");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string query = "DELETE FROM Account WHERE accountID = @accountID";
            string sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@accountID", id);

                    myCon.Open();
                    myCommand.ExecuteNonQuery();
                }
            }
            return Ok("Xóa bỏ thành công");
        }
    }
}
