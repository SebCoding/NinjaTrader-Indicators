#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.SebIndicators
{
		/// <summary>
	/// The Average True Range (ATR) is a measure of volatility. It was introduced by Welles Wilder
	/// in his book 'New Concepts in Technical Trading Systems' and has since been used as a component
	/// of many indicators and trading systems.
	/// </summary>
	public class MyATR : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "ATR Customized by Seb";
				Name						= "MyATR";
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.DarkCyan, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameATR);
			}
		}

		protected override void OnBarUpdate()
		{
			double high0 = High[0];
			double low0	 = Low[0];
			
			if (CurrentBar == 0)
				Value[0] = (DisplayInTicks) ? (high0 - low0) / TickSize: high0 - low0;
			else
			{
				if(DisplayInTicks) 
				{
					Value[0] = ATR(Period)[0] / TickSize;
					//Value[0] = Instrument.MasterInstrument.RoundToTickSize(ATR(Period)[0]) / TickSize;
				}
				else
				{
					Value[0] = ATR(Period)[0];
				}
//				Print("ATR Built-in= "+ ATR(Period)[0]);
//				Print("ATR Custom = "+ Value[0]);
//				Print(" ");
			}
		}

		#region Properties
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Parameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Display in Ticks", Description="Display ATR value in ticks.", Order=1, GroupName="Parameters")]
		public bool DisplayInTicks
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SebIndicators.MyATR[] cacheMyATR;
		public SebIndicators.MyATR MyATR(int period, bool displayInTicks)
		{
			return MyATR(Input, period, displayInTicks);
		}

		public SebIndicators.MyATR MyATR(ISeries<double> input, int period, bool displayInTicks)
		{
			if (cacheMyATR != null)
				for (int idx = 0; idx < cacheMyATR.Length; idx++)
					if (cacheMyATR[idx] != null && cacheMyATR[idx].Period == period && cacheMyATR[idx].DisplayInTicks == displayInTicks && cacheMyATR[idx].EqualsInput(input))
						return cacheMyATR[idx];
			return CacheIndicator<SebIndicators.MyATR>(new SebIndicators.MyATR(){ Period = period, DisplayInTicks = displayInTicks }, input, ref cacheMyATR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.MyATR MyATR(int period, bool displayInTicks)
		{
			return indicator.MyATR(Input, period, displayInTicks);
		}

		public Indicators.SebIndicators.MyATR MyATR(ISeries<double> input , int period, bool displayInTicks)
		{
			return indicator.MyATR(input, period, displayInTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.MyATR MyATR(int period, bool displayInTicks)
		{
			return indicator.MyATR(Input, period, displayInTicks);
		}

		public Indicators.SebIndicators.MyATR MyATR(ISeries<double> input , int period, bool displayInTicks)
		{
			return indicator.MyATR(input, period, displayInTicks);
		}
	}
}

#endregion
