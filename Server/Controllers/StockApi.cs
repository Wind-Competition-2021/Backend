using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BaoStock;
using Microsoft.AspNetCore.Mvc;
using Server.Attributes;
using Server.Models;
using Shared;
using Swashbuckle.AspNetCore.Annotations;
using Tushare;
using Api = BaoStock.BaoStockManager.Api;

namespace Server.Controllers {
	/// <summary>
	/// </summary>
	[ApiController]
	public class StockApiController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="baoStock"></param>
		/// <param name="tushare"></param>
		public StockApiController(BaoStockManager baoStock, TushareManager tushare) {
			BaoStock = baoStock;
			Tushare = tushare;
		}

		/// <summary>
		/// </summary>
		protected BaoStockManager BaoStock { get; }

		/// <summary>
		/// 
		/// </summary>
		protected TushareManager Tushare { get; }

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
		public async Task<IActionResult> GetStockInfo(
			[FromQuery] [Required] [RegularExpression(@"[a-zA-Z]{2}\.\d{6}")]
			string id
		) {
			var info = BaoStock.Fetch<StockInfo>(Api.StockInfo, (StockId)id);
			if (info.Type == StockInfo.SecurityType.Stock) {
				var extra = await Tushare.GetCompanyInformation(id);
				info.RegisteredCapital = extra.RegisterCapital;
				info.LegalRepresentative = extra.LegalRepresentative;
				info.GeneralManager = extra.GeneralManager;
				info.Secretary = extra.Secretary;
				info.EmployeeCount = extra.EmployeeCount;
				info.Province = extra.Province;
				info.City = extra.City;
				info.Office = extra.Office;
				info.Email = extra.Email;
				info.Website = extra.Website;
				info.BusinessScope = extra.BusinessScope;
				info.MainBusiness = extra.MainBusiness;
				info.Introduction = extra.Introduction;
			}
			return Ok(info);
		}

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
			return Ok(BaoStock.Fetch<StockBasicInfo[]>(Api.StockList, type, date));
		}
	}
}