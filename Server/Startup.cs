using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Colorful;
using Initiator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Server.Filters;
using Server.Managers;
using Server.Security;
using Tushare;
using Console = Colorful.Console;

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
		/// <param name="configuration"></param>
		public Startup(IWebHostEnvironment env, IConfiguration configuration) {
			_hostingEnv = env;
			Configuration = configuration;
		}

		private IConfiguration Configuration { get; }

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

			//Inject ConfigurationManager
			services.AddSingleton(new ConfigurationManager());

			//Inject Fetcher
			string fetcherRoot = Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent!.FullName, "Fetcher");
			string venvPath = Path.Combine(fetcherRoot, "Venv");
			string pythonPath = Directory.GetDirectories(venvPath)
				.Select(path => Directory.GetFiles(path, "python*"))
				.Aggregate(null as string, (path, next) => next.Length > 0 ? next[0] : path);
			var processInfo = new ProcessStartInfo {
				FileName = $"\"{pythonPath ?? throw new FileNotFoundException("Python not found in Venv")}\"",
				Arguments = $"\"{Path.Combine(fetcherRoot, "Fetcher.py")}\"",
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				StandardOutputEncoding = Encoding.UTF8,
				RedirectStandardError = true
			};
			var process = Process.Start(processInfo);
			process!.Exited += (_, _) => {
				Console.Error.WriteLine("Fetcher exited", Color.Red);
				if (!process.StandardError.EndOfStream)
					Console.Error.WriteLine(process.StandardError.ReadToEnd(), Color.Red);
				process = Process.Start(processInfo);
			};
			services.AddSingleton(process);

			//Inject custom serializer settings
			var settings = new JsonSerializerSettings() {
				Error = (_, args) => {
					Console.WriteLine($"Deserialization Error: {JsonConvert.SerializeObject(args.ErrorContext)}", Color.Red);
					process.Kill();
					process.Start();
				}
			};
			services.AddSingleton(settings);

			//Inject Tushare
			var tushare = new TushareManager("ecffe13bdfb4ccb617b344f276b4827d3614e0a736a5fe7c0c6767ce", settings);
			services.AddSingleton(tushare);

			//Inject RealtimeQuotesManager
			var realtimeQuotesManager = new RealtimeQuotesManager(tushare);
			services.AddSingleton(realtimeQuotesManager);

			//Inject QuickQuotesInitiator
			var initiator = new StockQuotesInitiator(@"..\Initiator\Config\Initiator.cfg");
			initiator.DefaultSession = initiator.Sessions.First();
			initiator.Start();
			initiator.LogIn();
			//initiator.UntilLoggedIn().ContinueWith(_ => initiator.RequestMarketData(StockQuotesInitiator.MarketDataRequestType.RealTime, DateTime.Now.Date, DateTime.Now.Date));
			services.AddSingleton(initiator);
		}

		/// <summary>
		///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		/// <param name="loggerFactory"></param>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory) {
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else {
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}
			app.UseRouting();
			app.Use(
				async (context, next) => {
					Console.WriteLineFormatted(
						"{0} {1} {2}",
						Color.Azure,
						new Formatter(DateTime.Now.ToString(), Color.HotPink),
						new Formatter(context.Request.Method, Color.Cyan),
						new Formatter(context.Request.GetDisplayUrl(), Color.MediumSpringGreen)
					);
					await next();
				}
			);
			app.UseSwagger();
			app.UseAuthorization();
			app.UseWebSockets();
			//app.UseFileServer();
			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}