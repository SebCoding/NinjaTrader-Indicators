#region Using declarations
using System;
using System.Xml;
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
				
				//User defined parameters
				FontColor									= Brushes.DodgerBlue;
				MarketOpenTime                              = new TimeSpan(9, 15, 00);
				MarketCloseTime                             = new TimeSpan(17, 00, 00);
				TimeSpanInMin_Open							= 10;
				TimeSpanInMin_Closed						= 60;
				InvertOpeningHours 							= false;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{	
			DateTime currentBarDateTime = Bars.GetTime(CurrentBar);
			Print("CurrentBar #" + CurrentBar + " time stamp is " + currentBarDateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
			
			DateTime MarketOpen = new DateTime(currentBarDateTime.Year, currentBarDateTime.Month, currentBarDateTime.Day, 0, 0 ,0) + MarketOpenTime;
			DateTime MarketClose = new DateTime(currentBarDateTime.Year, currentBarDateTime.Month, currentBarDateTime.Day, 0, 0 ,0) + MarketCloseTime;
			Print("MarketOpenTime (Param): " + MarketOpenTime);
			Print("MarketOpen: " + MarketOpen.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
		
			bool MarketIsOpen;
			int TimeRangeInMinutes;
			
			// It's a week day and the market is open
			if ((currentBarDateTime >= MarketOpen) 
				&& (currentBarDateTime <= MarketClose)
				&& (currentBarDateTime.DayOfWeek != DayOfWeek.Saturday) 
				&& (currentBarDateTime.DayOfWeek != DayOfWeek.Sunday))
			{
				MarketIsOpen = true;
				TimeRangeInMinutes = TimeSpanInMin_Open;
				if(InvertOpeningHours)
					TimeRangeInMinutes = TimeSpanInMin_Closed;
			}
			else
			{
				MarketIsOpen = false;
				TimeRangeInMinutes = TimeSpanInMin_Closed;
				if(InvertOpeningHours)
					TimeRangeInMinutes = TimeSpanInMin_Open;
			}
			
			if (TimeRangeInMinutes <= 0)
				return;
			
			Print("TimeRangeInMinutes: " + TimeRangeInMinutes);
			
			// Why is this here? seems logically invalid. Whgy are we comparing bar index with minutes?
//			if(CurrentBar < TimeRangeInMinutes)
//				return;
			
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

			string message = "\nBAR SPEED [last " + TimeRangeInMinutes + "m]\n";
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
		[XmlIgnore]
		[Display(Name="FontColor", Description="Font Color", Order=1, GroupName="Parameters")]
		public Brush FontColor
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Market Open Time", Description="Market open time on local computer.", Order=2, GroupName="Parameters")]
		public TimeSpan MarketOpenTime
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Market Close Time", Description="Market close time on local computer.", Order=3, GroupName="Parameters")]
		public TimeSpan MarketCloseTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 1400)]
		[Display(Name="Time Span In Minutes (within opening hours)", Description="Time span (in minutes) used to calculate speed.", Order=4, GroupName="Parameters")]
		public int TimeSpanInMin_Open
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 1440)]
		[Display(Name="Time Span In Minutes (outside opening hours)", Description="Time span (in minutes) used to calculate speed.", Order=5, GroupName="Parameters")]
		public int TimeSpanInMin_Closed
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="InvertOpeningHours", Description="Invert Opening Hours", Order=6, GroupName="Parameters")]
		public bool InvertOpeningHours
		{ get; set; }
		
		
		// XmlSerializer does not support TimeSpan, so use this property for 
		// serialization instead.
		[Browsable(false)]
		[XmlElement(DataType="duration", ElementName="MarketOpenTime")]
		public string MarketOpenTimeString
		{
		    get 
		    { 
		        return XmlConvert.ToString(MarketOpenTime); 
		    }
		    set 
		    { 
		        MarketOpenTime = string.IsNullOrEmpty(value) ?
		            TimeSpan.Zero : XmlConvert.ToTimeSpan(value); 
		    }
		}
		
		// XmlSerializer does not support TimeSpan, so use this property for 
		// serialization instead.
		[Browsable(false)]
		[XmlElement(DataType="duration", ElementName="MarketCloseTime")]
		public string MarketCloseTimeString
		{
		    get 
		    { 
		        return XmlConvert.ToString(MarketCloseTime); 
		    }
		    set 
		    { 
		        MarketCloseTime = string.IsNullOrEmpty(value) ?
		            TimeSpan.Zero : XmlConvert.ToTimeSpan(value); 
		    }
		}

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
		public SebIndicators.BarSpeed BarSpeed(Brush fontColor, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return BarSpeed(Input, fontColor, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}

		public SebIndicators.BarSpeed BarSpeed(ISeries<double> input, Brush fontColor, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			if (cacheBarSpeed != null)
				for (int idx = 0; idx < cacheBarSpeed.Length; idx++)
					if (cacheBarSpeed[idx] != null && cacheBarSpeed[idx].FontColor == fontColor && cacheBarSpeed[idx].MarketOpenTime == marketOpenTime && cacheBarSpeed[idx].MarketCloseTime == marketCloseTime && cacheBarSpeed[idx].TimeSpanInMin_Open == timeSpanInMin_Open && cacheBarSpeed[idx].TimeSpanInMin_Closed == timeSpanInMin_Closed && cacheBarSpeed[idx].InvertOpeningHours == invertOpeningHours && cacheBarSpeed[idx].EqualsInput(input))
						return cacheBarSpeed[idx];
			return CacheIndicator<SebIndicators.BarSpeed>(new SebIndicators.BarSpeed(){ FontColor = fontColor, MarketOpenTime = marketOpenTime, MarketCloseTime = marketCloseTime, TimeSpanInMin_Open = timeSpanInMin_Open, TimeSpanInMin_Closed = timeSpanInMin_Closed, InvertOpeningHours = invertOpeningHours }, input, ref cacheBarSpeed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.BarSpeed BarSpeed(Brush fontColor, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeed(Input, fontColor, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}

		public Indicators.SebIndicators.BarSpeed BarSpeed(ISeries<double> input , Brush fontColor, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeed(input, fontColor, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.BarSpeed BarSpeed(Brush fontColor, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeed(Input, fontColor, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}

		public Indicators.SebIndicators.BarSpeed BarSpeed(ISeries<double> input , Brush fontColor, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeed(input, fontColor, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}
	}
}

#endregion
