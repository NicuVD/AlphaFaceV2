using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlphaFacev2.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.IO;
using AlphaFacev2.Services;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;

namespace AlphaFacev2.Controllers
{
    public class FacesController : Controller
    {
        const string subscriptionKey = "9d1d213b82b440e881893324ed33cab9";
        const string uriBase = "https://northeurope.api.cognitive.microsoft.com/face/v1.0/detect";
        private readonly AppDbContext _context;
        private readonly CognitiveServices _cognitiveServices;
        private readonly AccountServices _accountServices;

        [TempData]
        public string StatusMessage { get; set; }

        public FacesController(AppDbContext context, CognitiveServices cognitiveServices, AccountServices accountServices)
        {
            _context = context;
            _cognitiveServices = cognitiveServices;
            _accountServices = accountServices;
        }

        public IActionResult Face()
        {
            return View();
        }

        public IActionResult Compare()
        {
            return View();
        }

        public IActionResult ComparisonResults()
        {
            return View();
        }

        // GET: Faces

        public async Task<IActionResult> Index()
        {
            return View(await _context.Face.ToListAsync());
        }

        // GET: Faces/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var face = await _context.Face
                .FirstOrDefaultAsync(m => m.Id == id);
            if (face == null)
            {
                return NotFound();
            }

            return View(face);
        }

        // GET: Faces/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Faces/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProfileId,Accuracy,Gender,FaceGuid")] Face face)
        {
            if (ModelState.IsValid)
            {
                _context.Add(face);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(face);
        }

        // GET: Faces/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var face = await _context.Face.FindAsync(id);
            if (face == null)
            {
                return NotFound();
            }
            return View(face);
        }

        // POST: Faces/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProfileId,Accuracy,Gender,FaceGuid")] Face face)
        {
            if (id != face.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(face);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FaceExists(face.Id))
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
            return View(face);
        }

        // GET: Faces/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var face = await _context.Face
                .FirstOrDefaultAsync(m => m.Id == id);
            if (face == null)
            {
                return NotFound();
            }

            return View(face);
        }

        // POST: Faces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var face = await _context.Face.FindAsync(id);
            _context.Face.Remove(face);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FaceExists(int id)
        {
            return _context.Face.Any(e => e.Id == id);
        }

        public async Task<IActionResult> PostProfilePicture(IFormFile file)
        {
            var lastLogin = _context.History.Last(l => l.IsActionSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

            // Added for reading the file as byte array an updating the database
            byte[] imageByteArray = await GetUploadedPicture(file);

            var profileToUpdate = _context.Profile.FirstOrDefault(p => p.UserName == user.UserName);
            profileToUpdate.ProfileImage = imageByteArray;
            _context.Update<Profile>(profileToUpdate);
            await _context.SaveChangesAsync();
            // -----------------------------------------------
            StatusMessage = "Your profile picture has been updated";
            //return RedirectToRoute("Index","/Identity/Account/Manage");
            return RedirectToAction("Index", "Profiles");
        }

        private static async Task<byte[]> GetUploadedPicture(IFormFile file)
        {
            byte[] imageByteArray = null;

            using (var client = new HttpClient())
            {
                HttpContent content = new StreamContent(file.OpenReadStream());
                imageByteArray = await content.ReadAsByteArrayAsync(); // serialize the http content to a byte array
            }

            return imageByteArray;
        }

        public async Task<IActionResult> CompareProfilePictures(IFormFile file)
        {
            var lastLogin = _context.History.Last(l => l.IsUserLoggedIn == true);

            if (lastLogin != null)
            {
                var userProfile = await _context.Profile.FirstOrDefaultAsync(p => p.UserName == lastLogin.Username);
                byte[] userImageByteArray = new byte[] { 0 };

                if (userProfile.ProfileImage != null)
                {
                    userImageByteArray = userProfile.ProfileImage;
                }
                else
                {
                    return RedirectToAction("Details", "Profiles");
                    // return with message "No profile picture stored for this user."
                }


                byte[] uploadedImageByteArray = await GetUploadedPicture(file);

                Stream userImage = new MemoryStream(userImageByteArray);
                Stream uploadedImage = new MemoryStream(uploadedImageByteArray);

                var result = await _cognitiveServices.VerifyAsync(userImage, uploadedImage);

                return View("ComparisonResults", result);
            }

            return RedirectToAction("Compare", "Faces");
        }

        public async Task<IActionResult> ComparePictures(IList<IFormFile> files)
        {
            var firstFile = files.First();
            var secondFile = files.Last();

            byte[] firstUploadedImageByteArray = await GetUploadedPicture(firstFile);
            byte[] secondUploadedImageByteArray = await GetUploadedPicture(secondFile);

            Stream firstImage = new MemoryStream(firstUploadedImageByteArray);
            Stream secondImage = new MemoryStream(secondUploadedImageByteArray);

            var result = await _cognitiveServices.VerifyAsync(firstImage, secondImage);

            return View("ComparisonResults", result);
        }

        [HttpPost]
        public async Task<IActionResult> ExtractFaceMetrics(IFormFile file)
        {
            // full path to file in temp location
            //var imageFilePath = Path.GetTempFileName();
            //var imageByteArray = Utilities.GetImageAsByteArray(imageFilePath);


            string requestParameters = "returnFaceId=false&returnFaceLandmarks=false&returnFaceAttributes=Smile&recognitionModel=recognition_01";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            string result = null;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                HttpContent content = new StreamContent(file.OpenReadStream());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var imageByteArray = await content.ReadAsByteArrayAsync(); // serialize the http content to a byte array

                //var response = await client.PostAsync(uri, content);
                var response = await client.PostAsync(uri, content);
                result = await response.Content.ReadAsStringAsync();
                result += "\n Image bytearray: \n" + imageByteArray.Length;
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var user = _accountServices.GetCurrentUser();

            byte[] imageByteArray = await GetUploadedPicture(file);
            StoreInDatabase(imageByteArray, user);

            return RedirectToAction("Index", "Faces");
        }

        private void StoreInDatabase(byte[] imageBytes, Profile profile)
        {
            try
            {
                if (imageBytes != null)
                {
                    string base64String = Convert.ToBase64String(imageBytes, 0, imageBytes.Length);
                    string imageUrl = string.Concat("data:image/jpg;base64,", base64String);
                    ImageStore imageStore = new ImageStore()
                    {
                        CreateDate = DateTime.Now,
                        ImageBase64String = imageUrl,
                        ImageId = 0,
                        ProfileId = profile.Id, // added
                        ImageByteArray = imageBytes
                    };
                    _context.ImageStore.Add(imageStore);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
