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
using System.Runtime.InteropServices.ComTypes;
//using Ical.Net;

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
        public static readonly XNamespace xCS = XNamespace.Get("http://calendarserver.org/ns/");

        private readonly XName hrefName;
        private string UserUrl;
        private string CalendarUrl;

        public CalDavManager(CalendarRepository calendarRepository, CalDavParser parser )
        {
            
            _CalendarRepo = calendarRepository;
            _Parser = parser;

            xNSD = "DAV:";
            xNSC = "urn:ietf:params:xml:ns:caldav";
            hrefName = xDav.GetName("href");
        }

        public async Task<XDocument> Propfind(int depth, string userName, string calendarName, XDocument xDoc)
        {
            //var result = _Parser.ParsePropfind(xmlDoc);
            UserUrl = $"/CalDav/Calendars/{userName}";
            CalendarUrl = $"{UserUrl}/{calendarName}";
            var props = xDoc.Descendants(XName.Get("prop", xNSD.NamespaceName)).FirstOrDefault().Elements();// GetName("prop")).FirstOrDefault().Elements()

            Calendar calendar = null;
            if (userName is not null && calendarName is not null)
            {
                calendar = await _CalendarRepo.GetCalendarByUserandNameAsync(userName, calendarName);
                if (calendar is null)
                {
                    //@Todo: create null response 
                    return null;
                }
            }
                    

            var allprop = props.Elements(xDav.GetName("allprops")).Any();
            //var hrefName = xDav.GetName("href");
            //var scheduleInboxURLName = xCalDav.GetName("schedule-inbox-URL");
            //var scheduleOutoxURLName = xCalDav.GetName("schedule-outbox-URL");
            //var addressbookHomeSetName = xCalDav.GetName("addressbook-home-set");

            /*start edit*/
            var availableProps = Props.Properties();
            var returnProps = new List<XElement>();
            foreach (var prop in props)
            {
                if (availableProps.TryGetValue(prop.Name.LocalName, out CaldavProperty propName))
                {
                    returnProps.Add(GetProperty(propName, allprop, calendar));
                }
                else
                {
                    throw new Exception($"Prop: {prop.Name} is not available.");
                }
            }
            /*end edit*/
            
            //copy and relocate
            var calendarUserAddressSetName = xCalDav.GetName("calendar-user-address-set");
            var calendarUserAddress = !allprop && !props.Any(x => x.Name == calendarUserAddressSetName) ? null :
                calendarUserAddressSetName.Element(
                    hrefName.Element(UserUrl),
                    hrefName.Element("mailto:" + GetUserEmail())
                );

            var supportedReportSetName = xDav.GetName("supported-report-set");
            var supportedReportSet = !allprop && !props.Any(x => x.Name == supportedReportSetName) ? null :
                supportedReportSetName.Element(
                    xDav.Element("supported-report", xDav.Element("report", xDav.Element("calendar-multiget")))
                );

            var calendarHomeSetName = xCalDav.GetName("calendar-home-set");
            var calendarHomeSet = !allprop && !props.Any(x => x.Name == calendarHomeSetName) ? null :
                calendarHomeSetName.Element(hrefName.Element(CalendarUrl));

            var getetagName = xDav.GetName("getetag");
            var getetag = !allprop && !props.Any(x => x.Name == getetagName) ? null :
                getetagName.Element();

            var currentUserPrincipalName = xDav.GetName("current-user-principal");
            var currentUserPrincipal = !props.Any(x => x.Name == currentUserPrincipalName) ? null :
                currentUserPrincipalName.Element(hrefName.Element(UserUrl));

            var resourceTypeName = xDav.GetName("resourcetype");
            var resourceType = !allprop && !props.Any(x => x.Name == resourceTypeName) ? null : (
                    resourceTypeName.Element(xDav.Element("collection"), xCalDav.Element("calendar"), xDav.Element("principal"))
                );

            var ownerName = xDav.GetName("owner");
            var owner = !allprop && !props.Any(x => x.Name == ownerName) ? null :
                ownerName.Element(hrefName.Element(UserUrl));

            var displayNameName = xDav.GetName("displayname");
            var displayName = calendar == null || (!allprop && !props.Any(x => x.Name == displayNameName)) ? null :
                displayNameName.Element(calendar.CalendarName ?? calendar.CalendarId);

            var calendarColorName = xApple.GetName("calendar-color");
            var calendarColor = !allprop && !props.Any(x => x.Name == calendarColorName) ? null :
                calendarColorName.Element("FF5800");

            var calendarDescriptionName = xCalDav.GetName("calendar-description");
            var calendarDescription = calendar == null || (!allprop && !props.Any(x => x.Name == calendarDescriptionName)) ? null :
                calendarDescriptionName.Element(calendar.Description);

            var supportedComponentsName = xCalDav.GetName("supported-calendar-component-set");
            var supportedComponents = !allprop && !props.Any(x => x.Name == supportedComponentsName) ? null :
                new[]{
                    xCalDav.Element("comp", new XAttribute("name", "VEVENT")),
                    xCalDav.Element("comp", new XAttribute("name", "VTODO"))
                };

            var getContentTypeName = xDav.GetName("getcontenttype");
            var getContentType = !allprop && !props.Any(x => x.Name == getContentTypeName) ? null :
                getContentTypeName.Element("text/calendar; component=vevent");
            

            /*
            var supportedProperties = new HashSet<XName> {
                resourceTypeName, ownerName, supportedComponentsName, getContentTypeName,
                displayNameName, calendarDescriptionName, calendarColorName,
                currentUserPrincipalName, calendarHomeSetName, calendarUserAddressSetName,
                supportedComponentsName
            };
            */
            var supportedProperties = availableProps;
            var prop404 = xDav.Element("prop", props
                        .Where(p => !supportedProperties.ContainsKey(p.Name.LocalName))
                        .Select(p => new XElement(p.Name))
                );
            var propStat404 = xDav.Element("propstat",
                xDav.Element("status", "HTTP/1.1 404 Not Found"), prop404);

            //@ToDo: look up properties and foreach below inside propstat to product the properties
            var e = new XElement(
                    /*xDav.Element("multistatus",*/ new XElement("multistatus", new XAttribute(XNamespace.Xmlns + "D", xDav.NamespaceName), 
                                                    new XAttribute(XNamespace.Xmlns + "C", xCalDav.NamespaceName),
                                                    new XAttribute(XNamespace.Xmlns + "CS", xCS.NamespaceName),
                    xDav.Element("response",
                    xDav.Element("href", UserUrl /*Request.RawUrl*/),
                    xDav.Element("propstat",
                                xDav.Element("status", "HTTP/1.1 200 OK"),
                                xDav.Element("prop",
                                    returnProps
                                    /*resourceType, owner, supportedComponents, displayName,
                                    getContentType, calendarDescription, calendarHomeSet,
                                    currentUserPrincipal, supportedReportSet, calendarColor,
                                    calendarUserAddress*/
                                )
                            ),

                            (prop404.Elements().Any() ? propStat404 : null)
                     ),

                     (depth == 0 ? null :
                         (_CalendarRepo.GetObjects(calendar).Result
                         .Where(x => x != null)
                         .ToArray()
                            .Select(item => xDav.Element("response",
                                hrefName.Element($"{CalendarUrl}/{item.EventId}"/*GetCalendarObjectUrl(calendar.ID, item.UID)*/),
                                    xDav.Element("propstat",
                                        xDav.Element("status", "HTTP/1.1 200 OK"),
                                        xDav.Element("prop",
                                        resourceType == null ? null : resourceTypeName.Element(),
                                        (getContentType == null ? null : getContentTypeName.Element("text/calendar; component=v" + item.GetType().Name.ToLower())),
                                        getetag == null ? null : getetagName.Element("\"" + XdoxHelpers.FormatDate(item.LastModifiedUtc) + "\"")
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
                    );

            return new XDocument(e);
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

        public async Task<string> UpdateEvent(string user, string calendarName, string fileName, string eventText)
        {
            fileName = fileName.Replace(".ics", "", StringComparison.OrdinalIgnoreCase);
            var cal = Ical.Net.Calendar.Load(eventText);
            if (cal.Events.Count == 1)
            {
                var calendarId = await _CalendarRepo.GetCalendarIdByUserandNameAsync(user, calendarName);
                var icalEvent = cal.Events.First();
                var e = icalEvent.ToCalDavEvent(fileName, calendarId, eventText);
                e.ETag = DateTime.UtcNow.GetHashCode().ToString();
                await _CalendarRepo.UpdateEvent(e);
                var ctag = await _CalendarRepo.UpdateCalendarCTag(calendarId);
                return e.ETag;
            }
            return null;
        }
        public async Task<string> PutEvent(string user, string calendarName, string fileName, string eventText)
        {
            fileName = fileName.Replace(".ics", "", StringComparison.OrdinalIgnoreCase);
            var cal = Ical.Net.Calendar.Load(eventText);
            if (cal.Events.Count == 1)
            {
                var calendarId = await _CalendarRepo.GetCalendarIdByUserandNameAsync(user, calendarName);
                var icalEvent = cal.Events.First();
                var e = icalEvent.ToCalDavEvent(fileName, calendarId, eventText);
                e.ETag = DateTime.UtcNow.GetHashCode().ToString();
                await _CalendarRepo.InsertEvent(e);
                var ctag = await _CalendarRepo.UpdateCalendarCTag(calendarId);
                return e.ETag;
            }
            else
            {
                throw new Exception("More than one event in PUT request");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="calendarName"></param>
        /// <param name="eventFileName"></param>
        /// <returns>Updated CTag for calendar</returns>
        public async Task<string> DeleteObject(string userName, string calendarName, string eventFileName)
        {
            eventFileName = eventFileName.Replace(".ics", "", StringComparison.OrdinalIgnoreCase);
            await _CalendarRepo.DeleteCalendarObject(eventFileName);
            var cId = await _CalendarRepo.GetCalendarIdByUserandNameAsync(userName, calendarName);
            return await _CalendarRepo.UpdateCalendarCTag(cId);
        }

        private XElement GetProperty(CaldavProperty caldavProperty, bool allprop, Calendar calendar)
        {
            XElement outputElement = null;
            
            if (allprop && !caldavProperty.IncludeInAllProp) return outputElement;

            switch (caldavProperty.PropertyName)
            {
                case "calendar-user-address-set":
                    outputElement =
                            caldavProperty.Name().Element(
                            hrefName.Element(UserUrl),
                            hrefName.Element("mailto:" + GetUserEmail())
                            );
                    break;

                case "supported-report-set":
                    outputElement =
                         caldavProperty.Name().Element(
                             xDav.Element("supported-report", xDav.Element("report", xDav.Element("calendar-multiget")))
                         );
                    break;

                case "calendar-home-set":
                    outputElement =
                        caldavProperty.Name().Element(hrefName.Element(CalendarUrl));
                    break;

                case "getetag":
                    outputElement = 
                        caldavProperty.Name().Element();
                    break;

                case "current-user-principal":
                    outputElement = 
                        caldavProperty.Name().Element(hrefName.Element(UserUrl));
                    break;

                case "resourcetype":
                    outputElement =
                        caldavProperty.Name().Element(xDav.Element("collection"), xCalDav.Element("calendar"), xDav.Element("principal"));
                    break;

                case "owner":
                    outputElement =
                        caldavProperty.Name().Element(hrefName.Element(UserUrl));
                    break;

                case "displayname":
                    outputElement = 
                        caldavProperty.Name().Element(calendar.CalendarName ?? calendar.CalendarId);
                    break;

                case "calendar-color":
                    outputElement = 
                        caldavProperty.Name().Element("FF5800");
                    break;

                case "calendar-description":
                    outputElement = 
                        caldavProperty.Name().Element(calendar.Description);
                    break;

                case "supported-calendar-component-set":
                    outputElement = xCalDav.GetName("supported-calendar-component-set").Element(
                        xCalDav.Element("comp", new XAttribute("name", "VEVENT")),
                        xCalDav.Element("comp", new XAttribute("name", "VTODO")));
                    
                    /*var supportedComponentsName = xCalDav.GetName("supported-calendar-component-set");
                    var supportedComponents = !allprop && !props.Any(x => x.Name == supportedComponentsName) ? null :
                        new[]{
                    xCalDav.Element("comp", new XAttribute("name", "VEVENT")),
                    xCalDav.Element("comp", new XAttribute("name", "VTODO"))
                        };
                    outputElement = new[]{
                    xCalDav.Element("comp", new XAttribute("name", "VEVENT")),
                    xCalDav.Element("comp", new XAttribute("name", "VTODO"))
                        };
                    */
                    break;

                case "getcontenttype":
                    outputElement = 
                        caldavProperty.Name().Element("text/calendar; component=vevent");
                    break;
                
                case "getctag":
                    outputElement =
                        caldavProperty.Name().Element(calendar.cTag);
                    break;

                default:
                    break;
            


            }
            return outputElement;
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
        /// <param name="xDoc"></param>
        /// <returns></returns>
        public async Task<XDocument> Report(string userName, string calendarName, string icsFile, XElement request)
        {
            var calendar = await _CalendarRepo.GetCalendarByUserandNameAsync(userName, calendarName);
            var filter = request.Element(xCalDav.GetName("filter"));
            var props = request.Elements(xDav.GetName("prop")).FirstOrDefault().Elements();
            //var result = _Parser.ParseReport(xmlDoc);
            var hrefs = request.Descendants(xDav.GetName("href")).Select(x => x.Value).ToArray();
            string baseUrl = $"/CalDav/Calendars/{userName}/{calendarName}/";
            //return null;


            /*
            var xdoc = GetRequestXml();
			if (xdoc == null) return new Result();

			var repo = GetService<ICalendarRepository>();
			var calendar = repo.GetCalendarByID(id);

			var request = xdoc.Root.Elements().FirstOrDefault();
			var filterElm = request.Element(CalDav.Common.xCalDav.GetName("filter"));
			var filter = filterElm == null ? null : new Filter(filterElm);
			var hrefName = CalDav.Common.xDav.GetName("href");
			var hrefs = xdoc.Descendants(hrefName).Select(x => x.Value).ToArray();
			var getetagName = CalDav.Common.xDav.GetName("getetag");
			var getetag = xdoc.Descendants(getetagName).FirstOrDefault();
			var calendarDataName = CalDav.Common.xCalDav.GetName("calendar-data");
			var calendarData = xdoc.Descendants(calendarDataName).FirstOrDefault();

			var ownerName = Common.xDav.GetName("owner");
			var displaynameName = Common.xDav.GetName("displayname");
            */
			IQueryable<Event> result = null;
            if (filter != null)
            { 
                result = await _CalendarRepo.GetObjectsByFilter(calendar.CalendarId, filter);
            }
            else if (hrefs.Any())
            {
                //var e1 = await _CalendarRepo.GetObjectByUID(calendar.CalendarId, GetObjectUIDFromPath(hrefs.First()));
                var foundEvents=await Task.WhenAll( hrefs.Select(x => _CalendarRepo.GetObjectByUID(calendar.CalendarId, GetObjectUIDFromPath(x))).ToList());
                result = foundEvents.Where(x => x != null).AsQueryable();
                //await _CalendarRepo.GetObjectByUID(calendar.CalendarId, GetObjectUIDFromPath(x));
                
                //IQueryable<Event> foundEvents = await Task.WhenAll(hrefs.Select(x => _CalendarRepo.GetObjectByUID(calendar.CalendarId, GetObjectUIDFromPath(x))));
                //result = hrefs.Select(async x => await _CalendarRepo.GetObjectByUID(calendar.CalendarId, GetObjectUIDFromPath(x))
                //     .Where(x => x != null)
                //     .AsQueryable();
            }
            if (result is not null)
            {
                var e = new XElement(
                    xDav.Element("multistatus", new XAttribute(XNamespace.Xmlns + "D", xDav.NamespaceName),
                                                    new XAttribute(XNamespace.Xmlns + "C", xCalDav.NamespaceName),
                                                    new XAttribute(XNamespace.Xmlns + "CS", xCS.NamespaceName),
                                result.Select(r =>
                                    xDav.Element("response",
                                        xDav.Element("href", new Uri(baseUrl + r.EventId + ".ics",UriKind.Relative)),
                                        xDav.Element("propstat",
                                        xDav.Element("status", "HTTP/1.1 200 OK"),
                                        xDav.Element("prop",
                                            xDav.Element("getetag", r.LastModifiedUtc.GetHashCode()),
                                            xCalDav.Element("calendar-data", r.ICS)
                                            )
                                        )
                                        )
                                    )
                                )
                    );
                return new XDocument(e);
            }
            return null;

                
                    //(getetag == null ? null : CalDav.Common.xDav.Element("getetag", "\"" + Common.FormatDate(r.LastModified) + "\"")),
                    //(calendarData == null ? null : CalDav.Common.xCalDav.Element("calendar-data",
                    //                ToString(r)
                    //                )
                    //                )); 
                    //xDav.Element("multistatus", new XElement("multistatus", new XAttribute(XNamespace.Xmlns + "D", xDav.NamespaceName),
                    //                                new XAttribute(XNamespace.Xmlns + "C", xCalDav.NamespaceName),
                    //                                new XAttribute(XNamespace.Xmlns + "CS", xCS.NamespaceName),
                    //xDav.Element("response",
                    //xDav.Element("href", UserUrl /*Request.RawUrl*/),
                    //xDav.Element("propstat",
                    //            xDav.Element("status", "HTTP/1.1 200 OK"),
                    //            xDav.Element("prop",
                
        }


        //         if (result != null) {
        //	return new Result {
        //		Status = (System.Net.HttpStatusCode)207,
        //		Content = CalDav.Common.xDav.Element("multistatus",
        //		result.Select(r =>
        //		 CalDav.Common.xDav.Element("response",
        //			 CalDav.Common.xDav.Element("href", new Uri(Request.Url, r.UID + ".ics")),
        //			 CalDav.Common.xDav.Element("propstat",
        //				 CalDav.Common.xDav.Element("status", "HTTP/1.1 200 OK"),
        //				 CalDav.Common.xDav.Element("prop",
        //					(getetag == null ? null : CalDav.Common.xDav.Element("getetag", "\"" + Common.FormatDate(r.LastModified) + "\"")),
        //					(calendarData == null ? null : CalDav.Common.xCalDav.Element("calendar-data",
        //						ToString(r)
        //					))
        //				 )
        //			 )
        //		 )
        //		))
        //	};
        //}

        /*
        return new Result {
            Headers = new Dictionary<string, string> {
                {"ETag" , calendar == null ? null : Common.FormatDate( calendar.LastModified ) }
            }
        };
        */

        private string GetObjectUIDFromPath(string path)
        {
            return path.Split("/").Last().Replace(".ics", "", StringComparison.OrdinalIgnoreCase);
        }
    }
}

