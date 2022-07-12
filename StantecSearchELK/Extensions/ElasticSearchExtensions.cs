using Nest;

namespace StantecSearchELK
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            // Getting configuration settings for connection to ElasticSearch
            var url = configuration["ELKConfiguration:Uri"];
            var defaultIndex = configuration["ELKConfiguration:index"];
            var username = configuration["ELKConfiguration:username"];
            var password = configuration["ELKConfiguration:password"];

            // Using NEST to create the connection
            var settings = new ConnectionSettings(new Uri(url))
                    .PrettyJson() // Makes the request a nice JSON request
                    .DefaultIndex(defaultIndex)
                    .BasicAuthentication(username, password);
            settings.EnableApiVersioningHeader();
            AddDefaultMappings(settings);

            // Creating the Elastic Search Client, will be used in some dependancy injection for controllers
            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client); // Singleton so that the same instance is across the application

            CreateIndex(client, defaultIndex);
            PeopleController peopleController = new PeopleController(client, null);
            peopleController.CreateData();
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            // Setting up how the Search will happen
            settings.DefaultMappingFor<People>(
                p =>p.IdProperty(x => x.Id)
            ); 
        }

        // This will be used to Create the Index
        // Gives the Elastic Client data to search through
        private static void CreateIndex(IElasticClient client, string indexName)
        {
            // Indices is a call that holds all the Indexs to be searched through
            client.Indices.Create(indexName, i => i.Map<People>(x => x.AutoMap())); // Auto mapping People
        }
    }
}