using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Configuration
{
	public class Block
	{
		public ulong StartId { get; set; }
		public int SyncSleepTime { get; set; }
		public int SyncCount { get; set; }
	}
}
