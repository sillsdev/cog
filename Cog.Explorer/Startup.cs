using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using EmbeddedBlazorContent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
			services.AddServerSideBlazor();
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
				BrowserWindow browserWindow = await Electron.WindowManager.CreateWindowAsync(
					new BrowserWindowOptions { Show = false, Frame = false }, "");
				browserWindow.OnReadyToShow += () => browserWindow.Show();
				browserWindow.LoadURL($"http://localhost:{BridgeSettings.WebPort}");
			});
		}
	}
}
