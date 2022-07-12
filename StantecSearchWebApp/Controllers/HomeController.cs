using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StantecSearchWebApp.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web;

namespace StantecSearchWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeViewModel homeVM;
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        homeVM = new HomeViewModel();
        List<string> autoCompleteList = new List<string>();
        autoCompleteList = OccupationsCSV();
        autoCompleteList.AddRange(CitiesCSV());

        string[] autoCompleteArray = autoCompleteList.ToArray();
        homeVM.AutoCompleteArray = autoCompleteArray;
    }

    public IActionResult Index()
    {
        return View(homeVM);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private void SetAutoCompleteArray(){
        
    }

    public async Task<HomeViewModel> GetListOfPeople(string keyword, string searchColumn)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // HTTP GET
            HttpResponseMessage response = await client.GetAsync("http://localhost:5033/People?keyword="+keyword+"&searchColumn="+searchColumn);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                homeVM.People = JsonConvert.DeserializeObject<List<People>>(result).ToList().Count() > 0 ? JsonConvert.DeserializeObject<List<People>>(result).ToList() : new List<People>() ;
                System.Console.WriteLine("People List Count: " + homeVM.People.Count().ToString());
            }
        }

        return homeVM;
    }

    public ActionResult Search(string searchString, string searchColumn)
    {
        homeVM.People = GetListOfPeople(searchString, searchColumn).Result.People;
        homeVM.SearchString = searchString;
        homeVM.SearchColumn = searchColumn;
        homeVM.peopleCount = homeVM.People.Count;
        return View("~/Views/Home/Index.cshtml", homeVM);
    }

    private List<string> OccupationsCSV()
        {
            List<string> Occupations = new List<string>();
            using (var reader = new StreamReader(@"Occupations.csv"))
            {
                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        Occupations.Add(line);
                }
            }

            return Occupations;
        }

        private List<string> CitiesCSV()
        {
            List<string> Cities = new List<string>();
            using (var reader = new StreamReader(@"Cities.csv"))
            {
                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        Cities.Add(line);
                }
            }

            return Cities;
        }
}
