namespace ComicAddressBook.Models
{
    public class Chapter
    {
        public int chapID { get; set; }
        public string chapName { get; set; }
        public int comicID { get; set; }
        public string chapLink { get; set; }
        public DateTime updateTime { get; set; }

        // Constructor
        public Chapter()
        {
            // Khởi tạo giá trị mặc định cho các thuộc tính (nếu cần)
            chapID = 0;
            chapName = "chap null";
            comicID = 0;
            chapLink = string.Empty;
            updateTime = DateTime.Now;
        }

        // Constructor có tham số
        public Chapter(int chapID, string chapName, int comicID, string chapLink, DateTime updateTime)
        {
            this.chapID = chapID;
            this.chapName = chapName;
            this.comicID = comicID;
            this.chapLink = chapLink;
            this.updateTime = updateTime;
        }
    }
}
