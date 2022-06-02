using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAccountsDataBaseWebApi;
using WebApiAccount.Models;
using WebApiAccount.Services;

namespace WebApiAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ReportsController : DataController<Report, ReportStore, ReportsAgent>
    {
        public ReportsController(ReportsAgent reports) : base(reports)
        {
            
        }
        [HttpPost("ChangeDate/{id}/{date}")]
        public async Task<ControllerResult<Report, bool, Report>> ChangeDate(int id, DateTime date)
        {
            var report = (await _store.Get(id)).Entity;
            var valid = await _store.ChangeDate(date, report);
            var res = new ControllerResult<Report, bool, Report>(report, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("ChangeText/{id}/{text}")]
        public async Task<ControllerResult<Report, bool, Report>> ChangeText(int id, string text)
        {
            var report = (await _store.Get(id)).Entity;
            var valid = await _store.ChangeText(text, report);
            var res = new ControllerResult<Report, bool, Report>(report, valid.IsValidate, valid);
            return res;
        }
    }
}
