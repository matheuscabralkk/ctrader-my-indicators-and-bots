using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class MaCrossClosePrice : Indicator
    {
        [Parameter("MA Type", Group = "Moving Average")]
        public MovingAverageType MAType { get; set; }

        [Parameter("MA Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("MA Lenght", Group = "Moving Average", DefaultValue = 10, MinValue = 2, MaxValue = 1000, Step = 10)]
        public int MALenght { get; set; }

        // OUTPUT
        [Output("LowerBand", LineColor = "green", PlotType = PlotType.DiscontinuousLine, Thickness = 3)]
        public IndicatorDataSeries MaLineAbove { get; set; }

        [Output("UpperBand", LineColor = "blue", PlotType = PlotType.DiscontinuousLine, Thickness = 3)]
        public IndicatorDataSeries MaLineBelow { get; set; }
 
        private MovingAverage _ma;

        protected override void Initialize()
        {
            _ma = Indicators.MovingAverage(SourceSeries, MALenght, MAType);
        }

        public override void Calculate(int index)
        {

            // double median = (Bars.HighPrices[index] + Bars.LowPrices[index]) / 2;
            
            double close = Bars.ClosePrices[index - 1];
            double maResult = _ma.Result[index - 1];
            
            if (maResult > close) {
                MaLineBelow[index - 1] = _ma.Result[index - 1];
                MaLineBelow[index] = _ma.Result[index];
            } else {
                MaLineAbove[index - 1] = _ma.Result[index - 1];
                MaLineAbove[index] = _ma.Result[index];
            }
            
        }
    }
}
