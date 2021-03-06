using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using BaoStock;
using Colorful;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Server.Filters;
using Server.Managers;
using Server.Security;
using StackExchange.Redis;
using Tushare;
using Console = Colorful.Console;

#if INITIATOR
using System.Linq;
using Initiator;
#endif

namespace Server {
	/// <summary>
	///     Startup
	/// </summary>
	public class Startup {
		private readonly IWebHostEnvironment _hostingEnv;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="env"></param>
		public Startup(IWebHostEnvironment env) => _hostingEnv = env;

		/// <summary>
		///     This method gets called by the runtime. Use this method to add services to the container.
		/// </summary>
		/// <param name="services"></param>
		public void ConfigureServices(IServiceCollection services) {
			// Add framework services.
			services.AddMvc(
					options => {
						options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
						options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
					}
				)
				.AddNewtonsoftJson(
					opts => {
						opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
						opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
						opts.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;
					}
				)
				.AddXmlSerializerFormatters();

			services.AddAuthentication(TokenHeaderAuthenticationHandler.SchemeName)
				.AddScheme<AuthenticationSchemeOptions, TokenHeaderAuthenticationHandler>(TokenHeaderAuthenticationHandler.SchemeName, null)
				.AddScheme<AuthenticationSchemeOptions, TokenQueryAuthenticationHandler>(TokenQueryAuthenticationHandler.SchemeName, null);

			services
				.AddSwaggerGen(
					c => {
						c.SwaggerDoc(
							"0.1",
							new OpenApiInfo {
								Version = "0.1",
								Title = "StockQuotes",
								Description = "StockQuotes (ASP.NET Core 3.1)",
								Contact = new OpenApiContact {
									Name = "Swagger Codegen Contributors",
									Url = new Uri("https://github.com/swagger-api/swagger-codegen"),
									Email = ""
								}
							}
						);
						c.CustomSchemaIds(type => type.FullName);
						c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");
						// Sets the basePath property in the Swagger document generated
						c.DocumentFilter<BasePathFilter>("");

						// Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
						// Use [ValidateModelState] on Actions to actually validate it in C# as well!
						c.OperationFilter<GeneratePathParamsValidationFilter>();
					}
				);
			string projectPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
			var config = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(File.ReadAllText(Path.Combine(projectPath!, "config.json")));

			//Inject ConfigurationManager
			services.AddSingleton(new ConfigurationManager());

			//Inject custom serializer settings
			var settings = new JsonSerializerSettings();
			services.AddSingleton(settings);

			//Redis
			var connection = ConnectionMultiplexer.Connect("localhost:6379");
			var redis = connection.GetDatabase();

			//Inject BaoStock
			var baoStock = new BaoStockManager(redis, settings);
			settings.Error += (_, args) => {
				Console.WriteLine($"Deserialization Error: {JsonConvert.SerializeObject(args.ErrorContext)}", Color.Red);
				baoStock.Process.Kill();
				baoStock.Process.Start();
			};
			services.AddSingleton(baoStock);

			//Inject Tushare
			var tushare = new TushareManager(config!["Tushare"].ToObject<Dictionary<string, JValue>>()!["token"].Value as string, redis, settings);
			services.AddSingleton(tushare);

			//Inject RealtimeQuotesManager
			var realtimeQuotesManager = new RealtimeQuotesManager(TimeSpan.FromSeconds(5), tushare);
			services.AddSingleton(realtimeQuotesManager);

			//Inject PlaybackQuotesManager
			var playbackQuotesManager = new PlaybackQuotesManager(baoStock);
			services.AddSingleton(playbackQuotesManager);

			#if INITIATOR
			//Inject QuickQuotesInitiator
			var initiator = new StockQuotesInitiator();
			initiator.DefaultSession = initiator.Sessions.First();
			services.AddSingleton(initiator);
			initiator.Start();
			initiator.LogIn();
			initiator.UntilLoggedIn().ContinueWith(_ => initiator.RequestMarketData(StockQuotesInitiator.MarketDataRequestType.RealTime, DateTime.Now.Date, DateTime.Now.Date));
			#endif
		}

		/// <summary>
		///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		/// <param name="loggerFactory"></param>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory) {
			if (loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseHsts();
			app.UseRouting();
			app.Use(
				async (context, next) => {
					Console.WriteLineFormatted(
						"{0} {1} {2}",
						Color.Azure,
						new Formatter(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"), Color.HotPink),
						new Formatter(context.Request.Method, Color.Cyan),
						new Formatter(context.Request.GetDisplayUrl(), Color.MediumSpringGreen)
					);
					await next();
				}
			);
			app.UseSwagger();
			app.UseAuthorization();
			app.UseWebSockets();
			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}