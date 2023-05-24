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
	public class BarSpeed : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Displays bar speed on tick charts";
				Name										= "BarSpeed";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				TimeRangeInMinutes					= 10;
				FontColor					= Brushes.DodgerBlue;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if((CurrentBar < TimeRangeInMinutes) || (TimeRangeInMinutes <= 0))
				return;
			
			NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Consolas", 16) { Size = 12, Bold = false };
			
			double nbSecs = TimeRangeInMinutes * 60;
			double nbBars = (CurrentBar - Bars.GetBar(Time[0].AddMinutes(-TimeRangeInMinutes))); 
			
//			string str1 = "From Time: " + Time[0].AddSeconds(-nbSecs);
//			string str2 = ", nbBars: " + nbBars;
//			Draw.TextFixed(this, "BarSpeed1", str1+str2, TextPosition.TopRight, Brushes.DodgerBlue, myFont, null, null, 100);

			// Bars Per Minute (BPM)
			double bpm = Math.Round(nbBars / (double)TimeRangeInMinutes, 2);
			double barDuration = (nbBars > 0) ? Math.Round(nbSecs / nbBars, 1): 0;
			
			string barDurationStr = "";
			if(barDuration >= 60) 
			{
				int remaingSecs = ((int)barDuration%60);
				barDurationStr = Math.Floor(barDuration/60) + "m " + ((remaingSecs >= 1) ? remaingSecs+"s": "");
			}
			else
			{
				barDurationStr = barDuration + "s";
			}


			//Draw.TextFixed(this, "BarSpeed0", "Time Now: " + Time[1] + "\nTime - 15M: "+ Time[1].AddSeconds(-nbSecs), TextPosition.TopRight, FontColor, myFont, null, null, 100);

			string message = "\nBAR SPEED [" + TimeRangeInMinutes + "m]\n";
			if(barDuration > 0)
			{
				message += "1 bar = " + barDurationStr + "\n";
				message += "1 min = "+ bpm + " bar" + ((bpm >= 2) ? "s": "");
			}
			else
			{
				message += "Interval does not contain enough bars";
			}
			Draw.TextFixed(this, "BarSpeed", message, TextPosition.TopRight, FontColor, myFont, null, null, 100);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TimeRangeInMinutes", Description="Time range (in minutes) used to calculate speed.", Order=1, GroupName="Parameters")]
		public int TimeRangeInMinutes
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="FontColor", Description="Font Color", Order=1, GroupName="Parameters")]
		public Brush FontColor
		{ get; set; }

		[Browsable(false)]
		public string FontColorSerializable
		{
			get { return Serialize.BrushToString(FontColor); }
			set { FontColor = Serialize.StringToBrush(value); }
		}	
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SebIndicators.BarSpeed[] cacheBarSpeed;
		public SebIndicators.BarSpeed BarSpeed(int timeRangeInMinutes, Brush fontColor)
		{
			return BarSpeed(Input, timeRangeInMinutes, fontColor);
		}

		public SebIndicators.BarSpeed BarSpeed(ISeries<double> input, int timeRangeInMinutes, Brush fontColor)
		{
			if (cacheBarSpeed != null)
				for (int idx = 0; idx < cacheBarSpeed.Length; idx++)
					if (cacheBarSpeed[idx] != null && cacheBarSpeed[idx].TimeRangeInMinutes == timeRangeInMinutes && cacheBarSpeed[idx].FontColor == fontColor && cacheBarSpeed[idx].EqualsInput(input))
						return cacheBarSpeed[idx];
			return CacheIndicator<SebIndicators.BarSpeed>(new SebIndicators.BarSpeed(){ TimeRangeInMinutes = timeRangeInMinutes, FontColor = fontColor }, input, ref cacheBarSpeed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.BarSpeed BarSpeed(int timeRangeInMinutes, Brush fontColor)
		{
			return indicator.BarSpeed(Input, timeRangeInMinutes, fontColor);
		}

		public Indicators.SebIndicators.BarSpeed BarSpeed(ISeries<double> input , int timeRangeInMinutes, Brush fontColor)
		{
			return indicator.BarSpeed(input, timeRangeInMinutes, fontColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.BarSpeed BarSpeed(int timeRangeInMinutes, Brush fontColor)
		{
			return indicator.BarSpeed(Input, timeRangeInMinutes, fontColor);
		}

		public Indicators.SebIndicators.BarSpeed BarSpeed(ISeries<double> input , int timeRangeInMinutes, Brush fontColor)
		{
			return indicator.BarSpeed(input, timeRangeInMinutes, fontColor);
		}
	}
}

#endregion
