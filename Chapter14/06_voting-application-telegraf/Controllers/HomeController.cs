namespace VotingApplication.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Web.Http;
    using System.Web.Mvc;
    
    using Ninject.Extensions.Logging;

    using VotingApplication.Interfaces;
    using VotingApplication.Models;

    public class HomeController : Controller
    {
        private readonly VotingApplicationContext db;
        private readonly ICpuStressWorker cpuStressWorker;
        private readonly IVoteLogManager voteLogManager;
        private readonly ILogger log;

        public HomeController(VotingApplicationContext db, ICpuStressWorker cpuStressWorker, IVoteLogManager voteLogManager, ILogger log)
        {
            this.db = db;
            this.cpuStressWorker = cpuStressWorker;
            this.voteLogManager = voteLogManager;
            this.log = log;
        }

        public ActionResult Index()
        {
            this.ViewBag.Title = "Kubernetes on Windows Voting Application - Home Page";

            var defaultSurvey = this.db.Surveys.FirstOrDefault(s => s.IsDefault) ?? this.db.Surveys.FirstOrDefault();

            if (defaultSurvey == null)
            {
                this.log.Warn("No default survey found");
                return this.View();
            }

            return this.RedirectToAction("Results", "Surveys", new { id = defaultSurvey.Id });
        }

        public ActionResult StressCpu([FromUri] int value)
        {
            this.Response.StatusCode = (int)HttpStatusCode.Accepted;
            var host = Dns.GetHostEntry(string.Empty).HostName;

            if (value < 0)
            {
                this.cpuStressWorker.Disable();
                return this.Json(new { host, status = $"Stressing CPU turned off" }, JsonRequestBehavior.AllowGet);
            }

            if (value > 100)
            {
                value = 100;
            }

            this.cpuStressWorker.Enable(value);
            return this.Json(new { host, status = $"Stressing CPU at {value}% level" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VotingLog()
        {
            this.ViewBag.Title = "Kubernetes on Windows Voting Application - Voting Log";
            this.ViewBag.VotingLog = this.voteLogManager.Get();

            return this.View();
        }

        public ActionResult TestException()
        {
            throw new InvalidOperationException("This action always throws an exception!");
        }
    }
}
