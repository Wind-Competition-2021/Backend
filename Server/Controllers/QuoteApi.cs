/*
 * StockQuotes
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 0.1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Attributes;
using Server.Models;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Controllers {
	/// <summary>
	/// 
	/// </summary>
	[ApiController]
	public class QuoteApiController : ControllerBase {
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Get the price summary of a stock</remarks>
		/// <param name="id"></param>
		/// <param name="beginDate"></param>
		/// <param name="endDate"></param>
		/// <param name="rehabilitation">Rehabilitation status, default value is \&quot;none\&quot;</param>
		/// <response code="200">Price summary returned successfully</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet]
		[Route("/truemogician/StockQuote/0.1/api/quote/history/day")]
		[ValidateModelState]
		[SwaggerOperation("GetDailyPrice")]
		[SwaggerResponse(statusCode: 200, type: typeof(List<DailyPrice>), description: "Price summary returned successfully")]
		public virtual IActionResult GetDailyPrice([FromQuery][Required()][RegularExpression("/\\d{6}\\.[a-zA-Z]{2}/")] string id, [FromQuery] DateTime? beginDate, [FromQuery] DateTime? endDate, [FromQuery] string rehabilitation) {
			//TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(200, default(List<DailyPrice>));

			//TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(400);

			//TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(500);
			string exampleJson = null;
			exampleJson = "[ \"\", \"\" ]";

			var example = exampleJson != null
			? JsonConvert.DeserializeObject<List<DailyPrice>>(exampleJson)
			: default(List<DailyPrice>);            //TODO: Change the data returned
			return new ObjectResult(example);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Get the price summary of a stock</remarks>
		/// <param name="id"></param>
		/// <param name="beginDate"></param>
		/// <param name="endDate"></param>
		/// <param name="frequency">Frequency of the quotes</param>
		/// <param name="rehabilitation">Rehabilitation status, default value is \&quot;none\&quot;</param>
		/// <response code="200">Price summary returned successfully</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet]
		[Route("/truemogician/StockQuote/0.1/api/quote/history/minute")]
		[ValidateModelState]
		[SwaggerOperation("GetMinutelyPrice")]
		[SwaggerResponse(statusCode: 200, type: typeof(List<MinutelyPrice>), description: "Price summary returned successfully")]
		public virtual IActionResult GetMinutelyPrice([FromQuery][Required()][RegularExpression("/\\d{6}\\.[a-zA-Z]{2}/")] string id, [FromQuery] DateTime? beginDate, [FromQuery] DateTime? endDate, [FromQuery] int? frequency, [FromQuery] string rehabilitation) {
			//TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(200, default(List<MinutelyPrice>));

			//TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(400);

			//TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(500);
			string exampleJson = null;
			exampleJson = "[ \"\", \"\" ]";

			var example = exampleJson != null
			? JsonConvert.DeserializeObject<List<MinutelyPrice>>(exampleJson)
			: default(List<MinutelyPrice>);            //TODO: Change the data returned
			return new ObjectResult(example);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Get the price summary of a stock</remarks>
		/// <param name="id"></param>
		/// <param name="beginDate"></param>
		/// <param name="endDate"></param>
		/// <param name="frequency">Frequency of the quotes</param>
		/// <param name="rehabilitation">Rehabilitation status, default value is \&quot;none\&quot;</param>
		/// <response code="200">Price summary returned successfully</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet]
		[Route("/truemogician/StockQuote/0.1/api/quote/history/week")]
		[ValidateModelState]
		[SwaggerOperation("GetWeeklyPrice")]
		[SwaggerResponse(statusCode: 200, type: typeof(List<WeeklyPrice>), description: "Price summary returned successfully")]
		public virtual IActionResult GetWeeklyPrice([FromQuery][Required()][RegularExpression("/\\d{6}\\.[a-zA-Z]{2}/")] string id, [FromQuery] DateTime? beginDate, [FromQuery] DateTime? endDate, [FromQuery] string frequency, [FromQuery] string rehabilitation) {
			//TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(200, default(List<WeeklyPrice>));

			//TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(400);

			//TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
			// return StatusCode(500);
			string exampleJson = null;
			exampleJson = "[ \"\", \"\" ]";

			var example = exampleJson != null
			? JsonConvert.DeserializeObject<List<WeeklyPrice>>(exampleJson)
			: default(List<WeeklyPrice>);            //TODO: Change the data returned
			return new ObjectResult(example);
		}
	}
}
