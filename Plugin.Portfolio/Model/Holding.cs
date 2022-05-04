using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Portfolio.Model
{
	public class Holding
	{
		public decimal Units { get; set; }
		public decimal Spent { get; set; }
		public string Currency { get; set; }
	}
}
