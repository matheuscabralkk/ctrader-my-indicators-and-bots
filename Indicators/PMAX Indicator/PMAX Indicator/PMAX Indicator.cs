using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class PMAXIndicator : Indicator
    {
        // INPUT
        [Parameter("Period", Group = "Supertrend", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int Period { get; set; }

        [Parameter("Multiplier", Group = "Supertrend", DefaultValue = 3.0, MinValue = 0.1, Step = 0.1)]
        public double Multiplier { get; set; }

        [Parameter("Change ATR Calculation?", Group = "Supertrend", DefaultValue = true)]
        public bool SmoothedAtr { get; set; }

        [Parameter("MA Type", Group = "Moving Average")]
        public MovingAverageType MAType { get; set; }

        [Parameter("MA Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("MA Lenght", Group = "Moving Average", DefaultValue = 10, MinValue = 2, Step = 20)]
        public int MALenght { get; set; }

        // OUTPUT
        [Output("LowerBand", LineColor = "green", PlotType = PlotType.DiscontinuousLine, Thickness = 3)]
        public IndicatorDataSeries LowerBand { get; set; }

        [Output("UpperBand", LineColor = "red", PlotType = PlotType.DiscontinuousLine, Thickness = 3)]
        public IndicatorDataSeries UpperBand { get; set; }

        [Output("MA Line", LineColor = "yellow", PlotType = PlotType.Points, Thickness = 1)]
        public IndicatorDataSeries MALine { get; set; }


        private IndicatorDataSeries _upperBandBuffer;
        private IndicatorDataSeries _lowerBandBuffer;
        private IndicatorDataSeries _trend;
        private AverageTrueRange _averageTrueRange;
        private MovingAverage _ma;
        private bool _changeofTrend;

        protected override void Initialize()
        {
            _trend = CreateDataSeries();
            _upperBandBuffer = CreateDataSeries();
            _lowerBandBuffer = CreateDataSeries();
            _averageTrueRange = Indicators.AverageTrueRange(Period, SmoothedAtr ? MovingAverageType.WilderSmoothing : MovingAverageType.Exponential);
            _ma = Indicators.MovingAverage(SourceSeries, MALenght, MAType);
        }

        public override void Calculate(int index)
        {
            LowerBand[index] = double.NaN;
            UpperBand[index] = double.NaN;

            MALine[index] = _ma.Result[index];

            // double median = (Bars.HighPrices[index] + Bars.LowPrices[index]) / 2;
            // double close = Bars.ClosePrices[index];

            double atr = _averageTrueRange.Result[index];

            _upperBandBuffer[index] = MALine[index] + Multiplier * atr;
            _lowerBandBuffer[index] = MALine[index] - Multiplier * atr;



            if (index < 1)
            {
                _trend[index] = 1;
                return;
            }


            if (MALine[index] > _upperBandBuffer[index - 1])
            {
                _trend[index] = 1;
                if (_trend[index - 1] == -1)
                    _changeofTrend = true;
            }
            else if (MALine[index] < _lowerBandBuffer[index - 1])
            {
                _trend[index] = -1;
                if (_trend[index - 1] == 1)
                    _changeofTrend = true;
            }
            else if (_trend[index - 1] == 1)
            {
                _trend[index] = 1;
                _changeofTrend = false;
            }
            else if (_trend[index - 1] == -1)
            {
                _trend[index] = -1;
                _changeofTrend = false;
            }


            if (_trend[index] < 0 && _trend[index - 1] > 0)
                _upperBandBuffer[index] = MALine[index] + (Multiplier * atr);
            else if (_trend[index] < 0 && _upperBandBuffer[index] > _upperBandBuffer[index - 1])
                _upperBandBuffer[index] = _upperBandBuffer[index - 1];

            if (_trend[index] > 0 && _trend[index - 1] < 0)
                _lowerBandBuffer[index] = MALine[index] - (Multiplier * atr);
            else if (_trend[index] > 0 && _lowerBandBuffer[index] < _lowerBandBuffer[index - 1])
                _lowerBandBuffer[index] = _lowerBandBuffer[index - 1];

            // Draw Bands

            if (_trend[index] == 1)
            {
                LowerBand[index] = _lowerBandBuffer[index];
                if (_changeofTrend)
                {
                    LowerBand[index - 1] = UpperBand[index - 1];
                    _changeofTrend = false;
                }
            }
            else if (_trend[index] == -1)
            {
                UpperBand[index] = _upperBandBuffer[index];
                if (_changeofTrend)
                {
                    UpperBand[index - 1] = LowerBand[index - 1];
                    _changeofTrend = false;
                }
            }
        }
    }
}
