﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Portfolio.Model
{
	public class Gain
	{
		public string Symbol { get; set; }
		public DateTime Date { get; set; }
		public decimal Value { get; set; }
		public string Currency { get; set; }
		public bool IsOpen { get; set; }
	}
}
