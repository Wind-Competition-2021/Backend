using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Attributes;
using Server.Managers;
using Server.Models;
using Server.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers {
	/// <summary>
	/// </summary>
	[ApiController]
	public sealed class ConfigApiController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="manager"></param>
		public ConfigApiController(ConfigurationManager manager) => ConfigurationManager = manager;

		/// <summary>
		/// </summary>
		public ConfigurationManager ConfigurationManager { get; init; }

		/// <summary>
		/// </summary>
		/// <remarks>Create or replace the configuration of a token</remarks>
		/// <param name="body">Configuration data</param>
		/// <response code="201">Configuration has been saved</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="401">Unauthorized</response>
		/// <response code="500">Unknown error on server</response>
		[HttpPut("/api/config")]
		[Authorize(AuthenticationSchemes = TokenHeaderAuthenticationHandler.SchemeName)]
		[ValidateModelState]
		[SwaggerOperation("CreateConfig")]
		public IActionResult CreateConfig([FromBody] Configuration body) {
			ConfigurationManager[Request.Headers["token"]] = body;
			return StatusCode(201);
		}

		/// <summary>
		/// </summary>
		/// <remarks>Get the configuration of given token or default one if token not provided</remarks>
		/// <param name="token"></param>
		/// <response code="200">Configuration returned successfully</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="500">Unknown error on server</response>
		[HttpGet("/api/config")]
		[ValidateModelState]
		[SwaggerOperation("GetConfig")]
		[SwaggerResponse(200, type: typeof(Configuration), description: "Configuration returned successfully")]
		public IActionResult GetConfig([FromQuery] string token) {
			if (string.IsNullOrEmpty(token))
				return Ok(ConfigurationManager.DefaultConfiguration);
			if (!ConfigurationManager.Contains(token))
				return BadRequest("Token doesn't exist");
			return Ok(ConfigurationManager[token]);
		}

		/// <summary>
		/// </summary>
		/// <remarks>Modify part of the configuration of a token</remarks>
		/// <param name="body">Part of the configuration that needs updating</param>
		/// <response code="200">Configuration has been updated</response>
		/// <response code="400">Invalid parameters or payload</response>
		/// <response code="401">Unauthorized</response>
		/// <response code="500">Unknown error on server</response>
		[HttpPatch("/api/config")]
		[Authorize(AuthenticationSchemes = TokenHeaderAuthenticationHandler.SchemeName)]
		[ValidateModelState]
		[SwaggerOperation("ModifyConfig")]
		public IActionResult ModifyConfig([FromBody] Configuration body) {
			var config = ConfigurationManager[Request.Headers["token"]];
			if (body.RefreshInterval != null) {
				if (body.RefreshInterval.Trend.HasValue)
					config.RefreshInterval.Trend = body.RefreshInterval.Trend;
				if (body.RefreshInterval.List.HasValue)
					config.RefreshInterval.Trend = body.RefreshInterval.List;
			}
			if (body.PinnedStocks != null)
				config.PinnedStocks = body.PinnedStocks;

			return Ok();
		}
	}
}