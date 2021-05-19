using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
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
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Server.Filters;
using Server.Managers;
using Server.Security;
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

			//Inject ConfigManager
			services.AddSingleton(new ConfigManager());

			//Inject Fetcher
			var fetcherRoot = new DirectoryInfo(Directory.GetCurrentDirectory());
			var fetcherPath = Path.Combine(fetcherRoot!.Parent!.FullName, "Fetcher");
			var processInfo = new ProcessStartInfo {
				FileName = $"\"{Path.Combine(fetcherPath, @"Venv\Scripts\python")}\"",
				Arguments = $"\"{Path.Combine(fetcherPath, "Fetcher.py")}\"",
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				StandardOutputEncoding = Encoding.UTF8,
				RedirectStandardError = true
			};
			var process = Process.Start(processInfo);
			process!.Exited += (sender, e) => {
				Console.Error.WriteLine("Fetcher exited", Color.Red);
				if (!process.StandardError.EndOfStream)
					Console.Error.WriteLine(process.StandardError.ReadToEnd(), Color.Red);
				process = Process.Start(processInfo);
			};
			services.AddSingleton(process);

			//Inject QuickQuotesInitiator
			var initiator = new StockQuotesInitiator(@"..\Initiator\Config\Initiator.cfg");
			initiator.Start();
			initiator.LogIn();
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
					Console.Write(context.Request.Method, Color.Gold);
					Console.WriteLine($" {context.Request.GetDisplayUrl()}", Color.Cyan);
					Console.ResetColor();
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