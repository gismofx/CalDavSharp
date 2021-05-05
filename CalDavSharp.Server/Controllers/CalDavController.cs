﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using CalDavSharp.Server.Services;
using System.Xml.Linq;
using System.IO;
using CalDavSharp.Shared;

namespace CalDavSharp.Server.Controllers
{

	[ApiController]
	[Route("[controller]")]
	public class CalDavController : ControllerBase
	{
		private CalDavManager _Manager = null;

		public CalDavController(CalDavManager manager)
		{
			_Manager = manager;
		}

		public IActionResult Spa()
		{
			return File("~/index.html", "text/html");
		}

		/*
		[AcceptVerbs("OPTIONS")]
		public async Task<IActionResult> Options([FromBody] XDocument xmlDoc)
		{
			//var xmlDoc = GetRequestXml();
			if (xmlDoc != null)
			{
				var request = xmlDoc.Root.Elements().FirstOrDefault();
				switch (request.Name.LocalName.ToLower())
				{
					case "calendar-collection-set":
						break;
						//var repo = GetService<ICalendarRepository>();
						//var calendars = repo.GetCalendars().ToArray();
						/*
						return new Result
						{
							Content =new XElement("options-response",
								new XElement("calendar-collection-set",
									calendars.Select(calendar =>
									new XElement("href",
										 new Uri(Request.Url, GetCalendarUrl(calendar.Name))
										 ))
							 )
						 )
						};
						
				}
			}
		

			var Headers = new Dictionary<string, string> {
					{"Allow", "OPTIONS, PROPFIND, HEAD, GET, REPORT, PROPPATCH, PUT, DELETE, POST" }
				};
			return new ContentResult()
			{
				StatusCode=200,
				Content = "",
				ContentType="text/xml"
			};

			throw new NotImplementedException();
			return null;
		}
		*/

		[AcceptVerbs("PROPFIND")]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Route("calendars")]
		[Route("calendars/{userName:alpha}/{calendarName:alpha}")]
		public async Task<ActionResult<System.Xml.Linq.XDocument>> PropFind([FromRoute] string userName,
												  [FromRoute] string calendarName,
												  [FromBody] XElement xrequest)
		{
			//throw new NotImplementedException();
			if (userName is null && calendarName is null)
			{
				return BadRequest();
			}
			var headers = HttpContext.Request.Headers;
			var depth = headers["Depth"].Count == 0 ? 0 : int.Parse(headers["Depth"]);
			var request = new XDocument(xrequest);

			var result = await _Manager.Propfind(depth, userName, calendarName, request);

			return new ContentResult
			{
				Content = result.ToString(),
				ContentType = "application/xml",
				StatusCode = 207
			};


			// StatusCode(201,result);

		}

		
				//this one works
				[AcceptVerbs("REPORT")]
				[ApiExplorerSettings(IgnoreApi = true)]
				[Route("calendars/{userName:alpha}/{calendarName:alpha}")]
				[Route("calendars/{userName:alpha}/{calendarName:alpha}/{icsFileName}")]//[FromRoute]
				public async Task<ActionResult<string>> Report([FromRoute] string userName, 
															   [FromRoute] string calendarName,
															   [FromRoute] string icsFileName,
															   [FromBody] XElement request)
				{
					//throw new NotImplementedException();
					//return await Task.FromResult(new ActionResult<string>(userName));
					//request.PreserveWhitespace = false;
					if (request is null)
					{
						return null;
					}
			//string icsFile=null;
					var result = await _Manager.Report(userName, calendarName, icsFileName, request);


			//var x = CalendarRepository.FindCalendar


			//var filter = requestDetails.

			//var repo = GetService<ICalendarRepository>();
			//var calendar = repo.GetCalendarByID(id);

			//var request = xdoc.Root.Elements().FirstOrDefault();
			//var filterElm = request.Element(CalDav.Common.xCalDav.GetName("filter"));
			//var filter = filterElm == null ? null : new Filter(filterElm);
			//var hrefName = CalDav.Common.xDav.GetName("href");
			//var hrefs = xdoc.Descendants(hrefName).Select(x => x.Value).ToArray();
			//var getetagName = CalDav.Common.xDav.GetName("getetag");
			//var getetag = xdoc.Descendants(getetagName).FirstOrDefault();
			//var calendarDataName = CalDav.Common.xCalDav.GetName("calendar-data");
			//var calendarData = xdoc.Descendants(calendarDataName).FirstOrDefault();

			//var ownerName = Common.xDav.GetName("owner");
			//var displaynameName = Common.xDav.GetName("displayname");

			//IQueryable<ICalendarObject> result = null;
			//if (filter != null) result = repo.GetObjectsByFilter(calendar, filter);
			//else if (hrefs.Any())
			//	result = hrefs.Select(x => repo.GetObjectByUID(calendar, GetObjectUIDFromPath(x)))
			//		.Where(x => x != null)
			//		.AsQueryable();

			//if (result != null)
			//{
			//	return new Result
			//	{
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

			//return new Result
			//{
			//	Headers = new Dictionary<string, string> {
			//		{"ETag" , calendar == null ? null : Common.FormatDate( calendar.LastModified ) }
			//	}
			//};

			return new ContentResult
			{
				Content = result.ToString(),
				ContentType = "application/xml",
				StatusCode = 207
			};
			//return result.ToString();
				}
		

