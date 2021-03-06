﻿using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CoffeeInvoice.App_Start
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// TODO: Add any additional configuration code.
			config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			// Web API routes
			config.MapHttpAttributeRoutes();
           
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			// WebAPI when dealing with JSON & JavaScript!
			// Setup json serialization to serialize classes to camel (std. Json format)
			var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
			formatter.SerializerSettings.ContractResolver =
				new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

		}
	}
}