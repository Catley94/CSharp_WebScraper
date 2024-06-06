using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeekNZScraper.Models
{
    public record ProgrammingLanguage
    {
        public string Name { get; }
        public int Count { get; private set; }

        public ProgrammingLanguage(string _Name) 
        {
            Name = _Name;
        }

        public void IncrementCount()
        {
            Count++;
        }
    }
}