		[HttpDelete]
		public async Task<IActionResult> Delete()
		{
			throw new NotImplementedException();
			return null;
		}

/*
		[Route("calendars/{userName}/{calendarName}/{icsFile}")]
		public async Task<ActionResult<string>> Put(string userName, string calendarName, string icsFile, [FromBody] string bodyContent)
		{
			throw new NotImplementedException();
			return null;
		}
*/
		
		//[Route("calendars/{userName:alpha}/{calendarName:alpha}/{fileName}")]
		[HttpPut]
		[Consumes("text/calendar")]
		[Route("calendars/{userName}/{calendarName}/{fileName}")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> Put([FromRoute] string userName,
											 [FromRoute] string calendarName,
											 [FromRoute] string fileName)
			                                 //[FromBody] string ics)
		{
			var request = HttpContext.Request;
			string body;// = await Request.  Body.ToString();// Content.ReadAsStringAsync();
			using var reader = new StreamReader(HttpContext.Request.Body);
			{
				body = await reader.ReadToEndAsync();
			}

			if (HttpContext.Request.Headers.TryGetValue("If-Match", out var ifMatch))
			{
				var etag = await _Manager.UpdateEvent(userName, calendarName, fileName, body);
				Response.Headers.Add("ETag", etag);
				return StatusCode(204);
			}
			else
			{
				await _Manager.PutEvent(userName, calendarName, fileName, body);
				return StatusCode(201, "Created");
			}
			

			return StatusCode(201,"Created");
		}
		
		//for debugging
		/*
		[Consumes("text/calendar")]
		[HttpPut]
		[Route("calendars/{*.}/")]
		public async Task<IActionResult> Put()
		{
			Debug.WriteLine(HttpContext.Request.Path);
			throw new NotImplementedException();
			return null;
		}
		*/

		[AcceptVerbs("MKCALENDAR")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> MkCalendar()
		{
			throw new NotImplementedException();
			return null;
		}

		[HttpGet]
        public async Task<IActionResult> Get()
        {
            throw new NotImplementedException();
            return null;
        }

        /*
         * switch (Request.HttpMethod) {
				case "OPTIONS": return Options();
				case "PROPFIND": return PropFind(id);
				case "REPORT": return Report(id);
				case "DELETE": return Delete(id, uid);
				case "PUT": return Put(id, uid);
				case "MKCALENDAR":
					if (DisallowMakeCalendar) return NotImplemented();
					return MakeCalendar(id);
				case "GET": return Get(id, uid);
				default: return NotImplemented();
		*/
    }
}
