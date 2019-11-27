namespace VotingApplication
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Ninject.Extensions.Logging;

    using VotingApplication.Interfaces;
    using VotingApplication.Models;

    public class OptionsController : Controller
    {
        private readonly VotingApplicationContext db;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ILogger log;

        public OptionsController(VotingApplicationContext db, IDateTimeProvider dateTimeProvider, ILogger log)
        {
            this.db = db;
            this.dateTimeProvider = dateTimeProvider;
            this.log = log;
        }

        // GET: Options
        public async Task<ActionResult> Index()
        {
            return this.View(await db.Options.ToListAsync());
        }

        // GET: Options/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Option option = await this.db.Options.FindAsync(id);
            if (option == null)
            {
                return this.HttpNotFound();
            }

            return this.View(option);
        }

        // GET: Options/Create
        public ActionResult Create()
        {
            ViewBag.AvailableSurveys = new SelectList(this.GetAvailableSurveys(), "Value", "Text");
            return this.View();
        }

        // POST: Options/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,SurveyId")] Option option)
        {
            if (this.ModelState.IsValid)
            {
                option.CreateDate = this.dateTimeProvider.GetCurrentTime();
                this.db.Options.Add(option);
                await this.db.SaveChangesAsync();
                this.log.Info("Created Option with ID={0}", option.Id);
                return this.RedirectToAction("Index");
            }

            return this.View(option);
        }

        // GET: Options/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Option option = await db.Options.FindAsync(id);
            if (option == null)
            {
                return this.HttpNotFound();
            }

            ViewBag.AvailableSurveys = new SelectList(this.GetAvailableSurveys(), "Value", "Text");
            return this.View(option);
        }

        // POST: Options/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CreateDate,Name,SurveyId")] Option option)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(option).State = EntityState.Modified;
                await this.db.SaveChangesAsync();
                this.log.Info("Updated Option with ID={0}", option.Id);
                return this.RedirectToAction("Index");
            }

            return this.View(option);
        }

        // GET: Options/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Option option = await this.db.Options.FindAsync(id);
            if (option == null)
            {
                return this.HttpNotFound();
            }

            return this.View(option);
        }

        // POST: Options/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Option option = await this.db.Options.FindAsync(id);
            this.db.Options.Remove(option);
            await this.db.SaveChangesAsync();
            this.log.Info("Deleted Option with ID={0}", option.Id);
            return this.RedirectToAction("Index");
        }

        private List<SelectListItem> GetAvailableSurveys()
        {
            return this.db.Surveys
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();
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
