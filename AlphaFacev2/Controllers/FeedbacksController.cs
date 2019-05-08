using Microsoft.AspNetCore.Mvc;
using AlphaFacev2.Models;
using Microsoft.AspNetCore.Authorization;

namespace AlphaFacev2.Controllers
{
    
    public class FeedbacksController : Controller
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbacksController(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PostFeedback(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _feedbackRepository.AddFeedback(feedback);
                return RedirectToAction("FeedbackComplete");
            }
            return View(feedback);
        }

        public IActionResult PostMessage(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _feedbackRepository.AddFeedback(feedback);
                return RedirectToAction(nameof(Index), "Home");
            }
            return View(nameof(Index), "Home");
        }

        public IActionResult FeedbackComplete()
        {
            return View();
        }
    }
}
