using System.ComponentModel.DataAnnotations;

namespace UniALPRMain.Models
{
    public class Camera
    {
        public int Id { get; set; }

        [Display(Name = "Веб-адрес видеопотока MJPG")]
        public string Url { get; set; }

        [Display(Name = "Веб-адрес текущего кадра IP-камеры")]
        public string PictureUrl { get; set; }
    }
}
