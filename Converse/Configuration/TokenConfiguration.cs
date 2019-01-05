using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Configuration
{
	public class Token
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int TransferMaxPerAccountEveryDay { get; set; }
		public int TransferOnlyWhenHasLessOrEqualThan { get; set; }
		public int TransferSteps { get; set; }
	}
}
