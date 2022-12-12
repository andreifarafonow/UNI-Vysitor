using System.ComponentModel.DataAnnotations;

namespace UniALPRMain.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        public string PhotoBase64 { get; set; }
    }
}
