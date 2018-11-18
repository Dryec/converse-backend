using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Service
{
	public class Test
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Test1Id { get; set; }
		public virtual Test1 Test1 { get; set; }
		public int Test2Id { get; set; }
		public virtual Test2 Test2 { get; set; }
	}

	public class Test1
	{
		public int Id { get; set; }
		public int Test2Id { get; set; }
		public virtual Test2 Test2 { get; set; }
		public string Name { get; set; }

		public virtual List<Test> Tests { get; set; }
	}

	public class Test2
	{
		public int Id { get; set; }
	}

	public class DatabaseContext : DbContext
	{
		public DbSet<Test> Tests { get; set; }
		public DbSet<Test1> Tests1 { get; set; }
		public DbSet<Test2> Tests2 { get; set; }

		public DatabaseContext(DbContextOptions contextOptions)
			: base(contextOptions)
		{
		}
	}
}
