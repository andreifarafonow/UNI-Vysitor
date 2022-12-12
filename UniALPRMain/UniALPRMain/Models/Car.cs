using System.ComponentModel.DataAnnotations;

namespace UniALPRMain.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Display(Name = "Марка ТС")]
        public string Name { get; set; }

        [Display(Name = "Гос. номер")]
        public string Number { get; set; }
    }
}
