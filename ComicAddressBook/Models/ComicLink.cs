namespace ComicAddressBook.Models
{
    public class ComicLink
    {
        public int comicID { get; set; }
        public string comicName { get; set; }
        public string comicLink { get; set; }
        public string translator { get; set; }
        public int accountID { get; set; }
        public string alternativeName { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public int latestChapID { get; set; } 
        public DateTime latestUpdateTime { get; set; } // Thêm thuộc tính latestUpdateTime kiểu DateTime
        public string avatarLink { get; set; }
    }
}
