using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static System.Console;


namespace AZS
{
    internal class Program
    {
        public static void Process(Stream stream)
        {
            List<string> exceptedTags = File.ReadAllLines("ExceptedTags.txt").OrderBy(q => q).ToList();
            List<string> requiredTags = File.ReadAllLines("RequiredTags.txt").ToList();
            HttpClient client = new();
            Utf8JsonWriter writer = new(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = true
            });

            string rawJson = client.GetStringAsync(File.ReadAllText("Query.txt")).GetAwaiter().GetResult();
            using JsonDocument document = JsonDocument.Parse(rawJson);
            var elements = document.RootElement.GetProperty("elements");

            writer.WriteStartArray();
            foreach (var element in elements.EnumerateArray())
            {
                List<JsonProperty> properties = new();

                properties.AddRange(element.EnumerateObject().Where(i => exceptedTags.BinarySearch(i.Name) < 0));
                properties.AddRange(element.GetProperty("tags").EnumerateObject().Where(i => exceptedTags.BinarySearch(i.Name) < 0));

                if (requiredTags.Except(properties.Select(p => p.Name)).Any()) continue;

                writer.WriteStartObject();
                foreach (var property in properties)
                    property.WriteTo(writer);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.Flush();
        }
        public static void Main(string[] args)
        {
            try
            {
                switch (args.Length)
                {
                    case 1 when args[0] == "-c":
                        {
                            using MemoryStream stream = new();
                            Process(stream);
                            WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
                            break;
                        }
                    case 2 when args[0] == "-f":
                        {
                            using FileStream stream = File.OpenWrite(args[1]);
                            Process(stream);
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

