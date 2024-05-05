namespace ComicAddressBook.Models
{
    public class ComicTag
    {
        public string comicName { get; set; }
        public string comicLink { get; set; }
        public string avatarLink { get; set; }
        public Chapter chapter { get; set; }

        // Constructor
        public ComicTag()
        {
            // Khởi tạo giá trị mặc định cho các thuộc tính (nếu cần)
            comicName = "null name";
            comicLink = "null link";
            avatarLink = "null avatar";
            chapter = new Chapter();
        }

        public ComicTag(string comicName, string comicLink, string avatarLink, Chapter chapter) {
            this.comicName = comicName;
            this.comicLink = comicLink;
            this.avatarLink = avatarLink;
            this.chapter = chapter;
        }
    }
}
