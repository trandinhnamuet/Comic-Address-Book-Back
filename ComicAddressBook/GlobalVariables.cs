using ComicAddressBook.Models;
using ComicAddressBook.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace ComicAddressBook
{
    public class GlobalVariables
    {
        /*Trước đây là public static LinkedList<Chapter> listLatestChap = new LinkedList<Chapter>
        tuy nhiên Để hiển thị 1 thẻ truyện trên trang chủ, cần có:
                - comicName
                - comicLink
                - avatarComic
                - chapName     v
                - chapLink     v
                - updateTime   v
                - comicID      v
                - chapID       v
        Do chapter thiếu 3 tham số trên, để khi client gọi trang chủ, chỉ cần trả về linkedlist thay vì phải truy vấn 3 giá trị trên,
        thay listLatestChap bằng listComicTags
        */
        public static LinkedList<ComicTag> listComicTags = new LinkedList<ComicTag>();
        static DataService dataService = new DataService();
        static GlobalVariables()
        {
            InitializeLatestChapters();
        }

        private static void InitializeLatestChapters()
        {
            // Lấy ra danh sách latestChapID từ tất cả bản ghi trong bảng ComicLink
            //// Lấy ra những bản ghi từ bảng Chapter có chapID tương ứng latestChapID
            //// Thêm vào một LinkedList<Chapter> listLatestChap
            using (SqlConnection con = new SqlConnection(dataService.getConnectionString()))
            {
                con.Open();

                // Lấy ra danh sách latestChapID từ tất cả bản ghi trong bảng ComicLink
                string queryGetLatestChapIDs = "SELECT latestChapID FROM ComicLink";
                SqlCommand cmd = new SqlCommand(queryGetLatestChapIDs, con);
                List<int> latestChapIDs = new List<int>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        latestChapIDs.Add(reader.GetInt32(0));
                    }
                }

                // Loại bỏ các chapID bị lặp
                latestChapIDs = latestChapIDs.Distinct().ToList();

                // Lấy ra những bản ghi từ bảng Chapter có chapID tương ứng với latestChapID
                // Thêm những bản ghi đó vào listLatestChap
                foreach (int chapID in latestChapIDs)
                {
                    string queryGetChapters = "SELECT chapID, chapName, comicID, chapLink, updateTime FROM Chapter WHERE chapID = @chapID";
                    SqlCommand cmdGetChapter = new SqlCommand(queryGetChapters, con);
                    cmdGetChapter.Parameters.AddWithValue("@chapID", chapID);

                    using (SqlDataReader reader = cmdGetChapter.ExecuteReader())
                    {
                        if (reader.Read()) // Assuming chapID is unique and will return at most one row
                        {
                            Chapter chapter = new Chapter(
                                reader.GetInt32(reader.GetOrdinal("chapID")),
                                reader.GetString(reader.GetOrdinal("chapName")),
                                reader.GetInt32(reader.GetOrdinal("comicID")),
                                reader.GetString(reader.GetOrdinal("chapLink")),
                                reader.GetDateTime(reader.GetOrdinal("updateTime"))
                            );

                            string comicName = dataService.ComicIDToComicName(chapter.comicID);
                            string comicLink = dataService.ComicIDToComicLink(chapter.comicID);
                            string avatarLink = dataService.ComicIDToAvatarLink(chapter.comicID);

                            listComicTags.AddLast(new ComicTag(comicName, comicLink, avatarLink, chapter));
                        }
                    }
                }

                //Sắp xếp listComicTags theo tag.chapter.updateTime từ mới nhất đến cũ nhất
                // Chuyển LinkedList sang List để có thể sắp xếp
                List<ComicTag> listComicTagsSortable = listComicTags.ToList();

                // Sắp xếp listComicTagsSortable sử dụng LINQ
                listComicTagsSortable = listComicTagsSortable.OrderByDescending(tag => tag.chapter.updateTime).ToList();

                // Bây giờ bạn cần chuyển listComicTagsSortable trở lại dạng LinkedList
                listComicTags = new LinkedList<ComicTag>(listComicTagsSortable);


                Console.WriteLine("Khởi tạo listComicTag: ");
                foreach (ComicTag tag in listComicTags)
                {
                    Console.WriteLine(tag.comicName + " : " + tag.chapter.chapName);
                }
            }


        }
    }
}
