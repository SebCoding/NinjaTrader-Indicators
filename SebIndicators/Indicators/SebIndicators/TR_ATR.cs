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
	public class TR_ATR : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Custom True Range (TR) and Average True Range (ATR) Indicator in ticks or in points.";
				Name										= "TR_ATR";

                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive					= true;
				
				Period = 14;

                AddPlot(Brushes.Pink, "BarSize");
                AddPlot(Brushes.DarkCyan, "ATR");
            }
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			double high0 = High[0];
			double low0	 = Low[0];

			if (CurrentBar == 0)
			{
				Values[0][0] = (DisplayInTicks) ? (high0 - low0) / TickSize : high0 - low0;
				Values[1][0] = (DisplayInTicks) ? (high0 - low0) / TickSize : high0 - low0;
            }
			else
			{
				if (DisplayInTicks)
				{
					Values[0][0] = ATR(1)[0] / TickSize; // ATR(1) = TR
					Values[1][0] = ATR(Period)[0] / TickSize;
				}
				else
				{
					Values[0][0] = ATR(1)[0]; // ATR(1) = TR
					Values[1][0] = ATR(Period)[0];
				}
				//				Print("ATR Built-in= "+ ATR(Period)[0]);
				//				Print("ATR Custom = "+ Value[0]);
				//				Print(" ");
			}
		}
		
		#region Properties
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR Period", GroupName = "Parameters", Order = 0)]
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
		private SebIndicators.TR_ATR[] cacheTR_ATR;
		public SebIndicators.TR_ATR TR_ATR(int period, bool displayInTicks)
		{
			return TR_ATR(Input, period, displayInTicks);
		}

		public SebIndicators.TR_ATR TR_ATR(ISeries<double> input, int period, bool displayInTicks)
		{
			if (cacheTR_ATR != null)
				for (int idx = 0; idx < cacheTR_ATR.Length; idx++)
					if (cacheTR_ATR[idx] != null && cacheTR_ATR[idx].Period == period && cacheTR_ATR[idx].DisplayInTicks == displayInTicks && cacheTR_ATR[idx].EqualsInput(input))
						return cacheTR_ATR[idx];
			return CacheIndicator<SebIndicators.TR_ATR>(new SebIndicators.TR_ATR(){ Period = period, DisplayInTicks = displayInTicks }, input, ref cacheTR_ATR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.TR_ATR TR_ATR(int period, bool displayInTicks)
		{
			return indicator.TR_ATR(Input, period, displayInTicks);
		}

		public Indicators.SebIndicators.TR_ATR TR_ATR(ISeries<double> input , int period, bool displayInTicks)
		{
			return indicator.TR_ATR(input, period, displayInTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.TR_ATR TR_ATR(int period, bool displayInTicks)
		{
			return indicator.TR_ATR(Input, period, displayInTicks);
		}

		public Indicators.SebIndicators.TR_ATR TR_ATR(ISeries<double> input , int period, bool displayInTicks)
		{
			return indicator.TR_ATR(input, period, displayInTicks);
		}
	}
}

#endregion
