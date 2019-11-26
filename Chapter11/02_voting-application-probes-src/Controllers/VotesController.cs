namespace VotingApplication
{
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Ninject.Extensions.Logging;

    using VotingApplication.Interfaces;
    using VotingApplication.Models;

    public class VotesController : Controller
    {
        private readonly VotingApplicationContext db;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ILogger log;

        public VotesController(VotingApplicationContext db, IDateTimeProvider dateTimeProvider, ILogger log)
        {
            this.db = db;
            this.dateTimeProvider = dateTimeProvider;
            this.log = log;
        }

        // GET: Votes
        public async Task<ActionResult> Index()
        {
            var votes = this.db.Votes.Include(v => v.Option).Include(v => v.Survey);
            return this.View(await votes.ToListAsync());
        }

        // GET: Votes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Vote vote = await this.db.Votes.FindAsync(id);
            if (vote == null)
            {
                return this.HttpNotFound();
            }

            return this.View(vote);
        }

        // GET: Votes/Create
        public ActionResult Create()
        {
            this.ViewBag.OptionId = new SelectList(db.Options, "Id", "Name");
            this.ViewBag.SurveyId = new SelectList(db.Surveys, "Id", "Name");
            return this.View();
        }

        // POST: Votes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,SurveyId,OptionId")] Vote vote)
        {
            if (this.ModelState.IsValid)
            {
                vote.CreateDate = this.dateTimeProvider.GetCurrentTime();
                this.db.Votes.Add(vote);
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.OptionId = new SelectList(db.Options, "Id", "Name", vote.OptionId);
            this.ViewBag.SurveyId = new SelectList(db.Surveys, "Id", "Name", vote.SurveyId);
            return this.View(vote);
        }

        // GET: Votes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Vote vote = await this.db.Votes.FindAsync(id);
            if (vote == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.OptionId = new SelectList(db.Options, "Id", "Name", vote.OptionId);
            this.ViewBag.SurveyId = new SelectList(db.Surveys, "Id", "Name", vote.SurveyId);
            return this.View(vote);
        }

        // POST: Votes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CreateDate,SurveyId,OptionId")] Vote vote)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(vote).State = EntityState.Modified;
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.OptionId = new SelectList(db.Options, "Id", "Name", vote.OptionId);
            this.ViewBag.SurveyId = new SelectList(db.Surveys, "Id", "Name", vote.SurveyId);
            return this.View(vote);
        }

        // GET: Votes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Vote vote = await this.db.Votes.FindAsync(id);
            if (vote == null)
            {
                return this.HttpNotFound();
            }

            return this.View(vote);
        }

        // POST: Votes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Vote vote = await this.db.Votes.FindAsync(id);
            this.db.Votes.Remove(vote);
            await this.db.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
