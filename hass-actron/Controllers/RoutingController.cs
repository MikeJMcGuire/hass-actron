
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace HMX.HASSActron.Controllers
{
	public class RoutingController : Controller
	{
		[Route("/{**path}")]
		public IActionResult RouteRequest()
		{
			ContentResult result = new ContentResult();
			StringBuilder sbContent = new StringBuilder();

			Logging.WriteDebugLog("RoutingController.RouteRequest() Client: {0}:{1}, Path: {2}", HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Connection.RemotePort.ToString(), HttpContext.Request.Path);

			result.ContentType = "text/html";
			result.StatusCode = 200;

			sbContent.Append("Request URI: " + HttpContext.Request.Path + "<br />");
			sbContent.Append("Request Query: " + HttpContext.Request.QueryString + "<br />");
			sbContent.Append("Request Method: " + HttpContext.Request.Method + "<br />");
			sbContent.Append("Request Host: " + HttpContext.Request.Host + "<br />");

			result.Content = sbContent.ToString();

			return result;
		}
	}
}
