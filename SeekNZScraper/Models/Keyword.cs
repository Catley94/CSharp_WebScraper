using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeekNZScraper.Models
{
    public record Keyword
    {
        public string Name { get; }
        public int Count { get; private set; }

        public List<string> HtmlStrings = new List<string>();
        public List<string> Urls = new List<string>();

        public Keyword(string _Name) 
        {
            Name = _Name;
        }

        public void IncrementCount()
        {
            Count++;
        }
    }
}
