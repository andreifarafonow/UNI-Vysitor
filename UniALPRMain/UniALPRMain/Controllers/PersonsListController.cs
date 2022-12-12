using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniALPRMain.Models;

namespace UniALPRMain.Controllers
{
    public class PersonsListController : Controller
    {
        private readonly VysitorDbContext _context;
        private readonly IConfiguration _config;
        private readonly string _faceRecognitionUrl;

        public PersonsListController(VysitorDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

            _faceRecognitionUrl = _config["FaceRecognition"];
        }

        // GET: PersonsList
        public async Task<IActionResult> Index()
        {
              return View(await _context.Persons.ToListAsync());
        }

        // GET: PersonsList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Persons == null)
            {
                return NotFound();
            }

            var person = await _context.Persons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: PersonsList/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: PersonsList/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,ImageFile")] Person person, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                byte[] bytes;
                using (var memoryStream = new MemoryStream())
                {
                    ImageFile.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }

                person.PhotoBase64 = Convert.ToBase64String(bytes);

                _context.Add(person);
                await _context.SaveChangesAsync();


                using (var memoryStream = new MemoryStream())
                {
                    ImageFile.CopyTo(memoryStream);

                    HttpClient httpClient = new HttpClient();

                    string json;

                    using (var multipartFormContent = new MultipartFormDataContent())
                    {
                        var fileStreamContent = new StreamContent(memoryStream);
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");


                        multipartFormContent.Add(new StreamContent(new MemoryStream(Convert.FromBase64String(person.PhotoBase64))), name: "img", fileName: "ttt.jpg");
                        multipartFormContent.Add(new StringContent($"{person.Id}"), "id");

                        var response = httpClient.PostAsync(_faceRecognitionUrl + "upload", multipartFormContent);
                        json = response.Result.Content.ReadAsStringAsync().Result;
                    }
                }


                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }

        // GET: PersonsList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Persons == null)
            {
                return NotFound();
            }

            var person = await _context.Persons.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: PersonsList/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName")] Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }

        // GET: PersonsList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Persons == null)
            {
                return NotFound();
            }

            var person = await _context.Persons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: PersonsList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Persons == null)
            {
                return Problem("Entity set 'VysitorDbContext.Persons'  is null.");
            }
            var person = await _context.Persons.FindAsync(id);
            if (person != null)
            {
                _context.Persons.Remove(person);
            }


            HttpClient httpClient = new HttpClient();

            string json;

            using (var multipartFormContent = new MultipartFormDataContent())
            {
                multipartFormContent.Add(new StringContent($"{person.Id}"), "id");

                var response = httpClient.PostAsync(_faceRecognitionUrl + "delete", multipartFormContent);
                json = response.Result.Content.ReadAsStringAsync().Result;

                int o = 0;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
          return _context.Persons.Any(e => e.Id == id);
        }
    }
}
