using ComicAddressBook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using HtmlAgilityPack;
using ComicAddressBook.Services;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;

namespace ComicAddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComicLinkController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public DataService dataService = new DataService();
        int newComicID = -1;
        public ComicLinkController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [NonAction]
        public LinkedList<Chapter> FirstScan(ComicLink newComicLink)
        {
            if (newComicLink.comicLink.Contains("truyenqq"))
            {
                return TruyenqqFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("blogtruyen"))
            {
                return BlogtruyenFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("nettruyen"))
            {
                return NettruyenFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("baotangtruyen"))
            {
                return BaotangtruyenFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("vlogtruyen"))
            {
                return VlogtruyenFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("nettrom"))
            {
                return NettromFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("cuutruyen"))
            {
                return CuutruyenFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("vivicomi"))
            {
                return VivicomiFirstScan(newComicLink);
            }
            else if (newComicLink.comicLink.Contains("youtube"))
            {
                //return YoutubeFirstScan(newComicLink);
                return YoutubeFirstScan(newComicLink).GetAwaiter().GetResult();
            }
            else
            {
                return null;
            }
        }

        [NonAction]
        public LinkedList<Chapter> NettruyenFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            //scan html code
            HttpClient client = new HttpClient();
            string page = client.GetStringAsync(newComicLink.comicLink).Result;

            //Đọc file html bằng HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            //Tìm thẻ div có class list-chapter và id nt_listchapter
            HtmlNode listChapterNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='list-chapter' and @id='nt_listchapter']");
            if (listChapterNode != null)
            {
               // LinkedList<string> chaptersList = new LinkedList<string>();

                //Danh sách chap được bao bởi 1 thẻ ul, mỗi chap nằm trong 1 thẻ li. Mỗi thẻ li có 1 div link chap tên chap và 1 div thời điểm cập nhật
                /*<ul>
                      <li class="row ">
                          <div class="col-xs-5 chapter"> <a href="https://nettruyenco.vn/truyen-tranh/nam-vung-y-te/chapter-15/2" data-id="2">Chapter 1.5</a> </div>
                          <div class="col-xs-4 no-wrap small text-center">10 giờ trước</div>
                      </li>
                      <li class="row ">
                          <div class="col-xs-5 chapter"> <a href="https://nettruyenco.vn/truyen-tranh/nam-vung-y-te/chapter-1/1" data-id="1">Chapter 1</a> </div>
                          <div class="col-xs-4 no-wrap small text-center">10 giờ trước</div>
                      </li>
                 </ul>*/
                HtmlNodeCollection liNodes = listChapterNode.SelectNodes(".//li");

                if (liNodes != null)
                {
                    for (int i = liNodes.Count - 1; i >= 0; i--)
                    {
                        HtmlNode liNode = liNodes[i];
                        HtmlNode chapterNode = liNode.SelectSingleNode(".//div[@class='col-xs-5 chapter']/a");
                        //HtmlNode dateNode = liNode.SelectSingleNode(".//div[@class='col-xs-4 no-wrap small text-center']");

                        string newChapLink = chapterNode.Attributes["href"].Value;
                        string newChapName = chapterNode.InnerText.Trim();

                        Console.WriteLine(newChapLink);
                        Console.WriteLine(newChapName);
                        Console.WriteLine("");
                        // Output chapLink and chapName
                        Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                        chapters.AddLast(chapter);
                    }

                }
                Console.WriteLine("Số lượng chap mới: " + chapters.Count);
            }
            else
            {
                Console.WriteLine("Không tìm thấy danh sách chương.");
            }

            return chapters;
        }
        [NonAction]
        public LinkedList<Chapter> BlogtruyenFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            // Tạo một đối tượng HtmlWeb để tải về nội dung của trang web
            HtmlWeb web = new HtmlWeb();
            // Tải về nội dung của trang web vào một đối tượng HtmlDocument
            HtmlDocument doc = web.Load(newComicLink.comicLink);
            // Lấy mã HTML của trang web
            string page = doc.DocumentNode.OuterHtml;

            //Đọc file html bằng HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            // Tìm thẻ div có class list-wrap và id list-chapters
            HtmlNode divNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='list-wrap' and @id='list-chapters']");

            if (divNode != null)
            {
                // Tạo danh sách các thẻ p trong thẻ div đã tìm thấy
                HtmlNodeCollection pNodes = divNode.SelectNodes(".//p");
                if (pNodes != null)
                {
                    // Duyệt qua từng thẻ p theo chiều ngược lại
                    for (int i = pNodes.Count - 1; i >= 0; i--)
                    {
                        HtmlNode pNode = pNodes[i];

                        // Trích xuất thông tin từ thẻ span có class là "title"
                        HtmlNode titleNode = pNode.SelectSingleNode(".//span[@class='title']/a");
                        string newChapName = titleNode.InnerText.Trim(); // Lấy nội dung của thẻ a
                        string newChapLink = "https://blogtruyenmoi.com" + titleNode.GetAttributeValue("href", ""); // Lấy thuộc tính href của thẻ a

                        // In ra thông tin đã trích xuất
                        Console.WriteLine("Tên chương mới: " + newChapName);
                        Console.WriteLine("Link chương mới: " + newChapLink);

                        // Output chapLink and chapName
                        Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                        chapters.AddLast(chapter);
                    }
                }
                else
                {
                    Console.WriteLine("Không tìm thấy thẻ p trong div list-wrap có id list-chapters.");
                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy div có class list-wrap và id list-chapters.");
            }

            return chapters;
        }
        [NonAction]
        public LinkedList<Chapter> TruyenqqFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            // Tạo một đối tượng HtmlWeb để tải về nội dung của trang web
            HtmlWeb web = new HtmlWeb();
            // Tải về nội dung của trang web vào một đối tượng HtmlDocument
            HtmlDocument doc = web.Load(newComicLink.comicLink);
            // Lấy mã HTML của trang web
            string page = doc.DocumentNode.OuterHtml;

            //Đọc file html bằng HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            // Tìm thẻ div có class là "works-chapter-list"
            HtmlNode divNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='works-chapter-list']");

            if (divNode != null)
            {
                // Tìm tất cả các thẻ div con có class là "works-chapter-item"
                HtmlNodeCollection pNodes = divNode.SelectNodes(".//div[@class='works-chapter-item']");

                if (pNodes != null)
                {
                    // Duyệt qua từng thẻ p theo chiều ngược lại
                    for (int i = pNodes.Count - 1; i >= 0; i--)
                    {
                        HtmlNode pNode = pNodes[i];

                        // Trích xuất thông tin từ thẻ a bên trong div
                        HtmlNode anchorNode = pNode.SelectSingleNode(".//a");
                        // Lấy giá trị của thuộc tính href và innerText của thẻ a
                        string newChapName = anchorNode.InnerText.Trim();
                        string newChapLink = anchorNode.GetAttributeValue("href", "");

                        // In ra thông tin đã trích xuất
                        Console.WriteLine("Tên chương mới: " + newChapName);
                        Console.WriteLine("Link chương mới: " + newChapLink);

                        // Output chapLink and chapName
                        Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                        chapters.AddLast(chapter);
                    }
                }
                else
                {
                    Console.WriteLine("Không tìm thấy thẻ p trong div list-wrap có id list-chapters.");
                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy div có class list-wrap và id list-chapters.");
            }

            return chapters;
        }
        [NonAction]
        public LinkedList<Chapter> BaotangtruyenFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            // Đường dẫn tới ChromeDriver
            var driverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            // Thiết lập cho trình duyệt chạy ở chế độ headless nếu muốn
            options.AddArguments("headless");
            string page = "";
            using (var driver = new ChromeDriver(driverService, options))
            {
                // Điều hướng đến trang web
                driver.Navigate().GoToUrl(newComicLink.comicLink);
                // Chờ đợi một thời gian cho trang web hoàn tất tải và xử lý JavaScript
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                // Lấy mã HTML của trang web lưu vào string page
                page = driver.PageSource;
            }

            //Đọc file html bằng HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            //Tìm thẻ div có class list-chapter và id nt_listchapter
            HtmlNode listChapterNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='list-chapter' and @id='nt_listchapter']");
            if (listChapterNode != null)
            {
               // LinkedList<string> chaptersList = new LinkedList<string>();

                HtmlNodeCollection aNodes = listChapterNode.SelectNodes(".//a");

                if (aNodes != null)
                {
                    for (int i = aNodes.Count - 1; i >= 0; i--)
                    {
                        HtmlNode aNode = aNodes[i];

                        string newChapLink = aNode.Attributes["href"].Value;
                        string newChapName = aNode.InnerText.Trim();

                        Console.WriteLine(newChapLink);
                        Console.WriteLine(newChapName);
                        Console.WriteLine("");
                        // Output chapLink and chapName
                        Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                        chapters.AddLast(chapter);
                    }

                }
                Console.WriteLine("Số lượng chap mới: " + chapters.Count);
            }
            else
            {
                Console.WriteLine("Không tìm thấy danh sách chương.");
            }

            return chapters;
        }
        [NonAction]
        public LinkedList<Chapter> VlogtruyenFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            // Đường dẫn tới ChromeDriver
            var driverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            // Thiết lập cho trình duyệt chạy ở chế độ headless nếu muốn
            options.AddArguments("headless");
            string page = "";
            using (var driver = new ChromeDriver(driverService, options))
            {
                // Điều hướng đến trang web
                driver.Navigate().GoToUrl(newComicLink.comicLink);
                // Chờ đợi một thời gian cho trang web hoàn tất tải và xử lý JavaScript
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                // Lấy mã HTML của trang web lưu vào string page
                page = driver.PageSource;
            }

            //Đọc file html bằng HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            //Tìm thẻ div có class list-chapter và id nt_listchapter
            HtmlNode listChapterNode = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='ul-list-chaper-detail-commic' and @id='style-15']");
            Console.WriteLine(page);
            if (listChapterNode != null)
            {
               // LinkedList<string> chaptersList = new LinkedList<string>();

                HtmlNodeCollection aNodes = listChapterNode.SelectNodes(".//a");

                if (aNodes != null)
                {
                    for (int i = aNodes.Count - 1; i >= 0; i--)
                    {
                        HtmlNode chapterNode = aNodes[i];
                        //HtmlNode dateNode = liNode.SelectSingleNode(".//div[@class='col-xs-4 no-wrap small text-center']");

                        string newChapLink = chapterNode.Attributes["href"].Value;
                        string newChapName = chapterNode.InnerText.Trim();

                        Console.WriteLine(newChapLink);
                        Console.WriteLine(newChapName);
                        Console.WriteLine("");
                        // Output chapLink and chapName
                        Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                        chapters.AddLast(chapter);
                    }

                }
                Console.WriteLine("Số lượng chap mới: " + chapters.Count);
            }
            else
            {
                Console.WriteLine("Không tìm thấy danh sách chương.");
            }

            return chapters;
        }
        [NonAction]
        public LinkedList<Chapter> NettromFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            // Đường dẫn tới ChromeDriver
            var driverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            // Thiết lập cho trình duyệt chạy ở chế độ headless nếu muốn
            options.AddArguments("headless");

            using (var driver = new ChromeDriver(driverService, options))
            {
                // Điều hướng đến trang web
                driver.Navigate().GoToUrl(newComicLink.comicLink);
                // Chờ đợi một thời gian cho trang web hoàn tất tải và xử lý JavaScript
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                // Lấy mã HTML của trang web lưu vào string page
                var chapterDivs = driver.FindElements(By.CssSelector("div.vue-recycle-scroller__item-wrapper > div > div"));

                foreach (var chapterDiv in chapterDivs)
                {
                    string newChapNumber = chapterDiv.FindElement(By.CssSelector("div.p-1.flex.font-bold.text-gray-800 > span:last-child")).Text.Trim();
                    string newChapTitle = chapterDiv.FindElement(By.CssSelector("div.p-2 > div.truncate")).Text.Trim();
                    string newChapName = ($"Chương {newChapNumber}. {newChapTitle}");
                    string newChapLink = chapterDiv.FindElement(By.CssSelector("a")).GetAttribute("href");

                    Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                    chapters.AddFirst(chapter);
                }
            }
            Console.WriteLine("Số lượng chap mới: " + chapters.Count);

            return chapters;
        }
        [NonAction]
        public LinkedList<Chapter> CuutruyenFirstScan(ComicLink newComicLink)
        {
            return NettromFirstScan(newComicLink);
        }
        [NonAction]
        public LinkedList<Chapter> VivicomiFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            //scan html code
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
            string page = client.GetStringAsync(newComicLink.comicLink).Result;

            //Đọc file html bằng HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            //Tìm thẻ div có class list-chapter và id nt_listchapter
            HtmlNode listChapterNode = htmlDoc.DocumentNode.SelectSingleNode("//table[@class='table table-striped']");
            //Console.WriteLine(listChapterNode.OuterHtml);
            if (listChapterNode != null)
            {

                HtmlNodeCollection aNodes = listChapterNode.SelectNodes(".//a");

                if (aNodes != null)
                {
                    for (int i = aNodes.Count - 1; i >= 0; i--)
                    {
                        HtmlNode aNode = aNodes[i];

                        string newChapLink = aNode.Attributes["href"].Value;
                        string newChapName = aNode.InnerText.Trim();

                        Console.WriteLine(newChapLink);
                        Console.WriteLine(newChapName);
                        Console.WriteLine("");
                        // Output chapLink and chapName
                        Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                        chapters.AddLast(chapter);
                    }

                }
                Console.WriteLine("Số lượng chap mới: " + chapters.Count);
            }
            else
            {
                Console.WriteLine("Không tìm thấy danh sách chương.");
            }

            return chapters;
        }

        [NonAction]
        static async Task<int> GetVideosCount(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string pageContent = await client.GetStringAsync(url);

                string searchString = "numVideosText\":{\"runs\":[{\"text\":\"";
                int startPosition = pageContent.IndexOf(searchString);
                if (startPosition == -1)
                {
                    throw new Exception("Không tìm thấy số lượng video trong playlist");
                }

                startPosition += searchString.Length;
                int endPosition = pageContent.IndexOf('"', startPosition);
                string videosCountString = pageContent.Substring(startPosition, endPosition - startPosition);

                // Loại bỏ các ký tự không phải là số
                videosCountString = videosCountString.Replace(".", "").Replace(",", "");

                // Bây giờ chuỗi chỉ chứa số nguyên, chuyển đổi nó sang int
                return int.Parse(videosCountString);
            }
        }
        [NonAction]
        public async Task<LinkedList<Chapter>> YoutubeFirstScan(ComicLink newComicLink)
        {
            LinkedList<Chapter> chapters = new LinkedList<Chapter>();

            // Đường dẫn tới ChromeDriver
            var driverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            // Thiết lập cho trình duyệt chạy ở chế độ headless nếu muốn
            options.AddArguments("headless");
            string page = "";
            using (var driver = new ChromeDriver(driverService, options))
            {
                // Điều hướng đến trang web
                driver.Navigate().GoToUrl(newComicLink.comicLink);

                //Mỗi 100 video phải lăn chuột xuống cuối trang 1 lần để load 100 video tiếp theo
                int videoCount = await GetVideosCount(newComicLink.comicLink);
                for (int ix = 0; ix < videoCount / 100; ix++)
                {
                    Console.WriteLine(ix);
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollBy(0, 999999999);");
                    System.Threading.Thread.Sleep(1000);
                }

                // Chờ đợi một thời gian cho trang web hoàn tất tải và xử lý JavaScript
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);

                // Lấy mã HTML của trang web lưu vào string page
                //page = driver.PageSource;

                // Lấy danh sách các thẻ a với id là video-title
                var videoLinks = driver.FindElements(By.Id("video-title"));
                List<string> videoDetails = new List<string>();

                int i = 0;
                // Duyệt qua mỗi thẻ và lấy tiêu đề cùng đường dẫn
                foreach (var videoLink in videoLinks)
                {
                    i++;
                    string newChapName = videoLink.GetAttribute("title");
                    string newChapLink = videoLink.GetAttribute("href");
                    Console.WriteLine(newChapLink);
                    Console.WriteLine(newChapName);
                    Console.WriteLine("");

                    // Output chapLink and chapName
                    Chapter chapter = new Chapter(0, newChapName, newComicID, newChapLink, DateTime.Now);
                    chapters.AddLast(chapter);
                }
            }

            return chapters;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = "Select comicID, comicName, comicLink, translator, accountID, alternativeName, author, description from ComicLink";
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
        public JsonResult Post(ComicLink newComicLink)
        {
            //      
            //string query = @"INSERT INTO ComicLink values (N'" + newComicLink.comicName + "','" + newComicLink.comicLink + "',N'" + newComicLink.translator + "','" + newComicLink.accountID + "',N'" + newComicLink.alternativeName + "',N'" + newComicLink.author + "',N'" + newComicLink.description + "'," + newComicLink.latestChapID + ",'" + newComicLink.latestUpdateTime.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            string query = @"INSERT INTO ComicLink (comicName, comicLink, translator, accountID, alternativeName, author, description, latestChapID, latestUpdateTime, avatarLink) VALUES (N'" + newComicLink.comicName + "','" + newComicLink.comicLink + "',N'" + newComicLink.translator + "','" + newComicLink.accountID + "',N'" + newComicLink.alternativeName + "',N'" + newComicLink.author + "',N'" + newComicLink.description + "'," + newComicLink.latestChapID + ",'" + newComicLink.latestUpdateTime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + newComicLink.avatarLink + "')";
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

            //Lấy newComicID của link vừa được thêm vào
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand("SELECT MAX(comicID) AS MaxComicID FROM ComicLink;", myCon))
                {
                    newComicID = Convert.ToInt32(myCommand.ExecuteScalar());
                    Console.WriteLine(newComicID);
                }
                myCon.Close();
            }

            //Quét chapter trong link và thêm toàn bộ chapter quét được vào bảng Chapter
            LinkedList<Chapter> chapters = FirstScan(newComicLink);
            Chapter lastChapter = null;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                foreach (Chapter chapter in chapters)
                {
                    Console.WriteLine("Dang them chap " + chapter.chapName);
                    string addChapterQuery = @"Insert into Chapter (chapName, comicID, chapLink, updateTime) values (N'" +
                            chapter.chapName + "','" + chapter.comicID + "','" + chapter.chapLink + "','" + chapter.updateTime.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                    using (SqlCommand myCommand = new SqlCommand(addChapterQuery, myCon))
                    {
                        myCommand.ExecuteNonQuery();
                    }
                    lastChapter = chapter;
                }
                myCon.Close();
            }

            //Lấy chapID của chap mới nhất vừa được thêm vào
            int newChapID = 0;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand("SELECT MAX(chapID) AS MaxChapID FROM Chapter;", myCon))
                {
                    newChapID = Convert.ToInt32(myCommand.ExecuteScalar());
                    Console.WriteLine(newComicID);
                }
                myCon.Close();
            }

            //Nếu người dùng không điền avatarLink cho truyện, thì tự lọc link lấy link ảnh avatar truyện
            if (String.IsNullOrEmpty(newComicLink.avatarLink))
            {
                dataService.UpdateAvatarLink(newComicID);
                newComicLink.avatarLink = dataService.ComicIDToAvatarLink(newComicID);
            }

            //Thêm chap mới nhất trong danh sách chap vừa quét vào listComicTags
            if (lastChapter != null)
            {
                lastChapter.chapID = newChapID;
                GlobalVariables.listComicTags.AddFirst(new ComicTag(newComicLink.comicName, newComicLink.comicLink, newComicLink.avatarLink, lastChapter));
            }

            //Cập nhật giá trị latestChapID của bản ghi trong ComicLink có comicID là newComicID thành newChapID
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                string updateComicLinkQuery = @"UPDATE ComicLink SET latestChapID = @newChapID WHERE comicID = @newComicID";
                using (SqlCommand myCommand = new SqlCommand(updateComicLinkQuery, myCon))
                {
                    // Sử dụng tham số để tránh lỗi SQL Injection
                    myCommand.Parameters.AddWithValue("@newChapID", newChapID);
                    myCommand.Parameters.AddWithValue("@newComicID", newComicID);

                    int rowsAffected = myCommand.ExecuteNonQuery();
                    Console.WriteLine($"Số hàng được cập nhật: {rowsAffected}");
                }
                myCon.Close();
            }

            //Đoạn code check, có thể xóa
            // Vòng lặp để duyệt qua từng node trong gl.listComicTag
            Console.WriteLine("Size of listComicTag: " + GlobalVariables.listComicTags.Count);
            foreach (ComicTag tag in GlobalVariables.listComicTags)
            {
                // In ra giá trị của chapID và comicID của node hiện tại
                Console.WriteLine("ChapID: " + tag.chapter.chapID + ", ComicID: " + tag.chapter.comicID);
            }


            return new JsonResult("Thêm link mới thành công");
        }





        [HttpPut]
        public JsonResult Put(ComicLink modifiedComicLink)
        {
            string query = "Update ComicLink set comicName = N'" + modifiedComicLink.comicName + "' " + "where comicID = " + modifiedComicLink.comicID;
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

        [HttpDelete("{comicID}")]
        public JsonResult Delete(int comicID)
        {
            //Xóa node trong GlobalVariables.listComicTags có thuộc tính chapter.comicID là comicID
            var node = GlobalVariables.listComicTags.First;
            while (node != null)
            {
                var nextNode = node.Next; // Lưu node tiếp theo trước khi có thể xóa node hiện tại
                if (node.Value.chapter.comicID == comicID)
                {
                    GlobalVariables.listComicTags.Remove(node);
                }
                node = nextNode; 
            }

            string query = "Delete from ComicLink where comicID = @comicID"; // Sử dụng tham số trong truy vấn để tránh SQL Injection
            DataTable table = new DataTable();
            String sqlDataSource = _configuration.GetConnectionString("Comic_Address_Book");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@comicID", comicID); // Thêm tham số vào truy vấn
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
