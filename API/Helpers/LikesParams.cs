namespace API.Helpers
{
    public class LikesParams : PaginationsParams
    {
        public int UserId { get; set; }
        public string Predicate { get; set; }
    }
}