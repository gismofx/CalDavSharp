using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalDavServer_Play
{
    public class ParsedRequest
    {
        public List<string> RequestedProperties { get; set; } = new List<string>();
    }
}
