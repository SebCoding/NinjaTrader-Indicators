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
using NinjaTrader.Gui.NinjaScript.Wizard;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.SebIndicators
{
	public class BarSpeedv1 : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Displays bar speed on tick charts";
				Name										= "BarSpeedv1";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				PaintPriceMarkers							= false;
                IsAutoScale									= false;
                ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

				//User defined parameters
				FontSize                                    = 12;
                FontColor									= Brushes.DodgerBlue;
				textPosition                                = TextPosition.TopRight;
				X_Offset                                    = 0;
				Y_Offset                                    = 0;
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
			//Print("CurrentBar #" + CurrentBar + " time stamp is " + currentBarDateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
			
			DateTime MarketOpen = new DateTime(currentBarDateTime.Year, currentBarDateTime.Month, currentBarDateTime.Day, 0, 0 ,0) + MarketOpenTime;
			DateTime MarketClose = new DateTime(currentBarDateTime.Year, currentBarDateTime.Month, currentBarDateTime.Day, 0, 0 ,0) + MarketCloseTime;
			//Print("MarketOpenTime (Param): " + MarketOpenTime);
			//Print("MarketOpen: " + MarketOpen.ToString("dddd, dd MMMM yyyy HH:mm:ss"));

			//Print("InvertOpeningHours: "+InvertOpeningHours);
		
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
			
			//Print("TimeRangeInMinutes: " + TimeRangeInMinutes);
			
			NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont( "Consolas", FontSize);
			//myFont.Bold = false;

			//Print($"CurrentBar: {CurrentBar}");
			
			double nbSecs = TimeRangeInMinutes * 60;
			double nbBars = (CurrentBar - Bars.GetBar(Time[0].AddMinutes(-TimeRangeInMinutes)));

            //Print($"nbBars: {nbBars}");

            //			string str1 = "From Time: " + Time[0].AddSeconds(-nbSecs);
            //			string str2 = ", nbBars: " + nbBars;
            //			Draw.TextFixed(this, "BarSpeedv1", str1+str2, TextPosition.TopRight, Brushes.DodgerBlue, myFont, null, null, 100);

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


			//Draw.TextFixed(this, "BarSpeedv1", "Time Now: " + Time[1] + "\nTime - 15M: "+ Time[1].AddSeconds(-nbSecs), TextPosition.TopRight, FontColor, myFont, null, null, 100);
			string message = "";
			string header = $"BAR SPEED [last {TimeRangeInMinutes}m]";
			int padding = header.Length + Math.Abs(X_Offset);

            // If we need to move the text down
            if (Y_Offset > 0)
			{
				for (int i = 0; i < Y_Offset; i++)
					message += "\n";
			}
			
			message += AddXTextOffset(header, padding) + "\n";
			if (barDuration > 0)
			{
				message += AddXTextOffset($"1 bar = {barDurationStr}", padding) + "\n";
				string plural = ((bpm >= 2) ? "s" : "");
                message += AddXTextOffset($"1 min = {bpm} bar{plural}", padding) + "\n";
			}
			else
			{
				message += AddXTextOffset("Interval does not contain enough bars", padding) + "\n";
			}

            // If we need to move the text up
            if (Y_Offset < 0)
            {
				int nb = Y_Offset * -1;
                for (int i = 0; i < nb; i++)
                    message += "\n";
            }

            Draw.TextFixed(this, "BarSpeedv1", message, textPosition, FontColor, myFont, null, null, 100);
		}

		public string AddXTextOffset (string txt, int padding)
		{
			string tmp = txt;
			if (X_Offset < 0)
			{
				tmp = tmp.PadRight(padding, ' ') + ".";
            }
			else if (X_Offset > 0)
				tmp = tmp.PadLeft(padding, ' ');
            return tmp;
		}

        #region Properties

        [NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "FontSize", Description = "Font Size", Order = 1, GroupName = "Parameters")]
        [Range(6, 36)]
        public int FontSize
        { get; set; }

        [NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="FontColor", Description="Font Color", Order=2, GroupName="Parameters")]
		public Brush FontColor
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Text Postion", Description ="Where to Display the Text on the Chart", Order =3, GroupName ="Parameters")]
		public TextPosition textPosition 
		{ get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "X Offset (+/- values to move left or right)", Description = "X Offset", Order = 4, GroupName = "Parameters")]
        [Range(-300, 300)]
        public int X_Offset
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Y Offset (+/- values to move up or down)", Description = "Y Offset", Order = 5, GroupName = "Parameters")]
        [Range(-80, 80)]
        public int Y_Offset
        { get; set; }

        [NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Market Open Time", Description="Market open time on local computer.", Order=6, GroupName="Parameters")]
		public TimeSpan MarketOpenTime
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Market Close Time", Description="Market close time on local computer.", Order=7, GroupName="Parameters")]
		public TimeSpan MarketCloseTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 1400)]
		[Display(Name="Time Span In Minutes (within opening hours)", Description="Time span (in minutes) used to calculate speed.", Order=8, GroupName="Parameters")]
		public int TimeSpanInMin_Open
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 1440)]
		[Display(Name="Time Span In Minutes (outside opening hours)", Description="Time span (in minutes) used to calculate speed.", Order=9, GroupName="Parameters")]
		public int TimeSpanInMin_Closed
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="InvertOpeningHours", Description="Invert Opening Hours", Order=10, GroupName="Parameters")]
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
		private SebIndicators.BarSpeedv1[] cacheBarSpeedv1;
		public SebIndicators.BarSpeedv1 BarSpeedv1(int fontSize, Brush fontColor, TextPosition textPosition, int x_Offset, int y_Offset, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return BarSpeedv1(Input, fontSize, fontColor, textPosition, x_Offset, y_Offset, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}

		public SebIndicators.BarSpeedv1 BarSpeedv1(ISeries<double> input, int fontSize, Brush fontColor, TextPosition textPosition, int x_Offset, int y_Offset, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			if (cacheBarSpeedv1 != null)
				for (int idx = 0; idx < cacheBarSpeedv1.Length; idx++)
					if (cacheBarSpeedv1[idx] != null && cacheBarSpeedv1[idx].FontSize == fontSize && cacheBarSpeedv1[idx].FontColor == fontColor && cacheBarSpeedv1[idx].textPosition == textPosition && cacheBarSpeedv1[idx].X_Offset == x_Offset && cacheBarSpeedv1[idx].Y_Offset == y_Offset && cacheBarSpeedv1[idx].MarketOpenTime == marketOpenTime && cacheBarSpeedv1[idx].MarketCloseTime == marketCloseTime && cacheBarSpeedv1[idx].TimeSpanInMin_Open == timeSpanInMin_Open && cacheBarSpeedv1[idx].TimeSpanInMin_Closed == timeSpanInMin_Closed && cacheBarSpeedv1[idx].InvertOpeningHours == invertOpeningHours && cacheBarSpeedv1[idx].EqualsInput(input))
						return cacheBarSpeedv1[idx];
			return CacheIndicator<SebIndicators.BarSpeedv1>(new SebIndicators.BarSpeedv1(){ FontSize = fontSize, FontColor = fontColor, textPosition = textPosition, X_Offset = x_Offset, Y_Offset = y_Offset, MarketOpenTime = marketOpenTime, MarketCloseTime = marketCloseTime, TimeSpanInMin_Open = timeSpanInMin_Open, TimeSpanInMin_Closed = timeSpanInMin_Closed, InvertOpeningHours = invertOpeningHours }, input, ref cacheBarSpeedv1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.BarSpeedv1 BarSpeedv1(int fontSize, Brush fontColor, TextPosition textPosition, int x_Offset, int y_Offset, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeedv1(Input, fontSize, fontColor, textPosition, x_Offset, y_Offset, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}

		public Indicators.SebIndicators.BarSpeedv1 BarSpeedv1(ISeries<double> input , int fontSize, Brush fontColor, TextPosition textPosition, int x_Offset, int y_Offset, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeedv1(input, fontSize, fontColor, textPosition, x_Offset, y_Offset, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.BarSpeedv1 BarSpeedv1(int fontSize, Brush fontColor, TextPosition textPosition, int x_Offset, int y_Offset, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeedv1(Input, fontSize, fontColor, textPosition, x_Offset, y_Offset, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}

		public Indicators.SebIndicators.BarSpeedv1 BarSpeedv1(ISeries<double> input , int fontSize, Brush fontColor, TextPosition textPosition, int x_Offset, int y_Offset, TimeSpan marketOpenTime, TimeSpan marketCloseTime, int timeSpanInMin_Open, int timeSpanInMin_Closed, bool invertOpeningHours)
		{
			return indicator.BarSpeedv1(input, fontSize, fontColor, textPosition, x_Offset, y_Offset, marketOpenTime, marketCloseTime, timeSpanInMin_Open, timeSpanInMin_Closed, invertOpeningHours);
		}
	}
}

#endregion
