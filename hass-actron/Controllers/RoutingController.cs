
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

namespace HMX.HASSActron.Controllers
{
	public class RoutingController : Controller
	{
		[Route("/{**path}")]
		public IActionResult RouteRequest()
		{
			ContentResult result = new ContentResult();

			Logging.WriteDebugLog("RoutingController.RouteRequest() Client: {0}:{1}, Path: {2}", HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Connection.RemotePort.ToString(), HttpContext.Request.Path);

			result.ContentType = "text/html";
			result.StatusCode = 200;

			result.Content = "Request URI: " + HttpContext.Request.Path;
			result.Content = "Request Query: " + HttpContext.Request.QueryString;
			result.Content = "Request Method: " + HttpContext.Request.Method;

			return result;
		}
	}
}
