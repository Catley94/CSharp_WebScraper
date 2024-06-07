using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SeekNZScraper.Data
{
    public class JsonSaveData
    {
        [JsonPropertyName("htmlStrings")]   
        public List<string> HtmlStrings { get; set; }

        [JsonPropertyName("urls")]
        public List<string> Urls { get; set; }

        [JsonConstructor]
        public JsonSaveData(List<string> htmlStrings, List<string> urls) 
        {
            this.HtmlStrings = htmlStrings;
            this.Urls = urls;
        }

    }
}