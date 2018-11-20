using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Converse
{
	public class Startup
	{
		private IConfiguration Configuration { get; }
		private Service.WalletClient WalletClient { get; set; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc()
				// For serializing. (E.g.: Blog has Posts and Post has Blog. Recursive Loop Exception)
				.AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			// Add Configurations
			services.Configure<Configuration.Node>(Configuration.GetSection("Node"));

			// Add Singletons
			services.AddSingleton<WalletClient>();

			// Add DatabaseContext with ConnectionString from the app settings.
			services.AddDbContextPool<Service.DatabaseContext>(options =>
				options
					//.UseLazyLoadingProxies()	// For resolving relationships automatically (Slower than using eager loading)
					.UseMySql(Configuration.GetConnectionString("MySql"), mySqlOptions =>
						mySqlOptions.ServerVersion(new Version(10, 1, 31), ServerType.MariaDb))
			);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			appLifetime.ApplicationStarted.Register(() =>
			{
				WalletClient = app.ApplicationServices.GetService<WalletClient>();
			});

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
