using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QuartzScheduler.Infrastructure.Utils;
using QuartzScheduler.Web.Models;
using QuartzScheduler.Web.Utils;

namespace QuartzScheduler.Web.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult JobList()
        {
            var jobs = GetAllJobs2();
            return PartialView(jobs);
        }

        [HttpPost]
        public ActionResult PauseJob(string name, string group)
        {
            Scheduler.Instance.PauseJob(name, group);
            return RedirectToAction("JobList");
        }

        [HttpPost]
        public ActionResult ResumeJob(string name, string group)
        {
            Scheduler.Instance.ResumeJob(name, group);
            return RedirectToAction("JobList");
        }

        [HttpPost]
        public ActionResult RemoveJob(string name, string group)
        {
            Scheduler.Instance.DeleteJob(name, group);
            return RedirectToAction("JobList");
        }

        [HttpPost]
        public ActionResult ResumeAll()
        {
            Scheduler.Instance.ResumeAll();
            return RedirectToAction("JobList");
        }

        [HttpPost]
        public ActionResult PauseAll()
        {
            Scheduler.Instance.PauseAll();
            return RedirectToAction("JobList");
        }

        public ActionResult AddJob()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddJob(AddJobViewModel model)
        {
            try
            {
                var type = JobUtil.Create(model.NameSpace, string.Format("{0}.{1}", model.NameSpace, model.ClassName));
                var properties = new Dictionary<string, object>();
                if (model.TriggerProperties != null)
                {
                    foreach (var keyVal in model.TriggerProperties)
                    {
                        try
                        {
                            string[] splitted = keyVal.Split('=');
                            properties.Add(splitted[0], splitted[1]);
                        }
                        catch (Exception){}
                    }
                }
                Scheduler.Instance.AddJob(model.JobName, model.JobGroup, model.JobName, model.JobGroup,
                    model.CronExpression, type, model.IsDurable, properties);
            }
            catch (Exception ex)
            {
                return Json(new {success = false, responseMessage = ex.Message});
            }
            
            return Json(new {success = true });
        }

        #region Private methods

        private List<JobTriggerViewModel> GetAllJobs2()
        {
            var jobTriggers = new List<JobTriggerViewModel>();
            try
            {
                var jobs = Scheduler.Instance.GetAllJobs();
                foreach (var job in jobs)
                {
                    jobTriggers.AddRange(job.Triggers.Select(trigger => new JobTriggerViewModel
                    {
                        JobName = job.Name,
                        JobGroup = job.GroupName,
                        TriggerName = trigger.Name,
                        TriggerGroup = trigger.GroupName,
                        State = trigger.State,
                        NextFireTime = trigger.NextFireTime,
                        PreviousFireTime = trigger.PreviousFireTime,
                        Properties =
                            trigger.DataMap != null
                                ? string.Join(";", trigger.DataMap.Select(x => x.Key + "=" + x.Value).ToArray())
                                : string.Empty
                    }));
                }
            }
            catch (Exception ex)
            {
            }
            return jobTriggers;
        }
        #endregion
    }
}