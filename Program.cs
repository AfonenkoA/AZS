using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using static System.Text.Json.JsonSerializer;

namespace AZS
{
    public class GasStation
    {
        public string name { get; set; } = "";
        public int id { get; set; }
        public string address { get; set; } = "";
        public int retail_network { get; set; }
        public int region { get; set; }
    }

    public class RetailNetwork
    {
        public string name { get; set; } = "";
        public int id { get; set; }
    }

    public class Region
    {
        public int id { get; set; }
        public string name { get; set; } = "";
    }

    public class ExtendedGasStation
    {
        public ExtendedGasStation(GasStation station, Dictionary<int, string> networks, Dictionary<int, string> regions)
        {
            name = station.name;
            address = station.address;
            retail_network = networks[station.retail_network];
            region = regions[station.region];
        }
        public string name { get; set; } = "";
        public string address { get; set; } = "";
        public string retail_network { get; set; } = "";
        public string region { get; set; } = "";
    }

    public class Program
    {
        private static readonly HttpClient client = new() { BaseAddress = new Uri("https://zaprauka.by/api/") };

        public static IEnumerable<T>? Get<T>(string path) where T : class
        {
            return Deserialize<IEnumerable<T>>(client.GetStringAsync(path + "/?format=json").GetAwaiter().GetResult());
        }

        public static string Process()
        {
            Exception exception = new("Parsing failed");
            IEnumerable<Region> regionList = Get<Region>("regions") ?? throw exception;
            IEnumerable<RetailNetwork> networkList = Get<RetailNetwork>("retail_networks") ?? throw exception;
            IEnumerable<GasStation> stations = Get<GasStation>("gas_stations") ?? throw exception;

            Dictionary<int, string> regions = new();
            foreach (Region region in regionList)
                regions.Add(region.id, region.name);

            Dictionary<int, string> networks = new();
            foreach (RetailNetwork network in networkList)
                networks.Add(network.id, network.name);

            List<ExtendedGasStation> result = new();
            foreach (GasStation station in stations)
                result.Add(new ExtendedGasStation(station, networks, regions));
            return Serialize(result, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            });
        }

        public static void Main(String[] args)
        {
            File.WriteAllText("1.json", Process());
        }
    }
}

