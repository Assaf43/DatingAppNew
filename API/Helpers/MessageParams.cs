namespace API.Helpers
{
    public class MessageParams : PaginationsParams
    {
        public string Username { get; set; }
        public string Container { get; set; } = "Unread";
    }
}