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
	public class MyRisk : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MyRisk";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				TicksToAdd                                  = 2;
				
				//NbContracts = 1;
				AddPlot(Brushes.DarkRed, "Ticks");
				AddPlot(Brushes.DarkRed, "Points");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			double tickValue = TickSize * Instrument.MasterInstrument.PointValue;
			double barSizeInPoint = High[0]-Low[0];
			
			Ticks[0] = Convert.ToInt32(barSizeInPoint/ TickSize) + TicksToAdd; 
			Points[0] = Instrument.MasterInstrument.RoundToTickSize((High[0]+TickSize)- (Low[0]-TickSize));
		}

		#region Properties
		
//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="NbContracts", Description="Number of contracts traded", Order=1, GroupName="Parameters")]
//		public int NbContracts
//		{ get; set; }
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "TicksToAdd", Description="Ticks to Add to the Size of the Signal Bar", GroupName = "Parameters", Order = 0)]
		public int TicksToAdd
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Ticks
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Points
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SebIndicators.MyRisk[] cacheMyRisk;
		public SebIndicators.MyRisk MyRisk(int ticksToAdd)
		{
			return MyRisk(Input, ticksToAdd);
		}

		public SebIndicators.MyRisk MyRisk(ISeries<double> input, int ticksToAdd)
		{
			if (cacheMyRisk != null)
				for (int idx = 0; idx < cacheMyRisk.Length; idx++)
					if (cacheMyRisk[idx] != null && cacheMyRisk[idx].TicksToAdd == ticksToAdd && cacheMyRisk[idx].EqualsInput(input))
						return cacheMyRisk[idx];
			return CacheIndicator<SebIndicators.MyRisk>(new SebIndicators.MyRisk(){ TicksToAdd = ticksToAdd }, input, ref cacheMyRisk);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.MyRisk MyRisk(int ticksToAdd)
		{
			return indicator.MyRisk(Input, ticksToAdd);
		}

		public Indicators.SebIndicators.MyRisk MyRisk(ISeries<double> input , int ticksToAdd)
		{
			return indicator.MyRisk(input, ticksToAdd);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.MyRisk MyRisk(int ticksToAdd)
		{
			return indicator.MyRisk(Input, ticksToAdd);
		}

		public Indicators.SebIndicators.MyRisk MyRisk(ISeries<double> input , int ticksToAdd)
		{
			return indicator.MyRisk(input, ticksToAdd);
		}
	}
}

#endregion
