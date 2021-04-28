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


        public static List<XName> Properties()
        {
            var properties = new List<XName>();
            properties.Add(xDav.GetName("allprops"));
            properties.Add(xDav.GetName("href"));
            properties.Add(xCalDav.GetName("calendar-user-address-set"));
            properties.Add(xDav.GetName("supported-report-set"));
            properties.Add(xDav.GetName("getetag"));
            properties.Add(xDav.GetName("current-user-principal"));
            properties.Add(xDav.GetName("resourcetype"));
            properties.Add(xDav.GetName("owner"));
            properties.Add(xDav.GetName("displayname"));
            properties.Add(xApple.GetName("calendar-color"));
            properties.Add(xCalDav.GetName("calendar-description"));
            properties.Add(xCalDav.GetName("supported-calendar-component-set"));
            properties.Add(xDav.GetName("getcontenttype"));
            properties.Add(xCalDav.GetName("calendar-description"));
            return properties;
        }
    }
}
