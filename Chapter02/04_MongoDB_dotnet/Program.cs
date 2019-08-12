namespace _04_MongoDB_dotnet
{
    using System;
    using System.Threading.Tasks;
    
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class Program
    {
        internal static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting to MongoDB...");
            
            var settings = new MongoClientSettings
            {
                Servers = new[]
                {
                    new MongoServerAddress("mongo-node1", 27017),
                    new MongoServerAddress("mongo-node2", 27017),
                    new MongoServerAddress("mongo-node3", 27017)
                },
                ConnectionMode = ConnectionMode.ReplicaSet,
                ReplicaSetName = "replSet0"
            };

            var client = new MongoClient(settings);
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("demo");
            var list = await collection.Find(_ => true).ToListAsync();

            foreach (var document in list)
            {
                Console.WriteLine(document["exampleValue"]);
            }
        }
    }
}
