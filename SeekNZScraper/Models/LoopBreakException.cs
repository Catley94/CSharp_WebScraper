using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeekNZScraper.Models
{
    public class LoopBreakException : Exception
    {
        public LoopBreakException() : base() { }
        public LoopBreakException(string message) : base(message) { }
    }
}
