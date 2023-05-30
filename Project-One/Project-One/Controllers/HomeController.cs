using Configurations;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Controllers
{
    public class HomeController : Controller
    {
        ContactDbEntities dt = new ContactDbEntities(); 

        [HttpGet]
        public ActionResult Index()
        {
            // Retrieve the configuration values from web.config
            string recaptchaSiteKey = ConfigurationManager.AppSettings["RecaptchaSiteKey"];
            string recaptchaSecretKey = ConfigurationManager.AppSettings["RecaptchaSecretKey"];

            // Create an instance of the GoogleRecaptchaConfig class
            var recaptchaConfig = new googleRecaptchaConfig
            {
                RecaptchaSiteKey = recaptchaSiteKey,
                RecaptchaSecretKey = recaptchaSecretKey
            };

            // Pass the recaptchaConfig object to the view
            ViewBag.RecaptchaConfig = recaptchaConfig;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Index(Table cot)
        {
            if (ModelState.IsValid)
            {
                var isCaptchaValid = await IsCaptchaValid(cot.GoogleCaptchaToken);
                if(isCaptchaValid)
                {
                    return RedirectToAction("Second");
                }
                else
                {
                    ModelState.AddModelError("GoogleCaptcha", "The captcha is not valid");
                }
                dt.Tables.Add(cot);
                dt.SaveChanges();
                return RedirectToAction("Second");
            }
            return View();
        }

        private async Task<bool> IsCaptchaValid(string response)
        {
            try
            {
                var secret = "6LesXEQmAAAAAI8M5W2FUMrQSzbpE-RgzVFgrwqE";
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        { "secret", secret },
                        { "response", response },
                        { "remoteip", Request.UserHostAddress }
                    };
                    var content = new FormUrlEncodedContent(values);
                    var verify = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                    var captchaResponseJson = await verify.Content.ReadAsStringAsync();
                    var captchaResult = JsonConvert.DeserializeObject<CaptchaResponseViewModel>(captchaResponseJson);
                    return captchaResult.Success
                        && captchaResult.Action == "submit"
                        && captchaResult.Score > 0.5;
                }
            }
            catch (Exception ex) {
                return false;
            }
        }

        public ActionResult Second()
        {
            return View();
        }
    }

    public class CaptchaResponseViewModel
    {
        public bool Success{ get; set; }
        [JsonProperty(PropertyName = "error-codes")]
        public IEnumerable<string> ErrorCodes { get; set; }
        public DateTime ChallengeTime { get; set; }
        public string HostName { get; set; }
        public double Score { get; set; }
        public string Action { get; set; }
    }
}