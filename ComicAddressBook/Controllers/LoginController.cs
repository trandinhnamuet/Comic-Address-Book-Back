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
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{accountNameInput}")]
        //Hàm HttpGet nhận string accountNameInput, kiểm tra trong bảng Account đã tồn tại bàn ghi nào có accountName = accountNameInput chưa
        //Nếu chưa thì trả về 0, nếu rồi thì trả về 1
        public JsonResult CheckAccountExists(string accountNameInput)
        {
            string query = $"SELECT COUNT(*) FROM Account WHERE accountName = @accountName";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Sử dụng parameter để tránh SQL Injection
                    myCommand.Parameters.AddWithValue("@accountName", accountNameInput);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            // Lấy ra số lượng tài khoản tìm thấy và trả về 0 nếu không tìm thấy, trả về 1 nếu có
            int count = int.Parse(table.Rows[0][0].ToString());
            return new JsonResult(count > 0 ? 1 : 0);
        }

        //Hàm check thông tin đăng nhập
        [HttpPost]
        public JsonResult Login(Account account)
        {
            //string query = "Select count(*) from Account where accountName = '"+account.accountName+"' and password = '"+account.password+"'";
            //string query = "Select TOP 1 accountID from Account where accountName = '"+account.accountName+"' and password = '"+account.password+"'";
            string query = "Select count(*), MIN(accountID) from Account where accountName = '" + account.accountName + "' and password = '" + account.password + "'";
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

        //[HttpPost]
        //public JsonResult Post(Account newAccount)
        //{
        //    string query = @"Insert into Account values ('" +   newAccount.accountName + "','" + newAccount.password + "','" + newAccount.accountType + "')";
        //    DataTable table = new DataTable();
        //    String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
        //    SqlDataReader myReader;
        //    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
        //    {
        //        myCon.Open();
        //        using (SqlCommand myCommand = new SqlCommand(query, myCon))
        //        {
        //            myReader = myCommand.ExecuteReader();
        //            table.Load(myReader);
        //            myReader.Close();
        //            myCon.Close();
        //        }
        //    }
        //    return new JsonResult("Thêm tài khoản mới thành công");
        //}

        //[HttpPut]
        //public JsonResult Put(Account modifiedAccount)
        //{
        //    string query = "UPDATE Account SET " +
        //       "accountName = '" + modifiedAccount.accountName + "', " +
        //       "password = '" + modifiedAccount.password + "', " +
        //       "accountType = '" + modifiedAccount.accountType + "' " +
        //       "WHERE accountID = " + modifiedAccount.accountID;

        //    DataTable table = new DataTable();
        //    String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
        //    SqlDataReader myReader;
        //    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
        //    {
        //        myCon.Open();
        //        using (SqlCommand myCommand = new SqlCommand(query, myCon))
        //        {
        //            myReader = myCommand.ExecuteReader();
        //            table.Load(myReader);
        //            myReader.Close();
        //            myCon.Close();
        //        }
        //    }
        //    return new JsonResult("Cập nhật tài khoản thành công");
        //}

        //[HttpDelete]
        //public JsonResult Delete(Account deleteAccount)
        //{
        //    string query = "Delete from Account where accountName = " + deleteAccount.accountName;
        //    DataTable table = new DataTable();
        //    String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
        //    SqlDataReader myReader;
        //    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
        //    {
        //        myCon.Open();
        //        using (SqlCommand myCommand = new SqlCommand(query, myCon))
        //        {
        //            myReader = myCommand.ExecuteReader();
        //            table.Load(myReader);
        //            myReader.Close();
        //            myCon.Close();
        //        }
        //    }
        //    return new JsonResult("Xóa bỏ thành công");
        //}
    }
}
