using Newtonsoft.Json;
using SpotifyAnalysis.Input.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Input
{
    public static class JsonReader
    {
        public static List<EndSongEntry> ReadStreamingHistory(IEnumerable<string> endSongFiles)
        {
            List<EndSongEntry> fullHistory = new List<EndSongEntry>();

            foreach (var file in endSongFiles)
            {
                Console.WriteLine($"Reading [{file}] contents");
                var fileContent = File.ReadAllText(file);
                Console.WriteLine($"Found content of length [{fileContent.Length:N0}]");

                Console.WriteLine($"Parsing to end songs");
                var parsedContent = JsonConvert.DeserializeObject<EndSongEntry[]>(fileContent);
                Console.WriteLine($"Parsed to [{parsedContent.Length:N0}] entries");

                fullHistory.AddRange(parsedContent);
            }


            foreach(var entry in fullHistory)
            {
                entry.TrackStopTimestamp = DateTime.SpecifyKind(entry.TrackStopTimestamp, DateTimeKind.Utc);
            }
            return fullHistory;
        }
    }
}