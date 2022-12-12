using AlprChecker.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace AlprChecker.Controllers
{
    public class HomeController : Controller
    {
        private readonly AlprDbContext _db;
        private readonly IConfiguration _config;
        private Camera[] _cameras;
        private readonly string _carRecognitionUrl;
        private readonly string _personRecognitionUrl;
        private readonly Dictionary<string, string> regions = new Dictionary<string, string>()
        {
            { "Altai Krai", "22" },
            { "Altai Republic", "4" },
            { "Amur Oblast", "28" },
            { "Arkhangelsk Oblast", "29" },
            { "Astrakhan Oblast", "30" },
            { "Belgorod Oblast", "31" },
            { "Bryansk Oblast", "32" },
            { "Chechen Republic", "95" },
            { "Chelyabinsk Oblast", "74" },
            { "Chukotka Autonomous Okrug", "87" },
            { "Chuvash Republic", "21" },
            { "Irkutsk Oblast", "38" },
            { "Jewish Autonomous Oblast", "79" },
            { "Kabardino - Balkar Republic", "7" },
            { "Kaliningrad Oblast", "39" },
            { "Kaluga Oblast", "40" },
            { "Kamchatka Krai", "41" },
            { "Karachay - Cherkess Republic", "9" },
            { "Kemerovo Oblast", "42" },
            { "Khabarovsk Krai", "27" },
            { "Khanty - Mansi Autonomous Okrug", "86" },
            { "Kirov Oblast", "43" },
            { "Komi Republic", "11" },
            { "Kostroma Oblast", "44" },
            { "Krasnodar Krai", "23" },
            { "Krasnoyarsk Krai", "24" },
            { "Kurgan Oblast", "45" },
            { "Kursk Oblast", "46" },
            { "Leningrad Oblast", "47" },
            { "Lipetsk Oblast", "48" },
            { "Magadan Oblast", "49" },
            { "Mari El Republic", "12" },
            { "Moscow", "77" },
            { "Moscow Oblast", "50" },
            { "Murmansk Oblast", "51" },
            { "Nenets Autonomous Okrug(Nenetsia)", "83" },
            { "Nizhny Novgorod Oblast", "52" },
            { "Novgorod Oblast", "53" },
            { "Novosibirsk Oblast", "54" },
            { "Omsk Oblast", "55" },
            { "Orenburg Oblast", "56" },
            { "Oryol Oblast", "57" },
            { "Penza Oblast", "58" },
            { "Perm Krai", "59" },
            { "Primorsky Krai", "25" },
            { "Pskov Oblast", "60" },
            { "Republic of Adygea", "1" },
            { "Republic of Bashkortostan", "2" },
            { "Republic of Buryatia", "3" },
            { "Republic of Crimea / \"Former Koryak Autonomous District\"", "82" },
            { "Republic of Dagestan", "5" },
            { "Republic of Ingushetia", "6" },
            { "Republic of Kalmykia", "8" },
            { "Republic of Karelia", "10" },
            { "Republic of Khakassia", "19" },
            { "Republic of Mordovia", "13" },
            { "Republic of North Ossetia�Alania", "15" },
            { "Republic of Tatarstan ", "16" },
            { "Rostov Oblast", "61" },
            { "Ryazan Oblast", "62" },
            { "Sakhalin Oblast", "65" },
            { "Sakha Republic", "14" },
            { "Samara Oblast", "63" },
            { "Saratov Oblast", "64" },
            { "Sevastopol", "92" },
            { "Smolensk Oblast", "67" },
            { "St. Petersburg", "78" },
            { "Stavropol Krai", "26" },
            { "Sverdlovsk Oblast", "66" },
            { "Tambov Oblast", "68" },
            { "Territories outside of the Russian Federation", "94" },
            { "Tomsk Oblast", "70" },
            { "Tula Oblast", "71" },
            { "Tuva Republic ", "17" },
            { "Tver Oblast", "69" },
            { "Tyumen Oblast", "72" },
            { "Udmurt Republic", "18" },
            { "Ulyanovsk Oblast", "73" },
            { "Vladimir Oblast", "33" },
            { "Volgograd Oblast", "34" },
            { "Vologda Oblast", "35" },
            { "Voronezh Oblast", "36" },
            { "Yamalo - Nenets Autonomous Okrug", "89" },
            { "Yaroslavl Oblast", "76" },
            { "Zabaykalsky Krai", "75" }
        };

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", "frame.jpg");



        public HomeController(AlprDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
            _cameras = db.Cameras.ToArray();

            _carRecognitionUrl = _config["CarRecognition"];
            _personRecognitionUrl = _config["PersonRecognition"];

            Checking();
        }

        private void Checking()
        {
            while(true)
            {
                _cameras = _db.Cameras.ToArray();

                for (int i = 0; i <_cameras.Length; i++)
                {
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            client.DownloadFile(new Uri(_cameras[i].PictureUrl), filePath);
                        }
                        catch(Exception e)
                        {
                            continue;
                        }

                        Thread.Sleep(100);

                        CarCheck();
                        Thread.Sleep(100);
                        PersonCheck();
                    }
                }
                
            }
        }

        private void PersonCheck()
        {
            HttpClient httpClient = new HttpClient();

            using (var multipartFormContent = new MultipartFormDataContent())
            {
                var fileStreamContent = new StreamContent(System.IO.File.OpenRead(filePath));
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                multipartFormContent.Add(fileStreamContent, name: "img", fileName: "frame.png");

                var response = httpClient.PostAsync(_personRecognitionUrl, multipartFormContent);
                string result = "";

                try
                {
                    result = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch
                {

                }

                if (!result.Contains("-1") && !result.ToLower().Contains("error"))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                    string base64 = Convert.ToBase64String(bytes);

                    Event _event = new Event()
                    {
                        Result = Event.ResultType.Confirm,
                        Subject = Event.SubjectType.Person,
                        Time = DateTime.Now,
                        SubjectName = _db.Persons.First(x => x.Id == int.Parse(result.Replace("\"", ""))).FullName,
                        PictureUrl = base64
                    };

                    _db.Events.Add(_event);

                    _db.SaveChanges();
                }
            }
        }

        private void CarCheck()
        {
            HttpClient httpClient = new HttpClient();

            string json;

            using (var multipartFormContent = new MultipartFormDataContent())
            {
                var fileStreamContent = new StreamContent(System.IO.File.OpenRead(filePath));
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                multipartFormContent.Add(fileStreamContent, name: "img", fileName: "frame.png");

                var response = httpClient.PostAsync(_carRecognitionUrl, multipartFormContent);
                json = response.Result.Content.ReadAsStringAsync().Result;
                json = json.Replace("\\", "");
                json = json.Substring(1, json.Length - 3);
            }

            var jObject = JObject.Parse(json);

            string number;
            string state;
            string car;


            if (!json.Contains("state") || !json.Contains("car"))
                return;

            try
            {
                number = jObject["plates"][0]["text"].ToString().Substring(0, 6);
                state = jObject["plates"][0]["country"][0]["state"].ToString();
                car = jObject["plates"][0]["car"]["makeModelYear"][0]["make"].ToString();


                number = jObject["plates"][0]["text"].ToString().Substring(0, 6);
                state = jObject["plates"][0]["country"][0]["state"].ToString();
                car = jObject["plates"][0]["car"]["makeModelYear"][0]["make"].ToString();

                number += regions[state];

                number = EngToRus(number);

                var carsWithNumber = _db.Cars.Where(x => x.Number == number).ToArray();

                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                string base64 = Convert.ToBase64String(bytes);

                Event _event = null;

                if (carsWithNumber.Length == 0)
                {
                    _event = new Event()
                    {
                        Result = Event.ResultType.Сancel,
                        Subject = Event.SubjectType.Car,
                        CarNumber = number,
                        Time = DateTime.Now,
                        PictureUrl = base64
                    };
                }
                else if (!carsWithNumber.Any(x => x.Name.ToLower() == car.ToLower()))
                {
                    _event = new Event()
                    {
                        Result = Event.ResultType.Сancel,
                        Subject = Event.SubjectType.Car,
                        CarNumber = number,
                        Time = DateTime.Now,
                        PictureUrl = base64
                    };
                }
                else
                {
                    _event = new Event()
                    {
                        Result = Event.ResultType.Confirm,
                        Subject = Event.SubjectType.Car,
                        CarNumber = number,
                        Time = DateTime.Now,
                        SubjectName = carsWithNumber.First().Name,
                        PictureUrl = base64
                    };
                }

                var lastEventForNumber = _db.Events.OrderByDescending(x => x.Time).FirstOrDefault(x => x.CarNumber == number);

                if (lastEventForNumber == null || DateTime.Now - lastEventForNumber.Time > TimeSpan.FromSeconds(5))
                {
                    _db.Events.Add(_event);
                    _db.SaveChanges();
                }

            }
            catch
            {
                int o = 0;
            }
        }

        string EngToRus(string source)
        {
            string result = source;

            Dictionary<char, char> lets = new Dictionary<char, char>() 
            {
                {'A', 'А'},
                {'B', 'В'},
                {'E', 'Е'},
                {'K', 'К'},
                {'M', 'М'},
                {'H', 'Н'},
                {'O', 'О'},
                {'P', 'Р'},
                {'C', 'С'},
                {'T', 'Т'},
                {'Y', 'У'},
                {'X', 'Х'}
            };

            foreach(var a in lets)
            {
                result = result.Replace(a.Key, a.Value);
            }

            return result;
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}