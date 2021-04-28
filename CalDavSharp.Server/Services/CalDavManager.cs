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
using CalDavSharp.Shared;
//using System.Globalization;
using CalDavSharp.Server.Data;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace CalDavSharp.Server.Services
{
    public class CalDavManager
    {
        private CalDavParser _Parser;
        private XNamespace xNSC;
        private XNamespace xNSD;
        private CalendarRepository _CalendarRepo;

        public static readonly XNamespace xDav = XNamespace.Get("DAV:");
        public static readonly XNamespace xCalDav = XNamespace.Get("urn:ietf:params:xml:ns:caldav");
        public static readonly XNamespace xApple = XNamespace.Get("http://apple.com/ns/ical/");

        public CalDavManager(CalendarRepository calendarRepository, CalDavParser parser )
        {
            
            _CalendarRepo = calendarRepository;
            _Parser = parser;

            xNSD = "DAV:";
            xNSC = "urn:ietf:params:xml:ns:caldav";

        }

        public async Task<XDocument> Propfind(int depth, string userName, string calendarName, XDocument xDoc)
        {
            //var result = _Parser.ParsePropfind(xmlDoc);
            string getUserUrl = $"/{userName}/{calendarName}/";
            var props = xDoc.Descendants(XName.Get("prop", xNSD.NamespaceName)).FirstOrDefault().Elements();// GetName("prop")).FirstOrDefault().Elements()

            var calendar = await _CalendarRepo.GetCalendarByUserandNameAsync(userName, calendarName);
            if (calendar is null)
            {
                //@Todo: create null response 
                return null;
            }

            var allprop = props.Elements(xDav.GetName("allprops")).Any();
            var hrefName = xDav.GetName("href");
            //var scheduleInboxURLName = xCalDav.GetName("schedule-inbox-URL");
            //var scheduleOutoxURLName = xCalDav.GetName("schedule-outbox-URL");
            //var addressbookHomeSetName = xCalDav.GetName("addressbook-home-set");

            var calendarUserAddressSetName = xCalDav.GetName("calendar-user-address-set");
            var calendarUserAddress = !allprop && !props.Any(x => x.Name == calendarUserAddressSetName) ? null :
                calendarUserAddressSetName.Element(
                    hrefName.Element(getUserUrl),
                    hrefName.Element("mailto:" + GetUserEmail())
                );


            var supportedReportSetName = xDav.GetName("supported-report-set");
            var supportedReportSet = !allprop && !props.Any(x => x.Name == supportedReportSetName) ? null :
                supportedReportSetName.Element(
                    xDav.Element("supported-report", xDav.Element("report", xDav.Element("calendar-multiget")))
                );

            var calendarHomeSetName = xCalDav.GetName("calendar-home-set");
            var calendarHomeSet = !allprop && !props.Any(x => x.Name == calendarHomeSetName) ? null :
                calendarHomeSetName.Element(hrefName.Element(getUserUrl));

            var getetagName = xDav.GetName("getetag");
            var getetag = !allprop && !props.Any(x => x.Name == getetagName) ? null :
                getetagName.Element();

            var currentUserPrincipalName = xDav.GetName("current-user-principal");
            var currentUserPrincipal = !props.Any(x => x.Name == currentUserPrincipalName) ? null :
                currentUserPrincipalName.Element(hrefName.Element(getUserUrl));

            var resourceTypeName = xDav.GetName("resourcetype");
            var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : (
                    resourceTypeName.Element(xDav.Element("collection"), xCalDav.Element("calendar"), xDav.Element("principal"))
                );

            var ownerName = xDav.GetName("owner");
            var owner = !allprop && !props.Any(x => x.Name == ownerName) ? null :
                ownerName.Element(hrefName.Element(getUserUrl));

            var displayNameName = xDav.GetName("displayname");
            var displayName = calendar == null || (!allprop && !props.Any(x => x.Name == displayNameName)) ? null :
                displayNameName.Element(calendar.Name ?? calendar.Id);

            var calendarColorName = xApple.GetName("calendar-color");
            var calendarColor = !allprop && !props.Any(x => x.Name == calendarColorName) ? null :
                calendarColorName.Element("FF5800");

            var calendarDescriptionName = xCalDav.GetName("calendar-description");
            var calendarDescription = calendar == null || (!allprop && !props.Any(x => x.Name == calendarDescriptionName)) ? null :
                calendarDescriptionName.Element(calendar.Name);

            var supportedComponentsName = xCalDav.GetName("supported-calendar-component-set");
            var supportedComponents = !allprop && !props.Any(x => x.Name == supportedComponentsName) ? null :
                new[]{
                    xCalDav.Element("comp", new XAttribute("name", "VEVENT")),
                    xCalDav.Element("comp", new XAttribute("name", "VTODO"))
                };

            var getContentTypeName = xDav.GetName("getcontenttype");
            var getContentType = !allprop && !props.Any(x => x.Name == getContentTypeName) ? null :
                getContentTypeName.Element("text/calendar; component=vevent");

            var supportedProperties = new HashSet<XName> {
                resourceTypeName, ownerName, supportedComponentsName, getContentTypeName,
                displayNameName, calendarDescriptionName, calendarColorName,
                currentUserPrincipalName, calendarHomeSetName, calendarUserAddressSetName,
                supportedComponentsName
            };
            var prop404 = xDav.Element("prop", props
                        .Where(p => !supportedProperties.Contains(p.Name))
                        .Select(p => new XElement(p.Name))
                );
            var propStat404 = xDav.Element("propstat",
                xDav.Element("status", "HTTP/1.1 404 Not Found"), prop404);

            //@ToDo: look up properties and foreach below inside propstat to product the properties
            var e = new XElement(
                    xDav.Element("multistatus",
                    xDav.Element("response",
                    xDav.Element("href", getUserUrl /*Request.RawUrl*/),
                    xDav.Element("propstat",
                                xDav.Element("status", "HTTP/1.1 200 OK"),
                                xDav.Element("prop",
                                    resourceType, owner, supportedComponents, displayName,
                                    getContentType, calendarDescription, calendarHomeSet,
                                    currentUserPrincipal, supportedReportSet, calendarColor,
                                    calendarUserAddress
                                )
                            ),

                            (prop404.Elements().Any() ? propStat404 : null)
                     ),

                     (depth == 0 ? null :
                         (_CalendarRepo.GetObjects(calendar).Result
                         .Where(x => x != null)
                         .ToArray()
                            .Select(item => xDav.Element("response",
                                hrefName.Element(getUserUrl/*GetCalendarObjectUrl(calendar.ID, item.UID)*/),
                                    xDav.Element("propstat",
                                        xDav.Element("status", "HTTP/1.1 200 OK"),
                                        resourceType == null ? null : resourceTypeName.Element(),
                                        (getContentType == null ? null : getContentTypeName.Element("text/calendar; component=v" + item.GetType().Name.ToLower())),
                                        getetag == null ? null : getetagName.Element("\"" + XdoxHelpers.FormatDate(item.LastModified) + "\"")
                                    )
                                )
                            )

                )
                )
                )
                    );
            
            //return new Result
            /*

            {
                Status = (System.Net.HttpStatusCode)207,
                Content = xDav.Element("multistatus",
                    xDav.Element("response",
                    xDav.Element("href", Request.RawUrl),
                    xDav.Element("propstat",
                                xDav.Element("status", "HTTP/1.1 200 OK"),
                                xDav.Element("prop",
                                    resourceType, owner, supportedComponents, displayName,
                                    getContentType, calendarDescription, calendarHomeSet,
                                    currentUserPrincipal, supportedReportSet, calendarColor,
                                    calendarUserAddress
                                )
                            ),

                            (prop404.Elements().Any() ? propStat404 : null)
                     ),

                     (depth == 0 ? null :
                         (repo.GetObjects(calendar)
                         .Where(x => x != null)
                         .ToArray()
                            .Select(item => xDav.Element("response",
                                hrefName.Element(GetCalendarObjectUrl(calendar.ID, item.UID)),
                                    xDav.Element("propstat",
                                        xDav.Element("status", "HTTP/1.1 200 OK"),
                                        resourceType == null ? null : resourceTypeName.Element(),
                                        (getContentType == null ? null : getContentTypeName.Element("text/calendar; component=v" + item.GetType().Name.ToLower())),
                                        getetag == null ? null : getetagName.Element("\"" + FormatDate(item.LastModified) + "\"")
                                    )
                                ))
                            .ToArray()))
                 )
            };
            */

            var resultDoc = new XDocument(
                new XDeclaration("1.1", "UTF-8", null),
                new XElement("multistatus", new XAttribute(XNamespace.Xmlns +"D", "DAV:"), new XAttribute(XNamespace.Xmlns + "C", "urn:ietf:params:xml:ns:caldav"),
                    new XElement("Response",
                        new XElement(xNSD+"href",$"/calendars/{userName}/{calendarName}"),
                        new XElement(
                            xNSD+"propstat",
                                new XElement(xNSD+"prop"),
                                StatusCode(xNSD)
                        )
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

        private string GetUserEmail()
        {
            throw new NotImplementedException();
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
