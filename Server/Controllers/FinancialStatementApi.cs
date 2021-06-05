using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BaoStock;
using Microsoft.AspNetCore.Mvc;
using Server.Attributes;
using Server.Models;
using Shared;
using Swashbuckle.AspNetCore.Annotations;
using Api = BaoStock.BaoStockManager.Api;

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
			=> Ok(BaoStock.Fetch<Profitability>(Api.Profitability, (StockId)id, year, quarter));

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
			=> Ok(BaoStock.Fetch<OperationalCapability>(Api.OperationalCapability, (StockId)id, year, quarter));

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
			=> Ok(BaoStock.Fetch<GrowthAbility>(Api.GrowthAbility, (StockId)id, year, quarter));

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
			=> Ok(BaoStock.Fetch<Solvency>(Api.Solvency, (StockId)id, year, quarter));

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
			=> Ok(BaoStock.Fetch<CashFlow>(Api.CashFlow, (StockId)id, year, quarter));

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
			=> Ok(BaoStock.Fetch<PerformanceReport[]>(Api.PerformanceReport, (StockId)id, begin, end));

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
			=> Ok(BaoStock.Fetch<PerformanceForecast[]>(Api.PerformanceForecast, (StockId)id, begin, end));
	}
}