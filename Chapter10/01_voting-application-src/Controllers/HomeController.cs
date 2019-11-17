namespace VotingApplication.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    
    using Ninject.Extensions.Logging;

    using VotingApplication.Interfaces;
    using VotingApplication.Models;

    public class HomeController : Controller
    {
        private readonly VotingApplicationContext db;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ILogger log;

        public HomeController(VotingApplicationContext db, IDateTimeProvider dateTimeProvider, ILogger log)
        {
            this.db = db;
            this.dateTimeProvider = dateTimeProvider;
            this.log = log;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Kubernetes on Windows Voting Application - Home Page";

            var defaultSurvey = this.db.Surveys.FirstOrDefault(s => s.IsDefault) ?? this.db.Surveys.FirstOrDefault();

            if (defaultSurvey == null)
            {
                this.log.Warn("No default survey found");
                return this.View();
            }

            return this.RedirectToAction("Results", "Surveys", new { id = defaultSurvey.Id });
        }
    }
}
