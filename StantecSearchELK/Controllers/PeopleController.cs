using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Linq;

namespace StantecSearchELK
{
    [ApiController]
    [Route("[controller]")]
    public class PeopleController : ControllerBase
    {
        public readonly IElasticClient elasticClient;
        private readonly ILogger<PeopleController>? logger;
        // counter used to determine how many should be created
        int counter = 2000;
        public PeopleController(IElasticClient _elasticClient, ILogger<PeopleController>? _logger)
        {
            elasticClient = _elasticClient;
            logger = _logger;
        }

        [HttpGet(Name = "GetPeoples")]
        public async Task<IActionResult> Get(string? keyword, string? searchColumn)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                 // This is the query used to get results back based off the search
                    var result = await elasticClient.SearchAsync<People>(
                    s => s.Query(
                        q => q.MatchAll()
                    ).Size(counter)
                );

                return Ok(result.Documents.ToList());
            }
            else if (!string.IsNullOrEmpty(keyword) && string.IsNullOrEmpty(searchColumn))
            {
                // This is the query used to get results back based off the search
                    var result = await elasticClient.SearchAsync<People>(
                    s => s.Query(
                        q => q.QueryString(
                            d =>d.Query('*'+keyword+'*') // * keyword * implies the keyword can be anywhere
                            .Fields(f => f
                                .Field(fi => fi.FirstName)
                                .Field(fi => fi.LastName)
                                .Field(fi => fi.Occupation)
                                .Field(fi => fi.City))
                        )
                    ).Size(counter)
                );
                return Ok(result.Documents.ToList());
            }
            else if (!string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(searchColumn))
            {
                    // This is the query used to get results back based off the search
                    var result = await elasticClient.SearchAsync<People>(
                    s => s.Query(
                        q => q.QueryString(
                            d =>d.Query('*'+keyword+'*') // * keyword * implies the keyword can be anywhere
                            .Fields(f => {
                                if (searchColumn == "FirstName")
                                {
                                    f.Field(fi => fi.FirstName);
                                }
                                if (searchColumn == "LastName")
                                {
                                    f.Field(fi => fi.LastName);
                                }
                                if (searchColumn == "City")
                                {
                                    f.Field(fi => fi.City);
                                }
                                if (searchColumn == "Occupation")
                                {
                                    f.Field(fi => fi.Occupation);
                                } 

                                return f;
                            })      
                        )
                    ).Size(counter)
                );
                return Ok(result.Documents.ToList());
            }

            return null;
        }

        [HttpPost(Name = "AddPeople")]
        public async Task<IActionResult> Post(People prod)
        {
            await elasticClient.IndexDocumentAsync(prod);
            
            return Ok(prod);
        }

        public void CreateData()
        {
            // Need to gather and figure out a way of creating data.
            // This will be used to clear the data so it doesnt become over bearing when restarting this service over and over.
            if (elasticClient.Count<People>().Count > 0)
            {
                //Deleteing the product index
                var delete = elasticClient.DeleteByQuery<People>(del => 
                del.Query(q => q.QueryString(qs => qs.Query("*"))));
            }

            // Lists to hold names that will be used randomly
            List<string> fNameList = FirstNameCSV();
            List<string> lNameList = LastNameCSV();
            List<string> occupationList = OccupationsCSV();
            List<string> cityList = CitiesCSV();
            for (int i = 0; i < counter-1; i++)
            {
                // Creating random numbers to be used for creating random data with the above lists.
                Random rng = new Random();
                int fNameRNG = rng.Next(0,99);
                int lNameRNG = rng.Next(0,99);
                int occupationRNG = rng.Next(0,129);
                int cityRNG = rng.Next(0,99);

                People person = new People();
                person.Id = i;
                person.FirstName = fNameList[fNameRNG];
                person.LastName = lNameList[lNameRNG];
                person.Occupation = occupationList[occupationRNG];
                person.Age = rng.Next(18, 99).ToString();
                person.City = cityList[cityRNG];
                person.SIN = rng.Next(0, 9).ToString() + rng.Next(0,9).ToString() + rng.Next(0,9).ToString() + "-" + rng.Next(0, 9).ToString() + rng.Next(0,9).ToString() + rng.Next(0,9).ToString() + "-" + rng.Next(0, 9).ToString() + rng.Next(0,9).ToString() + rng.Next(0,9).ToString();

                //inserting the person into the people index for ElasticSearch
                var result = Post(person);
            }
        }

        #region HelperMethods
        private List<string> FirstNameCSV()
        {
            List<string> firstNames = new List<string>();
            using (var reader = new StreamReader(@"FirstNames.csv"))
            {
                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        firstNames.Add(line);
                }
            }

            return firstNames;
        }

        private List<string> LastNameCSV()
        {
            List<string> lastNames = new List<string>();
            using (var reader = new StreamReader(@"LastNames.csv"))
            {
                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        lastNames.Add(line);
                }
            }

            return lastNames;
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
        #endregion

    }
}