using Microsoft.AspNetCore.Mvc;

namespace PalTracker
{
    [Route("/")]
    public class WelcomeController : Controller
    {
        [HttpGet]
        public string SayHello() => "hello";
    }
}