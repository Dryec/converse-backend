using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Converse.Singleton;
using Converse.Singleton.WalletClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
			services.Configure<Configuration.Token>(Configuration.GetSection("Token"));
			services.Configure<Configuration.Block>(Configuration.GetSection("Block"));

			// Add Singletons
			services.AddSingleton<FCMClient>();
			services.AddSingleton<WalletClient>();

			// Add DatabaseContext with ConnectionString from the app settings.
			services.AddDbContextPool<Database.DatabaseContext>(options =>
				options
					.UseMySql(Configuration.GetConnectionString("MySql"), mySqlOptions => mySqlOptions
						.CharSetBehavior(CharSetBehavior.AppendToAllColumns)
						.UnicodeCharSet(CharSet.Utf8mb4)
						.ServerVersion(new Version(10, 1, 31), ServerType.MariaDb)
					)
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
				var fcmClient = app.ApplicationServices.GetService<FCMClient>();
				fcmClient.Initialize(Configuration.GetSection("Firebase"));
				
				var walletClient = app.ApplicationServices.GetService<WalletClient>();
				walletClient.Start();
			});

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
