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
		/*
		public IActionResult Spa()
		{
			return File("~/index.html", "text/html");
		}*/

		[AcceptVerbs("OPTIONS")]
		public async Task<IActionResult> Options()
		{
			throw new NotImplementedException();
			return null;
		}

		
		[AcceptVerbs("PROPFIND")]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Route("calendars")]
		[Route("calendars/{userName:alpha}/{calendarName:alpha}")]
		public async Task<ActionResult<System.Xml.Linq.XDocument>> PropFind([FromRoute] string userName, 
											      [FromRoute] string calendarName, 
												  [FromBody] XmlDocument request)
		{
			//throw new NotImplementedException();
			if (userName is null && calendarName is null)
			{
				return BadRequest();
			}

			var result = _Manager.Propfind(userName, calendarName, request);

			return new ContentResult
			{
				Content = result.ToString(),
				ContentType = "text/xml",
				StatusCode = 201
			};


			// StatusCode(201,result);

		}
		

		//this one works
		[AcceptVerbs("REPORT")]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Route("calendars/{userName:alpha}/{calendarName:alpha}")]
		public async Task<ActionResult<string>> Report([FromRoute] string userName, 
													   [FromRoute] string calendarName, 
													   [FromBody] XmlDocument request)
		{
			//throw new NotImplementedException();
			//return await Task.FromResult(new ActionResult<string>(userName));
			//request.PreserveWhitespace = false;
			if (request is null)
			{
				return null;
			}
			var result = _Manager.Report(userName, calendarName, request);


			//var x = CalendarRepository.FindCalendar

			var filter = request.GetElementsByTagName("c:filter");
			//var filter = requestDetails.
			/*
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

			IQueryable<ICalendarObject> result = null;
			if (filter != null) result = repo.GetObjectsByFilter(calendar, filter);
			else if (hrefs.Any())
				result = hrefs.Select(x => repo.GetObjectByUID(calendar, GetObjectUIDFromPath(x)))
					.Where(x => x != null)
					.AsQueryable();

			if (result != null)
			{
				return new Result
				{
					Status = (System.Net.HttpStatusCode)207,
					Content = CalDav.Common.xDav.Element("multistatus",
					result.Select(r =>
					 CalDav.Common.xDav.Element("response",
						 CalDav.Common.xDav.Element("href", new Uri(Request.Url, r.UID + ".ics")),
						 CalDav.Common.xDav.Element("propstat",
							 CalDav.Common.xDav.Element("status", "HTTP/1.1 200 OK"),
							 CalDav.Common.xDav.Element("prop",
								(getetag == null ? null : CalDav.Common.xDav.Element("getetag", "\"" + Common.FormatDate(r.LastModified) + "\"")),
								(calendarData == null ? null : CalDav.Common.xCalDav.Element("calendar-data",
									ToString(r)
								))
							 )
						 )
					 )
					))
				};
			}

			return new Result
			{
				Headers = new Dictionary<string, string> {
					{"ETag" , calendar == null ? null : Common.FormatDate( calendar.LastModified ) }
				}
			};

*/
			return await Task.FromResult(new ActionResult<string>(userName));
		}


		[HttpDelete]
		public async Task<IActionResult> Delete()
		{
			throw new NotImplementedException();
			return null;
		}

		[HttpPut]
		public async Task<IActionResult> Put()
		{
			throw new NotImplementedException();
			return null;
		}

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
