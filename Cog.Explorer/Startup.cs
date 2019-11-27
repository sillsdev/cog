using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using EmbeddedBlazorContent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SIL.Cog.Domain;
using SIL.Cog.Explorer.Services;

namespace SIL.Cog.Explorer
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor().AddCircuitOptions(options => options.DetailedErrors = true);
			services.AddSingleton<SegmentPool>();
			services.AddSingleton<ProjectService>();
			services.AddTransient<DomService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			app.UseStaticFiles();

			app.UseRouting();

			app.UseEmbeddedBlazorContent(typeof(MatBlazor.BaseMatComponent).Assembly);

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});

			Task.Run(async () =>
			{
				Display display = await Electron.Screen.GetPrimaryDisplayAsync();
				int x = (display.WorkAreaSize.Width - 800) / 2;
				int y = (display.WorkAreaSize.Height - 600) / 2;
				BrowserWindow browserWindow = await Electron.WindowManager.CreateWindowAsync(
					new BrowserWindowOptions
					{
						X = x + 7,
						Y = y,
						Width = 800,
						Height = 600,
						Show = false,
						Frame = false,
						WebPreferences = new WebPreferences
						{
							WebSecurity = false,
							NodeIntegration = true,
							DefaultFontSize = 16
						}
					}, "");
				browserWindow.OnReadyToShow += () => browserWindow.Show();
				browserWindow.LoadURL($"http://localhost:{BridgeSettings.WebPort}");
			});
		}
	}
}
