#region Using declarations
using NinjaTrader.Gui;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
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
				Description = @"Custom True Range (TR) and Average True Range (ATR) Indicator in ticks or in points.";
				Name = "TR_ATR";

                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

				IsChartOnly = true;
				DrawOnPricePanel = true;
                IsOverlay = true;
                IsAutoScale = false;
                ShowTransparentPlotsInDataBox = true;
                Calculate = Calculate.OnPriceChange;

                ATR_Period = 14;
                DisplayInTicks = true;
				RiskAddTicks = 2;

                AddPlot(new Stroke(Brushes.DarkCyan), PlotStyle.PriceBox, "ATR");
                AddPlot(new Stroke(Brushes.LightPink), PlotStyle.PriceBox, "BarSize");
                AddPlot(new Stroke(Brushes.Crimson), PlotStyle.PriceBox, "Risk");

            }
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			double high0 = High[0];
			double low0	 = Low[0];

			//if (CurrentBar == 0)
			//{
			//	Values[0][0] = (DisplayInTicks) ? (high0 - low0) / TickSize : high0 - low0;
			//	Values[1][0] = (DisplayInTicks) ? (high0 - low0) / TickSize : high0 - low0;

			//}
			//else
			{
                if (DisplayInTicks)
				{
                    Values[0][0] = ATR(ATR_Period)[0] / TickSize;
                    Values[1][0] = ATR(1)[0] / TickSize; // ATR(1) = TR
					Values[2][0] = (ATR(1)[0] / TickSize) + RiskAddTicks;

                }
				else
				{
                    Values[0][0] = ATR(ATR_Period)[0];
                    Values[1][0] = ATR(1)[0]; // ATR(1) = TR	
                    Values[2][0] = ATR(1)[0] + RiskAddTicks;				
                }
            }
		}

		#region Properties

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "ATR Period", GroupName = "Parameters", Order = 0)]
		public int ATR_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Display in Ticks", Description="Calculate in ticks", GroupName="Parameters", Order = 1)]
		public bool DisplayInTicks
		{ get; set; }

		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Value of x in \"Risk = BarSize + x Ticks\"", GroupName = "Parameters", Order = 2)]
        public int RiskAddTicks
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
		public SebIndicators.TR_ATR TR_ATR(int aTR_Period, bool displayInTicks, int riskAddTicks)
		{
			return TR_ATR(Input, aTR_Period, displayInTicks, riskAddTicks);
		}

		public SebIndicators.TR_ATR TR_ATR(ISeries<double> input, int aTR_Period, bool displayInTicks, int riskAddTicks)
		{
			if (cacheTR_ATR != null)
				for (int idx = 0; idx < cacheTR_ATR.Length; idx++)
					if (cacheTR_ATR[idx] != null && cacheTR_ATR[idx].ATR_Period == aTR_Period && cacheTR_ATR[idx].DisplayInTicks == displayInTicks && cacheTR_ATR[idx].RiskAddTicks == riskAddTicks && cacheTR_ATR[idx].EqualsInput(input))
						return cacheTR_ATR[idx];
			return CacheIndicator<SebIndicators.TR_ATR>(new SebIndicators.TR_ATR(){ ATR_Period = aTR_Period, DisplayInTicks = displayInTicks, RiskAddTicks = riskAddTicks }, input, ref cacheTR_ATR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SebIndicators.TR_ATR TR_ATR(int aTR_Period, bool displayInTicks, int riskAddTicks)
		{
			return indicator.TR_ATR(Input, aTR_Period, displayInTicks, riskAddTicks);
		}

		public Indicators.SebIndicators.TR_ATR TR_ATR(ISeries<double> input , int aTR_Period, bool displayInTicks, int riskAddTicks)
		{
			return indicator.TR_ATR(input, aTR_Period, displayInTicks, riskAddTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SebIndicators.TR_ATR TR_ATR(int aTR_Period, bool displayInTicks, int riskAddTicks)
		{
			return indicator.TR_ATR(Input, aTR_Period, displayInTicks, riskAddTicks);
		}

		public Indicators.SebIndicators.TR_ATR TR_ATR(ISeries<double> input , int aTR_Period, bool displayInTicks, int riskAddTicks)
		{
			return indicator.TR_ATR(input, aTR_Period, displayInTicks, riskAddTicks);
		}
	}
}

#endregion
