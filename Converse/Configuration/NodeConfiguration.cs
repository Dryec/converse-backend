using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Configuration
{
	public class Node
	{
		public string Ip { get; set; }
		public ushort Port { get; set; }
		public ulong StartBlockId { get; set; }
		public int BlockSyncTime { get; set; }
	}
}
