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
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.SebIndicators
{
    public class MyBarSize : Indicator
    {
        protected override void OnStateChange()
        {
            SolidColorBrush avgColor;

            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "MyBarSize";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                MaximumBarsLookBack = MaximumBarsLookBack.Infinite;

                //Parameters
                IncludeAverages = false;
                AveragePeriod = 21;

                AddPlot(Brushes.Orange, "Ticks");
                AddPlot(Brushes.Orange, "Points");

                avgColor = IncludeAverages ? Brushes.Orange : Brushes.Transparent;
                AddPlot(new Stroke(avgColor, 1), PlotStyle.Line, "AvgTicks");
                AddPlot(new Stroke(avgColor, 1), PlotStyle.Line, "AvgPoints");

            }
            else if (State == State.Configure)
            {
                avgColor = IncludeAverages ? Brushes.Orange : Brushes.Transparent;
                Plots[2].Brush = avgColor;
                Plots[3].Brush = avgColor;
            }
        }

        protected override void OnBarUpdate()
        {
            Ticks[0] = Convert.ToInt32(Instrument.MasterInstrument.RoundToTickSize(High[0] - Low[0]) / TickSize);
            Points[0] = Instrument.MasterInstrument.RoundToTickSize(High[0] - Low[0]);

            if (IncludeAverages)
            {
                // Do not calculate averages if we don't have enough bars
                if (CurrentBar < AveragePeriod)
                    return;

                // Calculate average size for the last AveragePeriod bars
                //Print("CurrentBar: " + CurrentBar);
                double sum = 0;
                double sumTicks = 0;
                for (int barsAgo = 0; barsAgo < AveragePeriod; barsAgo++)
                {
                    sum = sum + Points[barsAgo];
                    sumTicks = sumTicks + Ticks[barsAgo];
                }

                double avgPoints = sum / AveragePeriod;
                double avgTicks = sumTicks / AveragePeriod;
                AvgTicks[0] = Math.Round(avgTicks, 1);
                AvgPoints[0] = Math.Round(avgPoints, 1);
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "AveragePeriod", Description = "Period used to calculate average", Order = 1, GroupName = "Parameters")]
        public int AveragePeriod
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "IncludeAverages", Order = 1, GroupName = "Parameters")]
        public bool IncludeAverages
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

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> AvgTicks
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> AvgPoints
        {
            get { return Values[3]; }
        }

        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
    {
        private SebIndicators.MyBarSize[] cacheMyBarSize;
        public SebIndicators.MyBarSize MyBarSize(int averagePeriod, bool includeAverages)
        {
            return MyBarSize(Input, averagePeriod, includeAverages);
        }

        public SebIndicators.MyBarSize MyBarSize(ISeries<double> input, int averagePeriod, bool includeAverages)
        {
            if (cacheMyBarSize != null)
                for (int idx = 0; idx < cacheMyBarSize.Length; idx++)
                    if (cacheMyBarSize[idx] != null && cacheMyBarSize[idx].AveragePeriod == averagePeriod && cacheMyBarSize[idx].IncludeAverages == includeAverages && cacheMyBarSize[idx].EqualsInput(input))
                        return cacheMyBarSize[idx];
            return CacheIndicator<SebIndicators.MyBarSize>(new SebIndicators.MyBarSize() { AveragePeriod = averagePeriod, IncludeAverages = includeAverages }, input, ref cacheMyBarSize);
        }
    }
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
    public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
    {
        public Indicators.SebIndicators.MyBarSize MyBarSize(int averagePeriod, bool includeAverages)
        {
            return indicator.MyBarSize(Input, averagePeriod, includeAverages);
        }

        public Indicators.SebIndicators.MyBarSize MyBarSize(ISeries<double> input, int averagePeriod, bool includeAverages)
        {
            return indicator.MyBarSize(input, averagePeriod, includeAverages);
        }
    }
}

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        public Indicators.SebIndicators.MyBarSize MyBarSize(int averagePeriod, bool includeAverages)
        {
            return indicator.MyBarSize(Input, averagePeriod, includeAverages);
        }

        public Indicators.SebIndicators.MyBarSize MyBarSize(ISeries<double> input, int averagePeriod, bool includeAverages)
        {
            return indicator.MyBarSize(input, averagePeriod, includeAverages);
        }
    }
}

#endregion
