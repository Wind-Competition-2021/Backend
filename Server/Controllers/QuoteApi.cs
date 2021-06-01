using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BaoStock;
using Microsoft.AspNetCore.Mvc;
using Server.Attributes;
using Server.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers {
	/// <summary>
	/// </summary>
	[ApiController]
	public class QuoteApiController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="baoStock"></param>
		public QuoteApiController(BaoStockManager baoStock) => BaoStock = baoStock;

		/// <summary>
		/// </summary>
		protected BaoStockManager BaoStock { get; }

		/// <summary>
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		[HttpGet("/api/quote/trade-status")]
		[ValidateModelState]
		[SwaggerOperation("CheckTradeStatus")]
		[SwaggerResponse(200, type: typeof(bool), description: "Trading status checked successfully")]
		public IActionResult CheckTradeStatus([FromQuery] DateTime? date) => Ok(BaoStock.Fetch<bool>("checkTradeStatus", date?.ToString("yyyy-MM-dd")));

		/// <summary>
		/// </summary>
		/// <remarks>Get the price summary of a stock</remarks>
		/// <param name="id"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <param name="rehabilitation">Rehabilitation status, default value is "none"</param>
		/// <response code="200">Get the quote of a stock every trading day</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet("/api/quote/history/day")]
		[ValidateModelState]
		[SwaggerOperation("GetDailyPrice")]
		[SwaggerResponse(200, type: typeof(List<DailyPrice>), description: "Get the quote of a stock every trading day")]
		public IActionResult GetDailyPrice(
			[FromQuery] [Required] [RegularExpression(@"[a-zA-Z]{2}\.\d{6}")]
			string id,
			[FromQuery] DateTime? begin = null,
			[FromQuery] DateTime? end = null,
			[FromQuery] string rehabilitation = "none"
		) {
			end ??= DateTime.Now.Date;
			begin ??= end - TimeSpan.FromDays(30);
			return Ok(
				BaoStock.Fetch<DailyPrice[]>(
					"getDailyPrice",
					id,
					begin.Value.ToString("yyyy-MM-dd"),
					end.Value.ToString("yyyy-MM-dd"),
					rehabilitation
				)
			);
		}

		/// <summary>
		/// </summary>
		/// <remarks>Get the price summary of a stock</remarks>
		/// <param name="id"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <param name="frequency">Frequency of the quotes</param>
		/// <param name="rehabilitation">Rehabilitation status, default value is "none"</param>
		/// <response code="200">Get the quote of a stock every several minutes</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet("/api/quote/history/minute")]
		[ValidateModelState]
		[SwaggerOperation("GetMinutelyPrice")]
		[SwaggerResponse(200, type: typeof(List<MinutelyPrice>), description: "Get the quote of a stock every several minutes")]
		public IActionResult GetMinutelyPrice(
			[FromQuery] [Required] [RegularExpression(@"[a-zA-Z]{2}\.\d{6}")]
			string id,
			[FromQuery] DateTime? begin = null,
			[FromQuery] DateTime? end = null,
			[FromQuery] int frequency = 60,
			[FromQuery] string rehabilitation = "none"
		) {
			end ??= DateTime.Now;
			begin ??= end - TimeSpan.FromMinutes(frequency * 30);
			return Ok(
				BaoStock.Fetch<MinutelyPrice[]>(
					"getDailyPrice",
					id,
					begin.Value.ToString("yyyy-MM-dd"),
					end.Value.ToString("yyyy-MM-dd"),
					frequency.ToString(),
					rehabilitation
				)
			);
		}

		/// <summary>
		/// </summary>
		/// <remarks>Get the price summary of a stock</remarks>
		/// <param name="id"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <param name="frequency">Frequency of the quotes</param>
		/// <param name="rehabilitation">Rehabilitation status, default value is "none"</param>
		/// <response code="200">Get the quote of a stock every week or month</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet("/api/quote/history/week")]
		[ValidateModelState]
		[SwaggerOperation("GetWeeklyPrice")]
		[SwaggerResponse(200, type: typeof(List<WeeklyPrice>), description: "Get the quote of a stock every week or month")]
		public IActionResult GetWeeklyPrice(
			[FromQuery] [Required] [RegularExpression(@"[a-zA-Z]{2}\.\d{6}")]
			string id,
			[FromQuery] DateTime? begin = null,
			[FromQuery] DateTime? end = null,
			[FromQuery] string frequency = "month",
			[FromQuery] string rehabilitation = "none"
		) {
			end ??= DateTime.Now;
			begin ??= end - TimeSpan.FromDays((frequency == "month" ? 30 : 7) * 30);
			return Ok(
				BaoStock.Fetch<MinutelyPrice[]>(
					"getDailyPrice",
					id,
					begin.Value.ToString("yyyy-MM-dd"),
					end.Value.ToString("yyyy-MM-dd"),
					frequency,
					rehabilitation
				)
			);
		}
	}
}