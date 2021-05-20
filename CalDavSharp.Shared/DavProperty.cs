using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq; 

namespace CalDavSharp.Shared
{
    public sealed class DavProperty
    {
        public static readonly XNamespace xDav = XNamespace.Get("DAV:");
        public static readonly XNamespace xCalDav = XNamespace.Get("urn:ietf:params:xml:ns:caldav");
        public static readonly XNamespace xApple = XNamespace.Get("http://apple.com/ns/ical/");
        public static readonly XNamespace xCS = XNamespace.Get("http://calendarserver.org/ns/");

        private static Dictionary<string,DavProperty> _Properties = new(StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyDictionary<string,DavProperty> Properties
        {
            get => _Properties;
        }

        public static DavProperty allprops {get;} = AddDavProperty("allprops", xDav, false);
        public static DavProperty href { get; } = AddDavProperty("href", xDav);
        public static DavProperty calendar_user_address_set { get; } = AddDavProperty("calendar-user-address-set", xCalDav, false);
        public static DavProperty supported_report_set { get; } = AddDavProperty("supported-report-set", xDav);
        public static DavProperty getetag { get; } = AddDavProperty("getetag", xDav);
        public static DavProperty current_user_principal { get; } = AddDavProperty("current-user-principal", xDav);
        public static DavProperty resourcetype { get; } = AddDavProperty("resourcetype", xDav);
        public static DavProperty owner { get; } = AddDavProperty("owner", xDav);
        public static DavProperty displayname { get; } = AddDavProperty("displayname", xDav);
        public static DavProperty calendar_color { get; } = AddDavProperty("calendar-color", xApple);
        public static DavProperty calendar_description { get; } = AddDavProperty("calendar-description", xCalDav, false);
        public static DavProperty supported_calendar_component_set { get; } = AddDavProperty("supported-calendar-component-set", xCalDav, false);
        public static DavProperty getcontenttype { get; } = AddDavProperty("getcontenttype", xDav);
        public static DavProperty getctag { get; } = AddDavProperty("getctag", xCS);
        public static DavProperty calendar_home_set { get; } = AddDavProperty("calendar-home-set", xCalDav, false);
        public static DavProperty principal_URL { get; } = AddDavProperty("principal-URL", xDav, false);
        public static DavProperty principal_collection_set { get; } = AddDavProperty("principal-collection-set", xDav, false);

        private static DavProperty AddDavProperty(string name, XNamespace propNamespace, bool includeInAllProp = true)
        {
            var prop = new DavProperty(name, propNamespace, includeInAllProp);
            _Properties.Add(name, prop);
            return prop;
        }

        public readonly XNamespace xNameSpace;
        
        /// <summary>
        /// The String Value/Name of the property per the spec
        /// </summary>
        public readonly string PropertyName;
        
        public readonly bool IncludeInAllProp;
        public DavProperty(string name, XNamespace propNamespace, bool includeInAllProp=true)
        {
            xNameSpace = propNamespace;
            PropertyName = name;
            IncludeInAllProp = includeInAllProp;
        }

        public XName xName()
        {
            return XName.Get(PropertyName, xNameSpace.NamespaceName);
        }
        public XElement xElement(params object[] inner)
        {
            return xName().Element(inner);

        }

    }
}
