namespace AlprChecker.Models
{
    public class Event
    {
        public int Id { get; set; }

        public enum SubjectType
        {
            Car,
            Person
        }

        public enum ResultType
        {
            Confirm,
            Сancel
        }

        public SubjectType Subject { get; set; }

        public ResultType Result { get; set; }

        public DateTime Time { get; set; }

        public string SubjectName { get; set; }

        public string CarNumber { get; set; }

        public string PictureUrl { get; set; }
    }
}
