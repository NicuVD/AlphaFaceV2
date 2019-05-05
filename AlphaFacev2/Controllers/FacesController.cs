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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;

namespace AlphaFacev2.Controllers
{
    public class FacesController : Controller
    {
        const string subscriptionKey = "9d1d213b82b440e881893324ed33cab9";
        const string uriBase = "https://northeurope.api.cognitive.microsoft.com/face/v1.0/detect";
        private readonly AppDbContext _context;
        private readonly CognitiveServices _cognitiveServices;
        private readonly AccountServices _accountServices;
        private readonly IHostingEnvironment _environment;

        [TempData]
        public string StatusMessage { get; set; }

        public FacesController(IHostingEnvironment hostingEnvironment, AppDbContext context, CognitiveServices cognitiveServices, AccountServices accountServices)
        {
            _environment = hostingEnvironment;
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

        public async Task<IActionResult> ComparisonResults()
        {
            return View(await _context.Face.OrderByDescending(f => f.Id).ToListAsync());
        }

        // GET: Faces

        public IActionResult Index()
        {
            // grab user profile picture
            var user = _accountServices.GetCurrentUser();
            Image userImage = new Image();
            try
            {
                userImage.ImageByteArray = user.ProfileImage;
                userImage.ImageBase64 = ConvertByteArrayToBase64(userImage.ImageByteArray);
                userImage.ImageSource = ReturnImageSource(userImage.ImageBase64);
            }
            catch (Exception)
            {
                //throw;
            }

            // grab incoming snapshot from the webcam and create a new Image object with the data
            //var webcamImageFromDataBase = await _context.ImageStore.LastOrDefaultAsync(i => i.ProfileId == user.Id);

            Image webcamImage = new Image();
            //if (webcamImageFromDataBase.ImageByteArray != null)
            //{
            //    webcamImage.ImageByteArray = webcamImageFromDataBase.ImageByteArray;
            //    webcamImage.ImageBase64 = ConvertByteArrayToBase64(webcamImageFromDataBase.ImageByteArray);
            //    webcamImage.ImageSource = ReturnImageSource(webcamImage.ImageBase64);
            //}

            List<Image> images = new List<Image>();
            images.Add(userImage);
            images.Add(webcamImage);

            return View(images);
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

        private static string ReturnImageSource(string base64)
        {
            return String.Format("data:image/jpg;base64,{0}", base64);
        }

        public string ConvertByteArrayToBase64(byte[] byteArray)
        {
            var base64 = Convert.ToBase64String(byteArray);
            return base64;
        }

        public byte[] ConvertBase64ToByteArray(string base64Image)
        {
            var byteArray = Convert.FromBase64String(base64Image);
            return byteArray;
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

        [HttpPost]
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

        [HttpPost]
        public async Task<IActionResult> CompareToWebcamPicture()
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

                ImageStore webcamImageStore = new ImageStore();
                webcamImageStore = await _context.ImageStore.LastOrDefaultAsync(i => i.ProfileId == userProfile.Id);
                

                byte[] webcamImage = webcamImageStore.ImageByteArray;

                Stream userImage = new MemoryStream(userImageByteArray);
                Stream uploadedImage = new MemoryStream(webcamImage);

                var result = await _cognitiveServices.VerifyAsync(userImage, uploadedImage);

                if (result != null)
                {
                    Face faceComparison = new Face()
                    {
                        ProfileId = userProfile.Id,
                        ProfileImage = userProfile.ProfileImage,
                        ComparisonImage = webcamImageStore.ImageByteArray,
                        IsIdentical = result.IsIdentical,
                        Confidence = result.Confidence
                    };

                    _context.Face.Add(faceComparison);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("ComparisonResults");
            }

            return RedirectToAction("Compare", "Faces");
        }

        //[HttpPost]
        //public async Task<IActionResult> ComparePictures(IList<IFormFile> files)
        //{
        //    var firstFile = files.First();
        //    var secondFile = files.Last();

        //    byte[] firstUploadedImageByteArray = await GetUploadedPicture(firstFile);
        //    byte[] secondUploadedImageByteArray = await GetUploadedPicture(secondFile);

        //    Stream firstImage = new MemoryStream(firstUploadedImageByteArray);
        //    Stream secondImage = new MemoryStream(secondUploadedImageByteArray);

        //    var result = await _cognitiveServices.VerifyAsync(firstImage, secondImage);

        //    return View("ComparisonResults", result);
        //}

        [HttpPost]
        public async Task<IActionResult> ComparePictures(IList<byte[]> byteArrays)
        {
            byte[] firstUploadedImageByteArray = byteArrays.First();
            byte[] secondUploadedImageByteArray = byteArrays.Last();

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

        public IActionResult Capture()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Capture(string name)
        {
            var lastLogin = _context.History.LastOrDefault();
            var lastLoggedUser = _context.Profile.FirstOrDefault(u => u.UserName == lastLogin.Username);
            Profile loggedUser = new Profile();
            if (lastLoggedUser != null)
            {
                if (lastLoggedUser.IsLoggedIn)
                {
                    loggedUser = lastLoggedUser;
                }
            }

            var files = HttpContext.Request.Form.Files;
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Getting Filename  
                        var fileName = file.FileName;
                        // Unique filename "Guid"  
                        var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                        // Getting Extension  
                        var fileExtension = Path.GetExtension(fileName);
                        // Concating filename + fileExtension (unique filename)  
                        var newFileName = string.Concat(myUniqueFileName, fileExtension);
                        //  Generating Path to store photo   
                        var filepath = Path.Combine(_environment.WebRootPath, "CameraPhotos") + $@"\{newFileName}";

                        if (!string.IsNullOrEmpty(filepath))
                        {
                            // Storing Image in Folder  
                            StoreInFolder(file, filepath);
                        }

                        var imageBytes = System.IO.File.ReadAllBytes(filepath);
                        if (imageBytes != null)
                        {
                            // Storing Image in Folder  
                            StoreInDatabase(imageBytes, loggedUser);
                        }
                    }
                }
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

       public IActionResult CaptureAndCompare(string name)
        {
            var lastLogin = _context.History.LastOrDefault();
            var lastLoggedUser = _context.Profile.FirstOrDefault(u => u.UserName == lastLogin.Username);
            Profile loggedUser = new Profile();

            if (lastLoggedUser.IsLoggedIn)
            {
                loggedUser = lastLoggedUser;
            }

            var file = HttpContext.Request.Form.Files.FirstOrDefault();
            if ((file != null) && (file.Length > 0))
            {
                // Getting Filename  
                var fileName = file.FileName;
                // Unique filename "Guid"  
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                // Getting Extension  
                var fileExtension = Path.GetExtension(fileName);
                // Concating filename + fileExtension (unique filename)  
                var newFileName = string.Concat(myUniqueFileName, fileExtension);
                //  Generating Path to store photo   
                var filepath = Path.Combine(_environment.WebRootPath, "CameraPhotos") + $@"\{newFileName}";

                var imageBytes = System.IO.File.ReadAllBytes(filepath);
                if (imageBytes != null)
                {
                    // Storing Image in Folder  
                    StoreInDatabase(imageBytes, loggedUser);
                }

                List<byte[]> images = new List<byte[]>();
                images.Add(loggedUser.ProfileImage);
                images.Add(imageBytes);

                return RedirectToAction("ComparePictures", images);
            }

            return View("Index");
        }

        [HttpGet]
        public IActionResult CompareDisplayedPictures()
        {
            if (ModelState.IsValid)
            {

            }
            return View(); // Wrong!
        }

        /// <summary>  
        /// Saving captured image into Folder.  
        /// </summary>  
        /// <param name="file"></param>  
        /// <param name="fileName"></param>  
        private void StoreInFolder(IFormFile file, string fileName)
        {
            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }

        /// <summary>  
        /// Saving captured image into database.  
        /// </summary>  
        /// <param name="imageBytes"></param>  
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
