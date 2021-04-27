using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperRepository;
using CalDavSharp.Server.Models;
using System.Xml;
using System.Diagnostics;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

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
                        new XElement(
                            xnsD+"propstat",
                                new XElement(xnsD+"prop"),
                                StatusCode(xnsD)
                        )
                           
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
        
        //@Todo: Add logic for return code
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        private XElement StatusCode(XNamespace ns)
        {
            return new XElement(ns + "status", "HTTP/1.1 200 OK");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="calendarName"></param>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public XmlDocument Report(string userName, string calendarName, XmlDocument xmlDoc)
        {
			var result = _Parser.ParseReport(xmlDoc);

            return null;
		}

    }
}
