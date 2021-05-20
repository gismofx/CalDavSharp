using System;
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
using NodaTime;
using DapperIdentity.Controllers.BasicAuth;

namespace CalDavSharp.Server.Controllers
{

	[ApiController]
	//[Route("[controller]")]
	[Route("calendars")]
	[Route("principals")]
	public class CalDavController : ControllerBase
	{
		private CalDavManager _Manager = null;

		public CalDavController(CalDavManager manager)
		{
			_Manager = manager;
		}

		[ApiExplorerSettings(IgnoreApi = true)]
		public IActionResult Spa()
		{
			return File("~/index.html", "text/html");
		}

		private async Task<XDocument> GetRequestXml()
		{
			var request = HttpContext.Request;
			string body;
			using var reader = new StreamReader(HttpContext.Request.Body);
			{
				body = await reader.ReadToEndAsync();
			}
			return body.Length > 0 ? XDocument.Parse(body) : null;
		}

		[BasicAuth("CalDAV Server")]
		[AcceptVerbs("OPTIONS")]
		[Route("")]
		[Route("{userName}")]
		[Route("{userName}/{calendarName}")]
		public async Task<IActionResult> Options([FromRoute] string userName, [FromRoute] string calendarName)
		{
			var b = await GetRequestXml();
			if (b is not null)
			{
				//	var request = xmlDoc.Root.Elements().FirstOrDefault();
				//	switch (request.Name.LocalName.ToLower())
				//	{
				//		case "calendar-collection-set":
				//			break;
				//			//var repo = GetService<ICalendarRepository>();
				//			//var calendars = repo.GetCalendars().ToArray();
				//			/*
				//			return new Result
				//			{
				//				Content =new XElement("options-response",
				//					new XElement("calendar-collection-set",
				//						calendars.Select(calendar =>
				//						new XElement("href",
				//							 new Uri(Request.Url, GetCalendarUrl(calendar.Name))
				//							 ))
				//				 )
				//			 )
				//			};
				return BadRequest("OPTIONS xml body content not implemented yet");

			}
			else
			{
				Response.Headers.Add("Allow", "OPTIONS, PROPFIND, HEAD, GET, REPORT, PROPPATCH, PUT, DELETE, POST");
				Response.Headers.Add("DAV", "1, 2, access-control, calendar-access");
				//Request.Headers.Add("Allow" "PROPFIND, PROPPATCH, LOCK, UNLOCK, REPORT, ACL"")
				return StatusCode(200);
			}
		}
		
		[BasicAuth("CalDAV Server")]
		[AcceptVerbs("PROPFIND")]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Route("")]
		[Route("{userName}")]
		[Route("{userName}/{calendarName}")]
		public async Task<ActionResult<System.Xml.Linq.XDocument>> PropFind([FromRoute] string userName,
												  [FromRoute] string calendarName,
												  [FromBody] XElement xrequest)
		{
			var headers = HttpContext.Request.Headers;
			var depth = headers["Depth"].Count == 0 ? 0 : int.Parse(headers["Depth"]);
			var request = new XDocument(xrequest);

			var user = HttpContext.User.Identity;

			if (userName is null)
			{
				userName = HttpContext.User.Identity.Name;
			}

			var result = await _Manager.Propfind(HttpContext, depth, HttpContext.Request.Path.ToString(), userName, calendarName, request);
			//ToDo: work on 404 response for bad user name/calendar name is result is null
			return new ContentResult
			{
				Content = result.Declaration.ToString() + result.ToString(),
				ContentType = "text/xml",
				StatusCode = 207
			};


			// StatusCode(201,result);

		}


		[BasicAuth("CalDAV Server")]
		[AcceptVerbs("REPORT")]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Route("{userName}/{calendarName}")]
		[Route("{userName}/{calendarName}/{icsFileName}")]//[FromRoute]
		public async Task<ActionResult<string>> Report([FromRoute] string userName, 
														[FromRoute] string calendarName,
														[FromRoute] string icsFileName,
														[FromBody] XElement request=null)
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
				Content = result.Declaration.ToString() + result.ToString(),
				ContentType = "text/xml",
				StatusCode = 207
			};
			//return result.ToString();
				}

		[BasicAuth("CalDAV Server")]
		[HttpDelete]
		[Route("{userName}/{calendarName}/{icsFileName}")]
		public async Task<IActionResult> Delete([FromRoute] string userName,
										    	[FromRoute] string calendarName,
											    [FromRoute] string icsFileName)
		{
			if (HttpContext.Request.Headers.TryGetValue("If-Match", out var ifMatch))
			{
				var ctag = await _Manager.DeleteObject(userName, calendarName, icsFileName);
				Response.Headers.Add("CTag", ctag);
				return StatusCode(204);
			}

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
		[BasicAuth("CalDAV Server")]
		[HttpPut]
		[Consumes("text/calendar")]
		[Route("{userName}/{calendarName}/{fileName}")]
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
				var etag = await _Manager.PutEvent(userName, calendarName, fileName, body);
				Response.Headers.Add("ETag", etag);
				return StatusCode(201);
			}
		}

		[BasicAuth("CalDAV Server")]
		[AcceptVerbs("MKCALENDAR")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> MkCalendar()
		{
			throw new NotImplementedException();
			return null;
		}

		[BasicAuth("CalDAV Server")]
		[HttpGet]
		[Route("/**.")]
        public async Task<IActionResult> Get()
        {
			throw new NotImplementedException();
            return null;
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
