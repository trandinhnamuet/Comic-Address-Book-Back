using ComicAddressBook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Specialized;
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
        public JsonResult Get()
        {
            string query = "Select accountID, accountName, password, accountType from Account";
            DataTable table = new DataTable();
            String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            SqlDataReader myReader;
            using(SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using(SqlCommand myCommand = new SqlCommand(query, myCon))
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
        public JsonResult Post(Account newAccount)
        {
            string query = @"Insert into Account values ('" +   newAccount.accountName + "','" + newAccount.password + "','" + newAccount.accountType + "')";
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
            return new JsonResult("Thêm tài khoản mới thành công");
        }

        [HttpPut]
        public JsonResult Put(Account modifiedAccount)
        {
            string query = "Update Account set accountName = '" + modifiedAccount.accountName + "' " + "where accountID = " + modifiedAccount.accountID;
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
            return new JsonResult("Cập nhật tài khoản thành công");
        }

        [HttpDelete]
        public JsonResult Delete(Account deleteAccount)
        {
            string query = "Delete from Account where accountID = " + deleteAccount.accountID;
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
