namespace VotingApplication.Controllers
{
    using System.Net;
    using System.Web.Mvc;

    using VotingApplication.Models;

    public class HealthController : Controller
    {
        private readonly VotingApplicationContext db;

        public HealthController(VotingApplicationContext db)
        {
            this.db = db;
        }

        public ActionResult CheckHealth()
        {
            this.Response.TrySkipIisCustomErrors = true;

            if (!this.db.Database.CompatibleWithModel(throwIfNoMetadata: true))
            {
                this.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return this.Json(new { status = "Database migrations pending" }, JsonRequestBehavior.AllowGet);
            }

            return this.Json(new { status = "Ok" }, JsonRequestBehavior.AllowGet);
        }
    }
}
