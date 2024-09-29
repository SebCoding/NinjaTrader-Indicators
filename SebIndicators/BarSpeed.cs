
#region Using declarations
using Gat;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.SebIndicators
{
    [Gui.CategoryOrder("Time", 1)]
    [Gui.CategoryOrder("Calculation (within opening hours)", 2)]
    [Gui.CategoryOrder("Calculation (outside opening hours)", 3)]
    [Gui.CategoryOrder("Text Output", 4)]
    public class BarSpeed : Indicator
	{
		private CalculationType calculationTypeDuringOpen;
        private CalculationType calculationTypeDuringClose;
        private TextLocation textLocation;
		private System.Windows.Media.Brush	textBrush;
		private string FontName = "Consolas";
	
		private string _notEnoughBars = "Not enough bars.";

		private string outputText;

        //public override string DisplayName
        //{
        //    get { return $"{this.GetType().Name}: xxxxxx)"; }
        //}


        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Display BarSpeed on Chart";
				Name						= "BarSpeed";
				Calculate					= Calculate.OnBarClose;
				DisplayInDataBox			= false;
				IsOverlay					= true;
				IsChartOnly					= true;
				IsSuspendedWhileInactive	= true;
				ScaleJustification			= ScaleJustification.Right;

                // Parameters
                TextLocation = TextLocation.TopRight;
				WMBrush = System.Windows.Media.Brushes.Gold;
				WMSize = 12;
				WMOpacity = 100;
				X_Offset = -45;
				Y_Offset = 10;

				CalculationTypeDuringOpen = CalculationType.TimeSpan;
                CalculationTypeDuringOpen = CalculationType.NumberOfBars;
                MarketOpenTime = new TimeSpan(9, 00, 00);
                MarketCloseTime = new TimeSpan(17, 00, 00);
                SpanDuringOpen = 10;
                SpanDuringClose = 5;
                InvertOpeningHours = false;
            }
			else if (State == State.Historical)
			{
				SetZOrder(-1); // default here is go below the bars and called in State.Historical
			}
			else if (State == State.Configure)
			{
			
			}
		}

        private bool isMarketOpen()
        {
            bool marketIsOpen;
            DateTime currentBarDateTime = Bars.GetTime(CurrentBar);
            //Print($"CurrentBar: {CurrentBar}, {currentBarDateTime.ToString()}");

            DateTime MarketOpen = new DateTime(currentBarDateTime.Year, currentBarDateTime.Month, currentBarDateTime.Day, 0, 0, 0) + MarketOpenTime;
            DateTime MarketClose = new DateTime(currentBarDateTime.Year, currentBarDateTime.Month, currentBarDateTime.Day, 0, 0, 0) + MarketCloseTime;
            //Print("MarketOpenTime (Param): " + MarketOpenTime);
            //Print("MarketOpen: " + MarketOpen.ToString("dddd, dd MMMM yyyy HH:mm:ss"));

            //Print("InvertOpeningHours: "+InvertOpeningHours);

            // It's a week day and the market is open
            if ((currentBarDateTime >= MarketOpen)
                && (currentBarDateTime <= MarketClose)
                && (currentBarDateTime.DayOfWeek != DayOfWeek.Saturday)
                && (currentBarDateTime.DayOfWeek != DayOfWeek.Sunday))
            {
                marketIsOpen = true;
            }
            else
            {
                marketIsOpen = false;
            }

            if (InvertOpeningHours)
                return !marketIsOpen;

            return marketIsOpen;
        }

        protected void calculateBasedOnTimeSpan(int TimeRangeInMinutes)
		{
            if (TimeRangeInMinutes <= 0)
                return;

            //Print("TimeRangeInMinutes: " + TimeRangeInMinutes);

            //Print($"CurrentBar: {CurrentBar}");

            // The barIndexToCalculate is different OnBarClose because the first bar that is not closed yet is part of the Count, but not in the series
            int barIndexToCalculate = Count - 1;
            if (Calculate == Calculate.OnBarClose)
            {
                barIndexToCalculate = barIndexToCalculate - 1;
            }

            // We only need to calculate once we are on the last bar closed
            // There is no need to calculate uselessly on every past bars of the chart
            if (CurrentBar < barIndexToCalculate)
                return;


            double nbSecs = TimeRangeInMinutes * 60;
            int nbBars = (CurrentBar - Bars.GetBar(Time[0].AddSeconds(-nbSecs)));

            //Print($"\nCurrentBar: {CurrentBar} [{currentBarDateTime.ToString()}], barIndexToCalculate: {barIndexToCalculate}, Count: {Count}");


            //Print("TimeRangeInMinutes: " + TimeRangeInMinutes);

            //Print($"CurrentBar: {CurrentBar}, {currentBarDateTime.ToString()}, NbBarSpan: {nbBarSpan}");

            //Print($"Time[0]: {Time[0].ToString()}, nbBars: {nbBars}");
            //Print($"Time[{nbBars - 1}]: {Time[nbBars - 1].ToString()}");


            //Print($"nbBars: {nbBars}");

            // Bars Per Minute (BPM)
            double bpm = Math.Round(nbBars / (double)TimeRangeInMinutes, 2);
            double barDuration = (nbBars > 0) ? Math.Round(nbSecs / nbBars, 1) : 0;

            string barDurationStr = "";
            if (barDuration >= 60)
            {
                int remaingSecs = ((int)barDuration % 60);
                barDurationStr = Math.Floor(barDuration / 60) + "m " + ((remaingSecs >= 1) ? remaingSecs + "s" : "");
            }
            else
            {
                barDurationStr = barDuration + "s";
            }

            outputText = $"BAR SPEED [last {TimeRangeInMinutes}m]\n";
            if (barDuration > 0)
            {
                outputText += $"1 bar = {barDurationStr}\n";
                string plural = ((bpm >= 2) ? "s" : "");
                outputText += $"1 min = {bpm} bar{plural}\n";
            }
            else
            {
                outputText += _notEnoughBars;
            }
        }

        protected void calculateBasedOnBarSpan(int nbBarSpan)
        {
            if (nbBarSpan > Count)
            {
                outputText = _notEnoughBars;
                return;
            }

            // The barIndexToCalculate is different OnBarClose because the first bar that is not closed yet is part of the Count, but not in the series
            int barIndexToCalculate = Count - 1;
            if (Calculate == Calculate.OnBarClose)
            {
                barIndexToCalculate = barIndexToCalculate - 1;
            }

            // We only need to calculate once we are on the last bar closed
            // There is no need to calculate uselessly on every past bars of the chart
            if (CurrentBar < barIndexToCalculate)
                return;

            //Print($"\nCurrentBar: {CurrentBar} [{currentBarDateTime.ToString()}], nbBarSpan: {nbBarSpan}, barIndexToCalculate: {barIndexToCalculate}, Count: {Count}");


            //Print("TimeRangeInMinutes: " + TimeRangeInMinutes);

            //Print($"CurrentBar: {CurrentBar}, {currentBarDateTime.ToString()}, NbBarSpan: {nbBarSpan}");

            //Print($"Time[0]: {Time[0].ToString()}, nbBarSpan: {nbBarSpan}");
            //Print($"Time[{nbBarSpan-1}]: {Time[nbBarSpan-1].ToString()}");

            double nbSecs = Time[0].Subtract(Time[nbBarSpan-1]).TotalSeconds;           

            // Bars Per Minute (BPM)
            double bpm = Math.Round(nbBarSpan / (nbSecs*60), 2);
            double barDuration = (nbBarSpan > 0) ? Math.Round(nbSecs / nbBarSpan, 1) : 0;

            //Print($"nbSecs: {nbSecs}, barDuration: {barDuration}s");

            string barDurationStr = "";
            //if (barDuration >= 3600)
            //{
            //    int remainingSecs = ((int)barDuration % 3600);
            //    int remaingMins = ((int)barDuration % 60);
            //    barDurationStr = Math.Floor(barDuration / 3600) + "h ";
            //    barDurationStr += ((remaingMins >= 1) ? remaingMins + "s" : "");
            //    barDurationStr += ((remainingSecs >= 1) ? remainingSecs + "s" : "");
            //}
            //else 
            if (barDuration >= 60)
            {
                int remaingSecs = ((int)barDuration % 60);
                barDurationStr = Math.Floor(barDuration / 60) + "m " + ((remaingSecs >= 1) ? remaingSecs + "s" : "");
            }
            else
            {
                barDurationStr = barDuration + "s";
            }

            string plural = (nbBarSpan > 1) ? "s" : "";
            outputText = $"BAR SPEED [last {nbBarSpan} bar{plural}]\n";
            if (barDuration > 0)
            {
                outputText += $"1 bar = {barDurationStr}\n";
                //plural = ((bpm >= 2) ? "s" : "");
                //outputText += $"1 min = {bpm} bar{plural}\n";
            }
            else
            {
                outputText += _notEnoughBars;
            }
        }

        protected override void OnBarUpdate()
        {
            if (Calculate != Calculate.OnBarClose)
            {
                outputText = $"BAR SPEED\nCalculation currently set to {Calculate}\nPlease set calculation to OnBarClose";
                return;
            }

            if (isMarketOpen())
            {
                switch (calculationTypeDuringOpen)
                {
                    case CalculationType.TimeSpan:
                        calculateBasedOnTimeSpan(SpanDuringOpen);
                        break;
                    case CalculationType.NumberOfBars:
                        calculateBasedOnBarSpan(SpanDuringOpen);
                        break;
                }
            }
            else
            {
                switch (calculationTypeDuringClose)
                {
                    case CalculationType.TimeSpan:
                        calculateBasedOnTimeSpan(SpanDuringClose);
                        break;
                    case CalculationType.NumberOfBars:
                        calculateBasedOnBarSpan(SpanDuringClose);
                        break;
                }
            }
        }

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (!IsInHitTest)
			{              
                // Notes:  RenderTarget is always the full ChartPanel, so we need to be mindful which sub-ChartPanel we're dealing with
                // Always use ChartPanel X, Y, W, H - as chartScale and chartControl properties WPF units, so they can be drastically different depending on DPI set
                SharpDX.Vector2 vTopLeft = new SharpDX.Vector2(ChartPanel.X, ChartPanel.Y);
				SharpDX.Vector2 vBottomRight = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);

				SharpDX.Vector2 vBottomLeft = new SharpDX.Vector2(ChartPanel.X, ChartPanel.Y + ChartPanel.H);
				SharpDX.Vector2 vTopRight = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, ChartPanel.Y);

				SharpDX.Vector2 vCenter = (vTopLeft + vBottomRight) / 2;

				SharpDX.Direct2D1.Brush textBrushDx;
				textBrushDx = textBrush.ToDxBrush(RenderTarget);
				textBrushDx.Opacity = (float)WMOpacity/100;

				SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;

				RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;

                //SharpDX.DirectWrite.TextFormat textFormat =
                //	new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, ChartControl.Properties.LabelFont.FamilySerialize, WMSize);
                SharpDX.DirectWrite.TextFormat textFormat =
					new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, FontName, WMSize);
                SharpDX.DirectWrite.TextLayout textLayout =
					new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, outputText, textFormat, ChartPanel.W, ChartPanel.H);

				SharpDX.Vector2 TextPoint;

				switch (textLocation)
				{
					case TextLocation.Center:
						{
							TextPoint = new SharpDX.Vector2(vCenter.X - (textLayout.Metrics.Width / 2) + X_Offset, vCenter.Y - (textLayout.Metrics.Height / 2) + Y_Offset); 
							break;
						}
					case TextLocation.TopLeft:
						{
							TextPoint = new SharpDX.Vector2(vTopLeft.X + X_Offset, vTopLeft.Y + Y_Offset); 
							break;
						}
					case TextLocation.TopCenter:
						{
							TextPoint = new SharpDX.Vector2(vCenter.X - (textLayout.Metrics.Width / 2) + X_Offset, vTopLeft.Y + Y_Offset); 
							break;
						}
					case TextLocation.TopRight:
						{
							TextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout.Metrics.Width + X_Offset, vTopLeft.Y + Y_Offset); 
							break;
						}
					case TextLocation.BottomLeft:
						{
							TextPoint = new SharpDX.Vector2(vBottomLeft.X + X_Offset, vBottomLeft.Y - textLayout.Metrics.Height + Y_Offset); 
							break;
						}
					case TextLocation.BottomCenter:
						{
							TextPoint = new SharpDX.Vector2(vCenter.X - (textLayout.Metrics.Width / 2) + X_Offset, vBottomLeft.Y - textLayout.Metrics.Height + Y_Offset); 
							break;
						}
					case TextLocation.BottomRight:
						{
							TextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout.Metrics.Width + X_Offset, vBottomLeft.Y - textLayout.Metrics.Height + Y_Offset); 
							break;
						}
					default:
						{
							TextPoint = new SharpDX.Vector2(vCenter.X - (textLayout.Metrics.Width / 2), vCenter.Y - (textLayout.Metrics.Height / 2));
							break;
						}
				}

				RenderTarget.DrawTextLayout(TextPoint, textLayout, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

				RenderTarget.AntialiasMode = oldAntialiasMode;

				textBrushDx.Dispose();
				textFormat.Dispose();
				textLayout.Dispose();			
			}
		}

        #region Properties

        // Time Properties
        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Market Open Time", Description = "Market open time on local computer.", Order = 1, GroupName = "Time")]
        public TimeSpan MarketOpenTime
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Market Close Time", Description = "Market close time on local computer.", Order = 2, GroupName = "Time")]
        public TimeSpan MarketCloseTime
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "InvertOpeningHours", Description = "Invert Opening Hours", Order = 3, GroupName = "Time")]
        public bool InvertOpeningHours
        { get; set; }

        // Calculation Related Properties

        [Display(Name = "Calculation Type", Description = "Calculation based on time span or number of past bars", Order = 1, GroupName = "Calculation (within opening hours)")]
        public CalculationType CalculationTypeDuringOpen
        {
            get { return calculationTypeDuringOpen; }
            set { calculationTypeDuringOpen = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Span in Min or NbBars", Description = "Span interval (in minutes or bars) used to calculate speed.", Order = 2, GroupName = "Calculation (within opening hours)")]
        public int SpanDuringOpen
        { get; set; }

        [Display(Name = "Calculation Type", Description = "Calculation based on time span or number of past bars", Order = 1, GroupName = "Calculation (outside opening hours)")]
        public CalculationType CalculationTypeDuringClose
        {
            get { return calculationTypeDuringClose; }
            set { calculationTypeDuringClose = value; }
        }
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Span in Min or NbBars", Description = "Span interval (in minutes or bars) used to calculate speed.", Order = 2, GroupName = "Calculation (outside opening hours)")]
        public int SpanDuringClose
        { get; set; }

        // Text Ouput Related Properties

        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Text Size", Order = 1, GroupName = "Text Output")]
		public int WMSize
		{ get; set; }

		[Display(Name = "Text Location", Description = "Select location", Order = 2, GroupName = "Text Output")]
		public TextLocation TextLocation
        {
			get { return textLocation; }
			set { textLocation = value; }
		}

		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name = "Text Opacity", Description = "Enter Opacity % from 1 to 100", Order = 3, GroupName = "Text Output")]
		public int WMOpacity
		{ get; set; }

		[XmlIgnore]
		[Display(Name = "Text Color", Description = "BarSpeed Color", Order = 4, GroupName = "Text Output")]
		public System.Windows.Media.Brush WMBrush
		{
			get { return textBrush; }
			set { textBrush = value; }
		}

        [NinjaScriptProperty]
        [Display(Name = "X Offset (+/- values to move left or right)", Description = "X Offset", Order = 5, GroupName = "Text Output")]
        [Range(-300, 300)]
        public int X_Offset
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Y Offset (+/- values to move up or down)", Description = "Y Offset", Order = 6, GroupName = "Text Output")]
        [Range(-80, 80)]
        public int Y_Offset
        { get; set; }

		
		#region Serialization 
		
		[Browsable(false)]
		public string TextBrushSerialize
		{
			get { return Serialize.BrushToString(WMBrush); }
			set { WMBrush = Serialize.StringToBrush(value); }
		}

        // XmlSerializer does not support TimeSpan, so use this property for 
        // serialization instead.
        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "MarketOpenTime")]
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
        [XmlElement(DataType = "duration", ElementName = "MarketCloseTime")]
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
		#endregion

        #endregion
    }

	#region enums
    public enum TextLocation
    {
        Center,
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

	public enum CalculationType 
	{
		TimeSpan,
		NumberOfBars
	}
	#endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SebIndicators.BarSpeed[] cacheBarSpeed;
		public SebIndicators.BarSpeed BarSpeed(TimeSpan marketOpenTime, TimeSpan marketCloseTime, bool invertOpeningHours, int spanDuringOpen, int spanDuringClose, int wMSize, int wMOpacity, int x_Offset, int y_Offset)
		{
			return BarSpeed(Input, marketOpenTime, marketCloseTime, invertOpeningHours, spanDuringOpen, spanDuringClose, wMSize, wMOpacity, x_Offset, y_Offset);
		}

		public SebIndicators.BarSpeed BarSpeed(ISeries<double> input, TimeSpan marketOpenTime, TimeSpan marketCloseTime, bool invertOpeningHours, int spanDuringOpen, int spanDuringClose, int wMSize, int wMOpacity, int x_Offset, int y_Offset)
		{
			if (cacheBarSpeed != null)
				for (int idx = 0; idx < cacheBarSpeed.Length; idx++)
					if (cacheBarSpeed[idx] != null && cacheBarSpeed[idx].MarketOpenTime == marketOpenTime && cacheBarSpeed[idx].MarketCloseTime == marketCloseTime && cacheBarSpeed[idx].InvertOpeningHours == invertOpeningHours && cacheBarSpeed[idx].SpanDuringOpen == spanDuringOpen && cacheBarSpeed[idx].SpanDuringClose == spanDuringClose && cacheBarSpeed[idx].WMSize == wMSize && cacheBarSpeed[idx].WMOpacity == wMOpacity && cacheBarSpeed[idx].X_Offset == x_Offset && cacheBarSpeed[idx].Y_Offset == y_Offset && cacheBarSpeed[idx].EqualsInput(input))
						return cacheBarSpeed[idx];
			return CacheIndicator<SebIndicators.BarSpeed>(new SebIndicators.BarSpeed(){ MarketOpenTime = marketOpenTime, MarketCloseTime = marketCloseTime, InvertOpeningHours = invertOpeningHours, SpanDuringOpen = spanDuringOpen, SpanDuringClose = spanDuringClose, WMSize = wMSize, WMOpacity = wMOpacity, X_Offset = x_Offset, Y_Offset = y_Offset }, input, ref cacheBarSpeed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.BarSpeed BarSpeed(TimeSpan marketOpenTime, TimeSpan marketCloseTime, bool invertOpeningHours, int spanDuringOpen, int spanDuringClose, int wMSize, int wMOpacity, int x_Offset, int y_Offset)
		{
			return indicator.BarSpeed(Input, marketOpenTime, marketCloseTime, invertOpeningHours, spanDuringOpen, spanDuringClose, wMSize, wMOpacity, x_Offset, y_Offset);
		}

		public Indicators.SebIndicators.BarSpeed BarSpeed(ISeries<double> input , TimeSpan marketOpenTime, TimeSpan marketCloseTime, bool invertOpeningHours, int spanDuringOpen, int spanDuringClose, int wMSize, int wMOpacity, int x_Offset, int y_Offset)
		{
			return indicator.BarSpeed(input, marketOpenTime, marketCloseTime, invertOpeningHours, spanDuringOpen, spanDuringClose, wMSize, wMOpacity, x_Offset, y_Offset);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.BarSpeed BarSpeed(TimeSpan marketOpenTime, TimeSpan marketCloseTime, bool invertOpeningHours, int spanDuringOpen, int spanDuringClose, int wMSize, int wMOpacity, int x_Offset, int y_Offset)
		{
			return indicator.BarSpeed(Input, marketOpenTime, marketCloseTime, invertOpeningHours, spanDuringOpen, spanDuringClose, wMSize, wMOpacity, x_Offset, y_Offset);
		}

		public Indicators.SebIndicators.BarSpeed BarSpeed(ISeries<double> input , TimeSpan marketOpenTime, TimeSpan marketCloseTime, bool invertOpeningHours, int spanDuringOpen, int spanDuringClose, int wMSize, int wMOpacity, int x_Offset, int y_Offset)
		{
			return indicator.BarSpeed(input, marketOpenTime, marketCloseTime, invertOpeningHours, spanDuringOpen, spanDuringClose, wMSize, wMOpacity, x_Offset, y_Offset);
		}
	}
}

#endregion
