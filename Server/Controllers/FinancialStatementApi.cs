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
	public class FinancialStatementApiController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="baoStock"></param>
		public FinancialStatementApiController(BaoStockManager baoStock) => BaoStock = baoStock;

		/// <summary>
		/// </summary>
		protected BaoStockManager BaoStock { get; }

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="year">Default value is current year</param>
		/// <param name="quarter">Default value is current quarter</param>
		/// <response code="200">Success</response>
		/// <response code="400">Invalid parameters or payload</response>
		[HttpGet]
		[Route("/api/statement/cash-flow")]
		[ValidateModelState]
		[SwaggerOperation("GetCashFlow")]
		[SwaggerResponse(200, type: typeof(CashFlow), description: "Success")]
		public IActionResult GetCashFlow(
			[FromQuery] [Required] [RegularExpression(@"^[a-zA-Z]{2}\.\d{6}$")]
			string id,
			[FromQuery] int? year,
			[FromQuery] int? quarter
		)
			=> Ok(BaoStock.Fetch<CashFlow>("getCashFlow", new StockId(id), year?.ToString(), quarter?.ToString()));

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="year">Default value is current year</param>
		/// <param name="quarter">Default value is current quarter</param>
		/// <response code="200">Success</response>
		/// <response code="400">Invalid parameters or payload</response>
		[HttpGet]
		[Route("/api/statement/growth-ability")]
		[ValidateModelState]
		[SwaggerOperation("GetGrowthAbility")]
		[SwaggerResponse(200, type: typeof(GrowthAbility), description: "Success")]
		public IActionResult GetGrowthAbility(
			[FromQuery] [Required] [RegularExpression(@"^[a-zA-Z]{2}\.\d{6}$")]
			string id,
			[FromQuery] int? year,
			[FromQuery] int? quarter
		)
			=> Ok(BaoStock.Fetch<GrowthAbility>("getGrowthAbility", new StockId(id), year?.ToString(), quarter?.ToString()));

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="year">Default value is current year</param>
		/// <param name="quarter">Default value is current quarter</param>
		/// <response code="200">Success</response>
		/// <response code="400">Invalid parameters or payload</response>
		[HttpGet]
		[Route("/api/statement/operational-capability")]
		[ValidateModelState]
		[SwaggerOperation("GetOperationalCapability")]
		[SwaggerResponse(200, type: typeof(OperationalCapability), description: "Success")]
		public IActionResult GetOperationalCapability(
			[FromQuery] [Required] [RegularExpression(@"^[a-zA-Z]{2}\.\d{6}$")]
			string id,
			[FromQuery] int? year,
			[FromQuery] int? quarter
		)
			=> Ok(BaoStock.Fetch<OperationalCapability>("getOperationalCapability", new StockId(id), year?.ToString(), quarter?.ToString()));

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <response code="200">Success</response>
		/// <response code="400">Invalid parameters or payload</response>
		[HttpGet]
		[Route("/api/statement/forecast")]
		[ValidateModelState]
		[SwaggerOperation("GetPerformanceForecast")]
		[SwaggerResponse(200, type: typeof(List<PerformanceForecast>), description: "Success")]
		public IActionResult GetPerformanceForecast(
			[FromQuery] [Required] [RegularExpression(@"^[a-zA-Z]{2}\.\d{6}$")]
			string id,
			[FromQuery] DateTime? begin,
			[FromQuery] DateTime? end
		)
			=> Ok(BaoStock.Fetch<PerformanceForecast>("getPerformanceForecast", new StockId(id), begin?.ToString("yyyy-MM-dd"), end?.ToString("yyyy-MM-dd")));

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <response code="200">Success</response>
		/// <response code="400">Invalid parameters or payload</response>
		[HttpGet]
		[Route("/api/statement/report")]
		[ValidateModelState]
		[SwaggerOperation("GetPerformanceReport")]
		[SwaggerResponse(200, type: typeof(List<PerformanceReport>), description: "Success")]
		public IActionResult GetPerformanceReport(
			[FromQuery] [Required] [RegularExpression(@"^[a-zA-Z]{2}\.\d{6}$")]
			string id,
			[FromQuery] DateTime? begin,
			[FromQuery] DateTime? end
		)
			=> Ok(BaoStock.Fetch<PerformanceReport>("getPerformanceReport", new StockId(id), begin?.ToString("yyyy-MM-dd"), end?.ToString("yyyy-MM-dd")));

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="year">Default value is current year</param>
		/// <param name="quarter">Default value is current quarter</param>
		/// <response code="200">Success</response>
		/// <response code="400">Invalid parameters or payload</response>
		[HttpGet]
		[Route("/api/statement/profitability")]
		[ValidateModelState]
		[SwaggerOperation("GetProfitability")]
		[SwaggerResponse(200, type: typeof(Profitability), description: "Success")]
		public IActionResult GetProfitability(
			[FromQuery] [Required] [RegularExpression(@"^[a-zA-Z]{2}\.\d{6}$")]
			string id,
			[FromQuery] int? year,
			[FromQuery] int? quarter
		)
			=> Ok(BaoStock.Fetch<Profitability>("getProfitability", new StockId(id), year?.ToString(), quarter?.ToString()));

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="year">Default value is current year</param>
		/// <param name="quarter">Default value is current quarter</param>
		/// <response code="200">Success</response>
		/// <response code="400">Invalid parameters or payload</response>
		[HttpGet]
		[Route("/api/statement/solvency")]
		[ValidateModelState]
		[SwaggerOperation("GetSolvency")]
		[SwaggerResponse(200, type: typeof(Solvency), description: "Success")]
		public IActionResult GetSolvency(
			[FromQuery] [Required] [RegularExpression(@"^[a-zA-Z]{2}\.\d{6}$")]
			string id,
			[FromQuery] int? year,
			[FromQuery] int? quarter
		)
			=> Ok(BaoStock.Fetch<Solvency>("getSolvency", new StockId(id), year?.ToString(), quarter?.ToString()));
	}
}