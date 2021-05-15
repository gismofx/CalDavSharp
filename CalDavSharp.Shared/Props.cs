using System;
using System.Xml.Linq;
using System.Xml;
using System.Collections.Generic;

namespace CalDavSharp.Shared
{
    public static class Props
    {
        public static readonly XNamespace xDav = XNamespace.Get("DAV:");
        public static readonly XNamespace xCalDav = XNamespace.Get("urn:ietf:params:xml:ns:caldav");
        public static readonly XNamespace xApple = XNamespace.Get("http://apple.com/ns/ical/");
        public static readonly XNamespace xCS = XNamespace.Get("http://calendarserver.org/ns/");

        public static Dictionary<string, DavProperty> Properties()
        {
            var properties = new Dictionary<string, DavProperty>();
            properties.AddCalDavPropery("allprops", xDav);// xDav.GetName("allprops"));
            properties.AddCalDavPropery("href", xDav);// xDav.GetName("href"));
            properties.AddCalDavPropery("calendar-user-address-set", xCalDav, false);//  xCalDav.GetName("calendar-user-address-set"));
            properties.AddCalDavPropery("supported-report-set", xDav);// Add("supported-report-set", xDav.GetName("supported-report-set"));
            properties.AddCalDavPropery("getetag", xDav);// ("getetag", xDav.GetName("getetag"));
            properties.AddCalDavPropery("current-user-principal", xDav); // ("current-user-principal",xDav.GetName("current-user-principal"));
            properties.AddCalDavPropery("resourcetype", xDav); // ("resourcetype",xDav.GetName("resourcetype"));
            properties.AddCalDavPropery("owner", xDav); // ("owner",xDav.GetName("owner"));
            properties.AddCalDavPropery("displayname", xDav); // ("displayname",xDav.GetName("displayname"));
            properties.AddCalDavPropery("calendar-color", xApple);// ("calendar-color",xApple.GetName("calendar-color"));
            properties.AddCalDavPropery("calendar-description", xCalDav, false); // ("calendar-description",xCalDav.GetName("calendar-description"));
            properties.AddCalDavPropery("supported-calendar-component-set", xCalDav, false);// ("supported-calendar-component-set",xCalDav.GetName("supported-calendar-component-set"));
            properties.AddCalDavPropery("getcontenttype", xDav);// ("getcontenttype",xDav.GetName("getcontenttype"));
            properties.AddCalDavPropery("getctag", xCS);// ("getcontenttype",xDav.GetName("getcontenttype"));
            properties.AddCalDavPropery("calendar-home-set", xCalDav, false);
            return properties;
        }

        public static void AddCalDavPropery(this Dictionary<string, DavProperty> propDict, string propertyName, XNamespace ns, bool includeInAllProp = true)
        {
            propDict.Add(propertyName, new DavProperty(propertyName, ns,includeInAllProp));
        }


    }

}
