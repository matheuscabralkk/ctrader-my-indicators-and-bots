using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class UTBOT : Indicator
    {
        [Parameter("Key", DefaultValue = 1)]
        public double keyvalue { get; set; }
        [Parameter("ATR Period", DefaultValue = 10)]
        public int atrperiod { get; set; }

        private AverageTrueRange xATR;

        [Output("UpTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "Green")]
        public IndicatorDataSeries XATRTrailingStopGreen { get; set; }
        [Output("Continuos", PlotType = PlotType.DiscontinuousLine, LineColor = "Green")]
        public IndicatorDataSeries XATRTrailingStop { get; set; }
        [Output("DownTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "Red")]
        public IndicatorDataSeries XATRTrailingStopRed { get; set; }

        protected override void Initialize()
        {
            xATR = Indicators.AverageTrueRange(atrperiod, MovingAverageType.Exponential);
        }

        public override void Calculate(int index)
        {
            double nLoss = keyvalue * xATR.Result[index];
            XATRTrailingStop[index] = (MarketSeries.Close[index] > XATRTrailingStop[index - 1] && MarketSeries.Close[index - 1] > XATRTrailingStop[index - 1]) ? Math.Max(XATRTrailingStop[index - 1], MarketSeries.Close[index] - nLoss) : (MarketSeries.Close[index] < XATRTrailingStop[index - 1] && MarketSeries.Close[index - 1] < XATRTrailingStop[index - 1]) ? Math.Min(XATRTrailingStop[index - 1], MarketSeries.Close[index] + nLoss) : (MarketSeries.Close[index] > XATRTrailingStop[index - 1]) ? MarketSeries.Close[index] - nLoss : MarketSeries.Close[index] + nLoss;
            XATRTrailingStopGreen[index] = XATRTrailingStop[index] < MarketSeries.Close[index] ? XATRTrailingStop[index] : double.NaN;
            XATRTrailingStopRed[index] = XATRTrailingStop[index] > MarketSeries.Close[index] ? XATRTrailingStop[index] : double.NaN;
        }
    }
}
