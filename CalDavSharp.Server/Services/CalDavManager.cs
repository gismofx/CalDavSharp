using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperRepository;
using CalDavSharp.Server.Models;
using System.Xml;
using System.Diagnostics;
using System.Xml.Linq;

namespace CalDavSharp.Server.Services
{
    public class CalDavManager
    {
        private IRepository<Event> _EventRepository;
        private IRepository<Calendar> _CalendarRepository;
        private CalDavParser _Parser;

        public CalDavManager(IRepository<Event> eventRepository, IRepository<Calendar> calendarRepository, CalDavParser parser )
        {
            _EventRepository = eventRepository;
            _CalendarRepository = calendarRepository;
            _Parser = parser;
        }

        public XDocument Propfind(string userName, string calendarName, XmlDocument xmlDoc)
        {
            var result = _Parser.ParsePropfind(xmlDoc);
            //@ToDo: look up properties and foreach below inside propstat to product the properties
            
            XNamespace xnsD = "DAV:";
            XNamespace xnsC = "urn:ietf:params:xml:ns:caldav";
            var resultDoc = new XDocument(
                new XDeclaration("1.1", "UTF-8", null),
                new XElement("multistatus", new XAttribute(XNamespace.Xmlns +"D", "DAV:"), new XAttribute(XNamespace.Xmlns + "C", "urn:ietf:params:xml:ns:caldav"),
                    new XElement("Response",
                        new XElement(xnsD+"href",$"/calendars/{userName}/{calendarName}"),
                        new XElement(xnsD+"propstat", "hi"))
                           
                    ));

            return resultDoc;
            /*
            //XmlDocument();
            var decl = resultDoc.CreateXmlDeclaration("1.1", "UTF-8", null);
            resultDoc.InsertBefore(decl, resultDoc.DocumentElement);
            XmlElement element1 = resultDoc.CreateElement(string.Empty, "body", string.Empty);
            resultDoc.AppendChild(element1);
            */

        }

        public XmlDocument Report(string userName, string calendarName, XmlDocument xmlDoc)
        {
			var result = _Parser.ParseReport(xmlDoc);

            return null;
		}

    }
}
