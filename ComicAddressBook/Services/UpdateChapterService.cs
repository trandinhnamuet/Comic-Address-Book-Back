using ComicAddressBook.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ComicAddressBook.Services
{
    public class UpdateChapterService
    {
        DataService dataService = new DataService();
        /*
         * Daily scan: Hàm check các truyện có trong csdl, có truyện nào có chap mới không
         * GlobalNewChapterScan -> duyệt GlobalVariables.listLatestChap, với mỗi node gọi NewChapterScan kiểm tra chap mới
         * Nếu có chap mới -> Xóa node hiện tại, thêm chap mới làm node mới vào đầu GlobalVariables.listLatestChap.
         * Nếu không có -> duyệt node tiếp theo
         */

        string[] firstArgs = new string[] { 
            "//div[@class='list-chapter' and @id='nt_listchapter']", 
            "//div[@class='list-wrap' and @id='list-chapters']",
            "//div[@class='works-chapter-list']",
            "//div[@id='nt_listchapter']",
            "//div[@class='ul-list-chaper-detail-commic']",
            "//table[@class='table table-striped']",
            "//div[@id='video-title']"};
        string[] secondArgs = new string[] { ".//a", ".//a", ".//a", ".//a", ".//a", ".//a", ".//a" };
        string[] thirdArgs = new string[] { 
            "", 
            "https://blogtruyenmoi.com", 
            "",
            "", 
            "",
            "",
            ""};

        public void ScheduledScan(int interval)
        {
            while (true)
            {
                Console.WriteLine("Current time: " + DateTime.Now);
                GlobalNewChapterScan();
                Thread.Sleep(interval);
            }
        }
        public void GlobalNewChapterScan()
        {
            Console.WriteLine("Size of listlatestchap: " + GlobalVariables.listComicTags.Count);

            var currentNode = GlobalVariables.listComicTags.First;
            while (currentNode != null)
            {
                Console.WriteLine("Scanning new chapter for: " + currentNode.Value.chapter.chapLink);
                var chapter = NewChapterScan(currentNode.Value.chapter);
                if (chapter != null )
                {
                    var oldNodeValue = currentNode.Value;
                    //Xóa chapter đang duyệt khỏi linkedList
                    var nextNode = currentNode.Next;
                    GlobalVariables.listComicTags.Remove(currentNode);
                    currentNode = nextNode;

                    //Thêm chapter mới vào đầu linkedList
                    GlobalVariables.listComicTags.AddFirst(new ComicTag(oldNodeValue.comicName, oldNodeValue.comicLink, oldNodeValue.avatarLink, chapter));

                    Console.WriteLine("Phát hiện chap mới: " +  chapter.chapLink + "\n------------------------------------------------");
                } else
                {
                    currentNode = currentNode.Next;
                }
            }
        }

        //Lưu ý trường hợp quét xong phát hiện tới 2 chap hoặc 3 hoặc nhiều hơn chap mới cùng lúc
        //Trường hợp xóa chap thì sao?
        //Check gặp trường hợp này hay không trong hàm scan của web đó
        //Các Web trả về html full: nettruyen, blogtruyen, truyenqq
        //Các Web lấy list chapter bằng js sau khi trả về html thiếu: baotangtruyen, vlogtruyen, cmanga
        public Chapter? NewChapterScan(Chapter chapter)
        {
            //Duyệt link xem là web nào, với mỗi web gọi hàm riêng
            switch (chapter.chapLink)
            {
                //Type 1 Web: các web trả về list chap trong html
                case string link when link.Contains("nettruyen"):
                    return ScanType1Web(chapter, 0);
                case string link when link.Contains("blogtruyen"):
                    return ScanType1Web(chapter, 1);

                case string link when link.Contains("truyenqq"):
                    return ScanType1Web(chapter, 2);

                //Type 2 Web: các web trả về list chap bằng javascript, chứ html code không chứa list chap
                case string link when link.Contains("baotangtruyen"):
                    return ScanType2Web(chapter, 3);
                case string link when link.Contains("vlogtruyen"):
                    return ScanType2Web(chapter, 4);
                case string link when link.Contains("nettrom"):                    //cuu truyen
                    return ScanNettrom(chapter);


                case string link when link.Contains("vivicomi"):         //https://vivicomi.info/           == tu sach nho
                    return ScanType1Web(chapter, 5);

                //Type 3 Web: Youtube
                case string link when link.Contains("youtube"):      //youtube playlist : https://www.youtube.com/playlist?list=PLLYpSDNhmyDiioz2CefunCLZOKbhNWDW5
                    return ScanType2Web(chapter, 6);
                    //return ScanYoutubePlaylist(chapter);
                case string link when link.Contains("cuutruyen"):   
                    return ScanCuutruyen(chapter);
                case string link when link.Contains("cmanga"): //chua ho tro cmanga
                    return ScanType1Web(chapter, 12);
                default:
                    // Xử lý nếu không khớp với bất kỳ trường hợp nào
                    Console.WriteLine("Link truyện chưa hỗ trợ daily scan new chap." + chapter.chapLink);
                    return null;
            }   
        }

        //Lấy code HTML, check chap mới nhất có phải là chap được truyền vào hay không
        //Nếu có -> không có chap mới -> trả về null
        //Nếu không -> Lấy danh sách chap thành list, tìm node trùng với chap được truyền vào, loại bỏ chap đó và các chap cũ hơn. Các chap không bị loại bỏ là
        //các chap mới. Thêm chúng vào csdl rồi trả về chap mới nhất
        public Chapter? ScanType1Web(Chapter chapter, int sn) //support number
        {
            //Lấy từ bảng ComicLink giá trị comicLink của bản ghi có comicID = chapter.comicID lưu vào biến string linkOfComicOfThisChap
            string linkOfComicOfThisChap = dataService.ComicIDToComicLink(chapter.comicID);
            Console.WriteLine("Đang quét: " + linkOfComicOfThisChap);
            // Tạo một đối tượng HtmlWeb để tải về nội dung của trang web, Tải về nội dung của trang web vào một đối tượng HtmlDocument, Lấy mã HTML của trang web, Đọc file html bằng HtmlDocument
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc;
            try
            {
                doc = web.Load(linkOfComicOfThisChap);
            } catch (Exception ex) { return null; }
            string page = doc.DocumentNode.OuterHtml;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);
            //Tìm thẻ div có class list-chapter và id nt_listchapter
            HtmlNode listChapterNode = htmlDoc.DocumentNode.SelectSingleNode(firstArgs[sn]);
            if (listChapterNode != null)
            {
                LinkedList<string> chaptersList = new LinkedList<string>();
                HtmlNodeCollection aNodes = listChapterNode.SelectNodes(secondArgs[sn]);
                //Kiểm tra liNode đầu tiên, nếu href của thẻ a trùng với chapter.chapLink thì return null
                if (aNodes != null && aNodes.Count > 0)
                {
                    // Lấy ra node đầu tiên trong danh sách
                    HtmlNode firstANode = aNodes[0];
                    // Lấy giá trị href từ thẻ a
                    string hrefValue = thirdArgs[sn] + firstANode.GetAttributeValue("href", string.Empty);
                    // So sánh giá trị href với chapter.chapLink
                    if (hrefValue.Equals(chapter.chapLink))
                    {
                        // Nếu giá trị href trùng với chapter.chapLink, trả về null
                        return null;
                    }
                    else
                    {
                        // Chap mới nhất không trùng chap được truyền vào -> có chap mới -> 
                        // Lấy danh sách chap từ liNodes. Duyệt từng node, nếu node đó có href của thẻ a khác chapter.chapLink
                        // thì bỏ qua cho đến khi gặp node có href của thẻ a trùng chapter.chapLink.
                        // Thêm các chap sau chap đó trở đi vào làm bản ghi mới cho bảng Chapter. Chap cuối cùng được thêm vào,
                        // cập nhật trường latestChapID của bản ghi comicID trong comicLink là chapID của chap đó, và hàm return chap cuối cùng đó

                        // Biến để kiểm tra xem đã tìm thấy chap trùng với chap hiện tại hay chưa
                        bool foundCurrentChapter = false;
                        string newChapLink = "Khoi tao bien newChapLink trong servive";
                        string newChapName = "Khoi tao bien newChapName trong servive";

                        aNodes.Reverse();

                        // Duyệt từng node trong danh sách chap
                        foreach (HtmlNode aNode in aNodes)
                        {
                            newChapLink = thirdArgs[sn] + aNode.Attributes["href"].Value;
                            newChapName = aNode.InnerText.Trim();

                            if (!foundCurrentChapter)
                            {
                                // Kiểm tra xem href của chap có khác với chap được truyền vào không. Nếu có thì từ chap sau bỏ qua khối code này.
                                if (newChapLink.Equals(chapter.chapLink))
                                {
                                    foundCurrentChapter = true;
                                }
                            }
                            else
                            {
                                // Thêm các chap sau chap hiện tại vào cơ sở dữ liệu
                                dataService.AddChapterToDatabase(newChapName, newChapLink, chapter.comicID);
                            }
                        }
                        // Cập nhật trường latestChapID của bản ghi comicID trong bảng comicLink là chapID của chap cuối cùng được thêm vào
                        dataService.UpdateLatestChapterInDatabase(chapter.comicID, dataService.GetLatestChapID());
                        // Trả về chap cuối cùng được thêm vào
                        return new Chapter(0, newChapName, chapter.comicID, newChapLink, DateTime.Now);

                    }
                }
                else Console.WriteLine("Link này không có chap nào");
            }
            else
            {
                Console.WriteLine("Không tìm thấy danh sách chương.");
            }
            Console.WriteLine("Luồng hoạt động scan new chap bất thường:" + chapter.chapLink);
            return null;
        }

        public Chapter? ScanType2Web(Chapter chapter, int sn)
        {
            //Lấy từ bảng ComicLink giá trị comicLink của bản ghi có comicID = chapter.comicID lưu vào biến string linkOfComicOfThisChap
            string linkOfComicOfThisChap = dataService.ComicIDToComicLink(chapter.comicID);
            Console.WriteLine("Đang quét: " + linkOfComicOfThisChap);

            // Đường dẫn tới ChromeDriver
            var driverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            // Thiết lập cho trình duyệt chạy ở chế độ headless nếu muốn
            options.AddArguments("headless");
            string page = "";
            using (var driver = new ChromeDriver(driverService, options))
            {
                // Điều hướng đến trang web
                Console.WriteLine("Link Comic Chap:" + linkOfComicOfThisChap);
                driver.Navigate().GoToUrl(linkOfComicOfThisChap);
                // Chờ đợi một thời gian cho trang web hoàn tất tải và xử lý JavaScript
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                // Lấy mã HTML của trang web lưu vào string page
                page = driver.PageSource;
            }

            // Tạo một đối tượng HtmlWeb để tải về nội dung của trang web, Tải về nội dung của trang web vào một đối tượng HtmlDocument, Lấy mã HTML của trang web, Đọc file html bằng HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            //Tìm thẻ div có class list-chapter và id nt_listchapter
            HtmlNode listChapterNode = htmlDoc.DocumentNode.SelectSingleNode(firstArgs[sn]);
            if (listChapterNode != null)
            {
                LinkedList<string> chaptersList = new LinkedList<string>();
                HtmlNodeCollection aNodes = listChapterNode.SelectNodes(secondArgs[sn]);
                //Kiểm tra liNode đầu tiên, nếu href của thẻ a trùng với chapter.chapLink thì return null
                if (aNodes != null && aNodes.Count > 0)
                {
                    // Lấy ra node đầu tiên trong danh sách
                    HtmlNode firstANode = aNodes[0];
                    // Lấy giá trị href từ thẻ a
                    string hrefValue = firstANode.GetAttributeValue("href", string.Empty);
                    // So sánh giá trị href với chapter.chapLink
                    if (hrefValue.Equals(chapter.chapLink))
                    {
                        // Nếu giá trị href trùng với chapter.chapLink, trả về null
                        return null;
                    }
                    else
                    {
                        // Chap mới nhất không trùng chap được truyền vào -> có chap mới -> 
                        // Lấy danh sách chap từ liNodes. Duyệt từng node, nếu node đó có href của thẻ a khác chapter.chapLink
                        // thì bỏ qua cho đến khi gặp node có href của thẻ a trùng chapter.chapLink.
                        // Thêm các chap sau chap đó trở đi vào làm bản ghi mới cho bảng Chapter. Chap cuối cùng được thêm vào,
                        // cập nhật trường latestChapID của bản ghi comicID trong comicLink là chapID của chap đó, và hàm return chap cuối cùng đó

                        // Biến để kiểm tra xem đã tìm thấy chap trùng với chap hiện tại hay chưa
                        bool foundCurrentChapter = false;
                        string newChapLink = "Khoi tao bien newChapLink trong servive";
                        string newChapName = "Khoi tao bien newChapName trong servive";
                        // Duyệt từng node trong danh sách chap
                        foreach (HtmlNode aNode in aNodes)
                        {
                            newChapLink = aNode.Attributes["href"].Value;
                            newChapName = aNode.InnerText.Trim();


                            if (!foundCurrentChapter)
                            {
                                // Kiểm tra xem href của chap có khác với chap được truyền vào không. Nếu có thì từ chap sau bỏ qua khối code này.
                                if (newChapLink.Equals(chapter.chapLink))
                                {
                                    foundCurrentChapter = true;
                                }
                            }
                            else
                            {
                                // Thêm các chap sau chap hiện tại vào cơ sở dữ liệu
                                dataService.AddChapterToDatabase(newChapName, newChapLink, chapter.comicID);
                            }
                        }

                        // Cập nhật trường latestChapID của bản ghi comicID trong bảng comicLink là chapID của chap cuối cùng được thêm vào
                        dataService.UpdateLatestChapterInDatabase(chapter.comicID, dataService.GetLatestChapID());

                        // Trả về chap cuối cùng được thêm vào
                        return new Chapter(0, newChapName, chapter.comicID, newChapLink, DateTime.Now);

                    }
                }
                else Console.WriteLine("Link này không có chap nào");

            }
            else
            {
                Console.WriteLine("Không tìm thấy danh sách chương.");
            }
            Console.WriteLine("Luồng hoạt động scan new chap bất thường");
            return null;
        }

        public Chapter? ScanNettrom(Chapter chapter)
        {
            // Lấy link của truyện từ bảng ComicLink dựa vào comicID của chương
            string linkOfComicOfThisChap = dataService.ComicIDToComicLink(chapter.comicID);
            Console.WriteLine("Đang quét: " + linkOfComicOfThisChap);

            // Cấu hình cho ChromeDriver
            var driverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            options.AddArguments("headless"); // Chạy trình duyệt ở chế độ không giao diện nếu cần
            using (var driver = new ChromeDriver(driverService, options))
            {
                Console.WriteLine("Link Comic Chap:" + linkOfComicOfThisChap);
                driver.Navigate().GoToUrl(linkOfComicOfThisChap);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                var listChapterDiv = driver.FindElements(By.CssSelector("div.vue-recycle-scroller__item-wrapper > div > div"));
                LinkedList<Chapter> chaptersList = new LinkedList<Chapter>();

                foreach (var chapterDiv in listChapterDiv)
                {
                    var anchor = chapterDiv.FindElement(By.CssSelector("a"));
                    var newChapLink = anchor.GetAttribute("href");

                    // Kiểm tra nếu link chương hiện tại trùng với link chương mới thì không làm gì cả
                    if (newChapLink == chapter.chapLink)
                    {
                        return null;
                    }

                    var chapterNumber = chapterDiv.FindElement(By.CssSelector("div.p-1.flex.font-bold.text-gray-800 > span:last-child")).Text.Trim();
                    var chapterTitle = chapterDiv.FindElement(By.CssSelector("div.p-2 > div.truncate")).Text.Trim();
                    var newChapName = $"Chương {chapterNumber}. {chapterTitle}";

                    // Tạo mới đối tượng Chapter và thêm vào danh sách
                    var newChapter = new Chapter(0, newChapName, chapter.comicID, newChapLink, DateTime.Now);
                    chaptersList.AddFirst(newChapter);
                }

                // Thêm từng chương mới vào cơ sở dữ liệu
                foreach (var newChapter in chaptersList)
                {
                    dataService.AddChapterToDatabase(newChapter.chapName, newChapter.chapLink, chapter.comicID);
                }

                // Cập nhật latestChapID cho bản ghi comicID trong bảng ComicLink
                // Giả sử ở đây chaptersList.Last.Value là chương cuối cùng được thêm vào
                var lastChapter = chaptersList.Last?.Value;
                if (lastChapter != null)
                {
                    dataService.UpdateLatestChapterInDatabase(chapter.comicID, dataService.GetLatestChapID());
                }

                return lastChapter; // Trả về chương cuối cùng được thêm vào
            }
        }


        public Chapter? ScanCuutruyen(Chapter chapter)
        {
            return ScanNettrom(chapter);
        }

        public Chapter? ScanVivicomi(Chapter chapter)
        {
            return null;
        }

        public Chapter? ScanYoutubePlaylist(Chapter chapter)
        {
            return null;
        }

        /*public Chapter ScanTruyenqq(Chapter chapter)
        {
            return null;
        }

        public Chapter ScanTruyenqq(Chapter chapter)
        {
            return null;
        }*/
    }
}
