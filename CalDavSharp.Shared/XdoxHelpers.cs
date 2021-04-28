using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CalDavSharp.Shared
{
    public static class XdoxHelpers
    {
        public static XElement Element(this XNamespace ns, string name, params object[] inner)
        {
            return new XElement(ns.GetName(name), inner);
        }

        public static XElement Element(this XName name, params object[] inner)
        {
            return new XElement(name, inner);
        }

        public static string FormatDate(this DateTime? dateTime)
        {
            if (dateTime == null) return null;
            return FormatDate(dateTime.Value);
        }
        public static string FormatDate(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddTHHmmss") + (dateTime.Kind == DateTimeKind.Utc ? "Z" : "");
        }

    }
}
