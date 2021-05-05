using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq; 

namespace CalDavSharp.Shared
{
    public class CaldavProperty
    {
        public XNamespace xNameSpace { get; set; }
        public string PropertyName { get; set; }
        public bool IncludeInAllProp { get; set; }
        public CaldavProperty(string name, XNamespace propNamespace, bool includeInAllProp=true)
        {
            xNameSpace = propNamespace;
            PropertyName = name;
            IncludeInAllProp = includeInAllProp;
        }

        public XName Name()
        {
            return XName.Get(PropertyName, xNameSpace.NamespaceName);
        }


    }
}
