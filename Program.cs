using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Console;


namespace AZS
{
    internal class Program
    {
        private static readonly HttpClient Client = new();

        class GasStation
        {
            [JsonPropertyName("lat")] public string? Latitude { get; set; }
            [JsonPropertyName("lng")] public string? Longitude { get; set; }

            [JsonPropertyName("range")] public string Range { get; set; } = "100";
            public void SetGroupByBrand(string? brand)
            {
                Group = $"_Локальные/АЗС/{brand}";
            }
            [JsonPropertyName("group")] public string? Group { get; set; }

            [JsonPropertyName("label")]
            public string? Label { get; set; }
            
            public void SetAddressByCoordinates(string? lat,string? lon)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json");
                // Add our custom headers
                requestMessage.Headers.Add("User-Agent", "User-Agent");
                var response = Client.SendAsync(requestMessage).GetAwaiter().GetResult();
                var s = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using var document = JsonDocument.Parse(s);
                Address = document.RootElement.GetProperty("display_name").GetString();
            }

            [JsonPropertyName("address")]
            public string? Address { get; set; }

            [JsonPropertyName("detailedaddress")] public string? DetailedAddress { get; set; } = null;
            
            [JsonPropertyName("icon")]
            public string Icon { get; set; } = "shop_green";
        }
        public static string Process()
        {
            List<string> requiredTags = File.ReadAllLines("Config\\RequiredTags.txt").ToList();
            var options =  new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string rawJson = Client.GetStringAsync(File.ReadAllText("Config\\Query.txt")).GetAwaiter().GetResult();
            using JsonDocument document = JsonDocument.Parse(rawJson);
            var elements = document.RootElement.GetProperty("elements");

            List<string> stations = new();

            foreach (var element in elements.EnumerateArray())
            {
                List<JsonProperty> properties = new();
                properties.AddRange(element.EnumerateObject());
                properties.AddRange(element.GetProperty("tags").EnumerateObject());
                
                if (requiredTags.Except(properties.Select(p => p.Name)).Any()) continue;

                GasStation station = new();
                foreach (var property in properties)
                {
                   
                    switch (property.Name)
                    {
                        case "lat":
                            station.Latitude = property.Value.ToString();
                            break;
                        case "lon":
                            station.Longitude = property.Value.ToString();
                            break;
                        case "brand":
                            station.SetGroupByBrand(property.Value.ToString());
                            break;
                        case "name":
                            station.Label = property.Value.ToString();
                            break;
                    }
                }
                station.SetAddressByCoordinates(station.Latitude, station.Longitude);
                stations.Add(JsonSerializer.Serialize(station, options));
            }

            StringBuilder result = new();
            result.Append("[" + Environment.NewLine);
            result.AppendJoin("," + Environment.NewLine, stations);
            result.Append(Environment.NewLine + "]");
            return result.ToString();
        }
        public static void Main(string[] args)
        {
            try
            {
                switch (args.Length)
                {
                    case 1 when args[0] == "-c":
                        {
                            WriteLine(Process());
                            break;
                        }
                    case 2 when args[0] == "-f":
                        {
                            File.WriteAllText(args[1],Process());
                            break;
                        }
                    default:
                        WriteLine("run with:\n-c to show result in console;\n-f [filename] to write result in file;");
                        break;
                }
            }
            catch (Exception e)
            {
                WriteLine(e);
            }
        }
    }
}

