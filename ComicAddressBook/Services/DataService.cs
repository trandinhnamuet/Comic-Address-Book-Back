
using ComicAddressBook.Models;
using HtmlAgilityPack;
using Microsoft.Data.SqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Net;
using System.Xml;

namespace ComicAddressBook.Services
{
    public class DataService
    {
        string connectionString;
        string avatarFolder = @"C:\AngularProject\FinalDialog\src\assets\ComicImage";
        public DataService()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            connectionString = configuration.GetConnectionString("Comic_Address_Book");
        }

        public string getConnectionString()
        {
            return connectionString;
        }

        //Hàm trả về comicID từ comicLink
        public int ComicLinkToComicID(string comicLink)
        {
            int comicID = -1; // Giả sử rằng -1 là giá trị không hợp lệ

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT comicID FROM ComicLink WHERE comicLink = @comicLink", connection))
                {
                    command.Parameters.AddWithValue("@comicLink", comicLink);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        comicID = Convert.ToInt32(result);
                    }
                }
            }

            return comicID;
        }

        //Hàm nhận comicID, trả về comicLink
        public string ComicIDToComicLink(int comicID)
        {
            string comicLink = "This link is null";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT comicLink FROM ComicLink WHERE comicID = @comicID", connection))
                {
                    command.Parameters.AddWithValue("@comicID", comicID);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        comicLink = result.ToString() ?? "This link is null";
                    }
                }
            }

            return comicLink;
        }

        // Hàm nhận comicID, trả về comicName
        public string ComicIDToComicName(int comicID)
        {
            string comicName = "This name is null";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT comicName FROM ComicLink WHERE comicID = @comicID", connection))
                {
                    command.Parameters.AddWithValue("@comicID", comicID);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        comicName = result.ToString() ?? "This name is null";
                    }
                }
            }

            return comicName;
        }

        // Hàm nhận comicID, trả về avatarLink
        public string ComicIDToAvatarLink(int comicID)
        {
            string avatarLink = "This link is null";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT avatarLink FROM ComicLink WHERE comicID = @comicID", connection))
                {
                    command.Parameters.AddWithValue("@comicID", comicID);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        avatarLink = result.ToString() ?? "This link is null";
                    }
                }
            }

            return avatarLink;
        }


        //Hàm trả về comicID từ chapLink, truy vấn bảng Chapter
        public int ChapLinkToComicID(string chapLink)
        {
            int comicID = -1; // Giả sử rằng -1 là giá trị không hợp lệ

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT comicID FROM Chapter WHERE chapLink = @chapLink", connection))
                {
                    command.Parameters.AddWithValue("@chapLink", chapLink);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        comicID = Convert.ToInt32(result);
                    }
                }
            }

            return comicID;
        }


        //Hàm trả về chapID lớn nhất trong bảng Chapter
        public int GetLatestChapID()
        {
            int latestChapID = -1; // Giả sử rằng -1 là giá trị không hợp lệ

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT MAX(chapID) FROM Chapter", connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        latestChapID = Convert.ToInt32(result);
                    }
                }
            }

            return latestChapID;
        }

        //Hàm thêm chapter mới vào bảng Chapter, lưu ý hàm này không cập nhật chap này làm chap mới nhất trong ComicLink
        public void AddChapterToDatabase(string newChapName, string newChapLink, int comicID)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("INSERT INTO Chapter (chapName, comicID, chapLink, updateTime) VALUES (@chapName, @comicID, @chapLink, @releaseDate)", connection))
                {
                    //Chắc là chapID tự generate ra tịnh tiến
                    command.Parameters.AddWithValue("@chapName", newChapName);
                    command.Parameters.AddWithValue("@comicID", comicID);
                    command.Parameters.AddWithValue("@chapLink", newChapLink);
                    command.Parameters.AddWithValue("@releaseDate", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
        }


        //Hàm cập nhật trường latestChapID của bản ghi comicID trong bảng comicLink là chapID của chap cuối cùng được thêm vào
        public void UpdateLatestChapterInDatabase(int comicID, int latestChapID)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE ComicLink SET latestChapID = @latestChapID WHERE comicID = @comicID", connection))
                {
                    command.Parameters.AddWithValue("@latestChapID", latestChapID);
                    command.Parameters.AddWithValue("@comicID", comicID);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Hàm nhận comicID, lọc link ảnh avatar truyện từ comicLink, cập nhật giá trị đó cho trường avatarLink của bản ghi có comicID là comicID của bảng ComicLink
        public void UpdateAvatarLink(int comicID)
        {
            string comicLink = ComicIDToComicLink(comicID);

            if (comicLink == null) {
                Console.WriteLine("Không tìm được comicLink ứng với comicID " + comicID);
                return; 
            }
            // Kiểm tra xem comicLink chứa domain nào
            if (comicLink.Contains("nettruyen"))
            {
                Console.WriteLine("Lấy Avatar từ Nettruyen");
                UpdateAvatarLinkNettruyen(comicID);
                return;
            }
            else if (comicLink.Contains("blogtruyen"))
            {
                Console.WriteLine("Lấy Avatar từ Blogtruyen");
                UpdateAvatarLinkBlogtruyen(comicID);
                return;
            }
            else if (comicLink.Contains("truyenqq"))
            {
                Console.WriteLine("Lấy Avatar từ Truyenqq");
                UpdateAvatarLinkTruyenqq(comicID);
                return;
            }
            else if (comicLink.Contains("baotangtruyen"))
            {
                Console.WriteLine("Lấy Avatar từ baotangtruyen");
                UpdateAvatarLinkBaotangtruyen(comicID);
                return;
            }
            else if (comicLink.Contains("vlogtruyen"))
            {
                Console.WriteLine("Lấy Avatar từ vlogtruyen");
                UpdateAvatarLinkVlogtruyen(comicID);
                return;
            }
            else if (comicLink.Contains("nettrom"))
            {
                Console.WriteLine("Lấy Avatar từ nettrom");
                UpdateAvatarNettrom(comicID);
                return;
            }
            else if (comicLink.Contains("cuutruyen"))
            {
                Console.WriteLine("Lấy Avatar từ cuutruyen");
                UpdateAvatarCuutruyen(comicID);
                return;
            }
            else if (comicLink.Contains("vivicomi"))
            {
                Console.WriteLine("Lấy Avatar từ vivicomi");
                UpdateAvatarVivicomi(comicID);
                return;
            }
            else if (comicLink.Contains("youtube"))
            {
                Console.WriteLine("Lấy Avatar từ youtube playlist");
                UpdateAvatarYoutube(comicID);
                return;
            }
            else
            {
                Console.WriteLine("Link này chưa hỗ trợ lấy Avatar");
            }
        }

        public void UpdateAvatarLinkNettruyen(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    // Tạo và tải nội dung HTML từ trang web
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(comicLink);

                    // Tìm nút div có class là 'col-image' chứa link ảnh avatar
                    HtmlNode avatarNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'col-xs-4') and contains(@class,'col-image')]/img");

                    // Kiểm tra xem có tìm thấy nút không
                    if (avatarNode != null)
                    {
                        // Lấy giá trị của thuộc tính 'src' (link ảnh)
                        string avatarLink = avatarNode.GetAttributeValue("src", "");
                        // Cập nhật trường avatarLink của bản ghi trong bảng ComicLink
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                            {
                                command.Parameters.AddWithValue("@avatarLink", avatarLink);
                                command.Parameters.AddWithValue("@comicID", comicID);

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        // Trường hợp không tìm thấy nút div có class là 'col-image'
                        Console.WriteLine("Không tìm thấy link ảnh avatar cho truyện có comicID = " + comicID);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có lỗi trong quá trình truy cập hoặc phân tích HTML
                    Console.WriteLine("Lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
        public void UpdateAvatarLinkBlogtruyen(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    // Tạo và tải nội dung HTML từ trang web
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(comicLink);

                    // Tìm thẻ div có class là 'thumbnail'
                    HtmlNode thumbnailNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'thumbnail')]");

                    // Kiểm tra xem thẻ div thumbnail có tồn tại không
                    if (thumbnailNode != null)
                    {
                        // Lấy thẻ img trong thẻ div thumbnail
                        HtmlNode imgNode = thumbnailNode.SelectSingleNode(".//img");

                        // Kiểm tra xem thẻ img có tồn tại không
                        if (imgNode != null)
                        {
                            // Lấy giá trị của thuộc tính 'src' (link ảnh)
                            string avatarLink = imgNode.GetAttributeValue("src", "");

                            // Cập nhật trường avatarLink của bản ghi trong bảng ComicLink
                            using (var connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                                {
                                    command.Parameters.AddWithValue("@avatarLink", avatarLink);
                                    command.Parameters.AddWithValue("@comicID", comicID);
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Không tìm thấy thẻ img trong thẻ div thumbnail cho truyện có comicID = " + comicID);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Không tìm thấy thẻ div thumbnail cho truyện có comicID = " + comicID);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
        public void UpdateAvatarLinkTruyenqq(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    // Tạo và tải nội dung HTML từ trang web
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(comicLink);

                    // Tìm nút div có class là 'col-image' chứa link ảnh avatar
                    HtmlNode avatarNode = doc.DocumentNode.SelectSingleNode("//img[@itemprop='image']");

                    // Kiểm tra xem có tìm thấy nút không
                    if (avatarNode != null)
                    {
                        // Lấy giá trị của thuộc tính 'src' (link ảnh)
                        string avatarLink = avatarNode.GetAttributeValue("src", "");
                        // Cập nhật trường avatarLink của bản ghi trong bảng ComicLink
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                            {
                                command.Parameters.AddWithValue("@avatarLink", avatarLink);
                                command.Parameters.AddWithValue("@comicID", comicID);

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        // Trường hợp không tìm thấy nút div có class là 'col-image'
                        Console.WriteLine("Không tìm thấy link ảnh avatar cho truyện có comicID = " + comicID);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có lỗi trong quá trình truy cập hoặc phân tích HTML
                    Console.WriteLine("Lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
        public void UpdateAvatarLinkBaotangtruyen(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    // Tạo và tải nội dung HTML từ trang web
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(comicLink);

                    // Tìm nút div có class là 'col-image' chứa link ảnh avatar
                    HtmlNode avatarNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");

                    // Kiểm tra xem có tìm thấy nút không
                    if (avatarNode != null)
                    {
                        // Lấy giá trị của thuộc tính 'src' (link ảnh)
                        string avatarLink = avatarNode.GetAttributeValue("content", "");
                        // Cập nhật trường avatarLink của bản ghi trong bảng ComicLink
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                            {
                                command.Parameters.AddWithValue("@avatarLink", avatarLink);
                                command.Parameters.AddWithValue("@comicID", comicID);

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        // Trường hợp không tìm thấy nút div có class là 'col-image'
                        Console.WriteLine("Không tìm thấy link ảnh avatar cho truyện có comicID = " + comicID);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có lỗi trong quá trình truy cập hoặc phân tích HTML
                    Console.WriteLine("Lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
        public void UpdateAvatarLinkVlogtruyen(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    // Tạo và tải nội dung HTML từ trang web
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(comicLink);

                    // Tìm thẻ img trong thẻ div có class là 'image-commic-detail'
                    HtmlNode avatarNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'image-commic-detail')]/a/img");

                    // Kiểm tra xem có tìm thấy nút không
                    if (avatarNode != null)
                    {
                        // Lấy giá trị của thuộc tính 'src' (link ảnh)
                        string avatarLink = avatarNode.GetAttributeValue("data-src", "");

                        //Vì Vlogtruyen dùng hotlink protection để chống nhúng ảnh của họ vào trang web khác -> tải ảnh về local
                        //"sanitize" (làm sạch) tên này để nó trở thành một tên file hợp lệ
                        string sanitizedComicName = new string(ComicIDToComicName(comicID).Select(c => char.IsLetterOrDigit(c) || c == ' ' ? c : '_').ToArray());
                        sanitizedComicName = sanitizedComicName.Replace(" ", "_");
                        //sanitizedComicName = "image";
                        // Lấy phần mở rộng của file từ URL
                        string fileExtension = Path.GetExtension(avatarLink);
                        // Tạo đường dẫn đầy đủ với tên file và phần mở rộng
                        string localPath = Path.Combine(avatarFolder, sanitizedComicName + fileExtension);
                        // Phương thức tải ảnh
                        try
                        {
                            // Tạo yêu cầu HTTP
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(avatarLink);
                            request.Referer = "http://www.vlogtruyen12.com";

                            // Gửi yêu cầu và nhận phản hồi
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                // Kiểm tra xem yêu cầu có thành công không
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    // Đọc luồng dữ liệu trả về
                                    using (Stream inputStream = response.GetResponseStream())
                                    {
                                        // Đảm bảo thư mục đích tồn tại
                                        Directory.CreateDirectory(avatarFolder); // Tạo thư mục nếu nó không tồn tại

                                        // Tạo file đầu ra
                                        using (Stream outputStream = File.OpenWrite(localPath))
                                        {
                                            // Copy dữ liệu từ luồng đầu vào sang đầu ra
                                            inputStream.CopyTo(outputStream);
                                        }
                                    }
                                }
                            }
                        }
                        catch (WebException we)
                        {
                            // Xử lý lỗi ở đây
                            Console.WriteLine(we.Message);
                        }





                        // Cập nhật trường avatarLink của bản ghi trong bảng ComicLink
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                            {
                                command.Parameters.AddWithValue("@avatarLink", localPath);
                                command.Parameters.AddWithValue("@comicID", comicID);

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        // Trường hợp không tìm thấy nút div có class là 'col-image'
                        Console.WriteLine("Không tìm thấy link ảnh avatar cho truyện có comicID = " + comicID);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có lỗi trong quá trình truy cập hoặc phân tích HTML
                    Console.WriteLine("Lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
        public void UpdateAvatarNettrom(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    var service = ChromeDriverService.CreateDefaultService();
                    var options = new ChromeOptions();
                    options.AddArgument("--headless");
                    using (var driver = new ChromeDriver(service, options))
                    {
                        driver.Navigate().GoToUrl(comicLink);
                        System.Threading.Thread.Sleep(5000); // Đợi 5 giây

                        string content = driver.PageSource;

                        // Tìm thẻ img có class "rounded-lg absolute w-full top-0 left-0"
                        var avatarImg = driver.FindElement(By.CssSelector("img.rounded-lg.absolute.w-full.top-0.left-0"));

                        // Lấy giá trị src của thẻ img đó và lưu vào string avatarLink
                        string avatarLink = avatarImg.GetAttribute("src");

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                            {
                                command.Parameters.AddWithValue("@avatarLink", avatarLink);
                                command.Parameters.AddWithValue("@comicID", comicID);

                                command.ExecuteNonQuery();  
                            }
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine("Exception xảy ra khi lấy avatar truyện " + comicLink + " " + e.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
        public void UpdateAvatarCuutruyen(int  comicID)
        {
            UpdateAvatarNettrom(comicID);
        }
        public void UpdateAvatarVivicomi(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    // Tạo và tải nội dung HTML từ trang web
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(comicLink);

                    // Tìm thẻ img trong thẻ div có class là 'image-commic-detail'
                    HtmlNode avatarNode = doc.DocumentNode.SelectSingleNode("//img[contains(@class,'img-thumbnail')]");

                    // Kiểm tra xem có tìm thấy nút không
                    if (avatarNode != null)
                    {
                        // Lấy giá trị của thuộc tính 'src' (link ảnh)
                        string avatarLink = avatarNode.GetAttributeValue("src", "");

                        //Vì Vlogtruyen dùng hotlink protection để chống nhúng ảnh của họ vào trang web khác -> tải ảnh về local
                        //"sanitize" (làm sạch) tên này để nó trở thành một tên file hợp lệ
                        string sanitizedComicName = new string(ComicIDToComicName(comicID).Select(c => char.IsLetterOrDigit(c) || c == ' ' ? c : '_').ToArray());
                        sanitizedComicName = sanitizedComicName.Replace(" ", "_");
                        //sanitizedComicName = "image";
                        // Lấy phần mở rộng của file từ URL
                        string fileExtension = Path.GetExtension(avatarLink);
                        // Tạo đường dẫn đầy đủ với tên file và phần mở rộng
                        string localPath = Path.Combine(avatarFolder, sanitizedComicName + fileExtension);
                        Console.WriteLine(localPath);
                        // Phương thức tải ảnh
                        try
                        {
                            // Tạo yêu cầu HTTP
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(avatarLink);
                            request.Referer = "http://www.vivicomi.info";

                            // Gửi yêu cầu và nhận phản hồi
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                // Kiểm tra xem yêu cầu có thành công không
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    // Đọc luồng dữ liệu trả về
                                    using (Stream inputStream = response.GetResponseStream())
                                    {
                                        // Đảm bảo thư mục đích tồn tại
                                        Directory.CreateDirectory(avatarFolder); // Tạo thư mục nếu nó không tồn tại

                                        // Tạo file đầu ra
                                        using (Stream outputStream = File.OpenWrite(localPath))
                                        {
                                            // Copy dữ liệu từ luồng đầu vào sang đầu ra
                                            inputStream.CopyTo(outputStream);
                                        }
                                    }
                                }
                            }
                        }
                        catch (WebException we)
                        {
                            // Xử lý lỗi ở đây
                            Console.WriteLine(we.Message);
                        }





                        // Cập nhật trường avatarLink của bản ghi trong bảng ComicLink
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                            {
                                command.Parameters.AddWithValue("@avatarLink", localPath);
                                command.Parameters.AddWithValue("@comicID", comicID);

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        // Trường hợp không tìm thấy nút div có class là 'col-image'
                        Console.WriteLine("Không tìm thấy link ảnh avatar cho truyện có comicID = " + comicID);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có lỗi trong quá trình truy cập hoặc phân tích HTML
                    Console.WriteLine("Lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
        public void UpdateAvatarYoutube(int comicID)
        {
            // Lấy comicLink từ bảng ComicLink
            string comicLink = ComicIDToComicLink(comicID);

            // Kiểm tra nếu comicLink không rỗng
            if (!string.IsNullOrEmpty(comicLink))
            {
                try
                {
                    // Tạo và tải nội dung HTML từ trang web
                    var service = ChromeDriverService.CreateDefaultService();
                    var options = new ChromeOptions();
                    options.AddArgument("--headless");

                    using (var driver = new ChromeDriver(service, options))
                    {
                        driver.Navigate().GoToUrl(comicLink);

                        // Đợi cho đến khi nội dung được tải xong
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);

                        // Tìm thẻ img đầu tiên có class là 'style-scope yt-img-shadow' và id là 'img' chứa link ảnh avatar
                        // Lấy giá trị của thuộc tính 'src' (link ảnh) và in ra
                        IWebElement avatarImg = driver.FindElement(By.CssSelector("img.style-scope.yt-img-shadow#img"));

                        // Lấy giá trị của thuộc tính 'src' (link ảnh)
                        string avatarUrl = avatarImg.GetAttribute("src");

                        // In ra link ảnh avatar
                        Console.WriteLine("Link ảnh avatar: " + avatarUrl);

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("UPDATE ComicLink SET avatarLink = @avatarLink WHERE comicID = @comicID", connection))
                            {
                                command.Parameters.AddWithValue("@avatarLink", avatarUrl);
                                command.Parameters.AddWithValue("@comicID", comicID);

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có lỗi trong quá trình truy cập hoặc phân tích HTML
                    Console.WriteLine("Lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                // Trường hợp không có comicLink
                Console.WriteLine("Không tìm thấy comicLink cho comicID = " + comicID);
            }
        }
    }
}
