using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Attributes;
using Server.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers {
	/// <summary>
	/// </summary>
	[ApiController]
	public class StockApiController : FetcherController {
		/// <inheritdoc cref="FetcherController(Process,JsonSerializerSettings)"/>
		public StockApiController(Process fetcher, JsonSerializerSettings settings) : base(fetcher, settings) { }

		/// <summary>
		/// </summary>
		/// <remarks>Get the information of a stock</remarks>
		/// <param name="id"></param>
		/// <response code="200">Information returned successfully</response>
		/// <response code="404">Resource not found</response>
		[HttpGet("/api/stock/info")]
		[ValidateModelState]
		[SwaggerOperation("GetStockInfo")]
		[SwaggerResponse(200, type: typeof(StockInfo), description: "Information returned successfully")]
		public IActionResult GetStockInfo(
			[FromQuery] [Required] [RegularExpression(@"[a-zA-Z]{2}\.\d{6}")]
			string id
		)
			=> Ok(Fetch<StockInfo>("getStockInfo", new StockId(id)));

		/// <summary>
		/// </summary>
		/// <remarks>Get the list of stocks</remarks>
		/// <param name="type"></param>
		/// <param name="date">Only meaningful when querying constituent stock list. Default value is today</param>
		/// <response code="200">List returned successfully</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet("/api/stock/list")]
		[ValidateModelState]
		[SwaggerOperation("GetStockList")]
		[SwaggerResponse(200, type: typeof(List<StockBasicInfo>), description: "List returned successfully")]
		public IActionResult GetStockList([FromQuery] string type = "default", [FromQuery] DateTime? date = null) {
			date ??= DateTime.Now.Date;
			return Ok(Fetch<StockBasicInfo[]>("getStockList", type, date.Value.ToString("yyyy-MM-dd")));
		}
	}
}