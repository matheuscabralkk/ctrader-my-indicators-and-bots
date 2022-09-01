using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class SupertrendCustom : Indicator
    {
        [Parameter(DefaultValue = 10)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 3.0)]
        public double Multiplier { get; set; }

        [Parameter(DefaultValue = true)]
        public bool SmoothedAtr { get; set; }

        [Output("LowerBand", LineColor = "green", PlotType = PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries LowerBand { get; set; }

        [Output("UpperBand", LineColor = "red", PlotType = PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries UpperBand { get; set; }

        private IndicatorDataSeries _upBuffer;
        private IndicatorDataSeries _downBuffer;
        private AverageTrueRange _averageTrueRange;
        private int[] _trend;
        private bool _changeofTrend;

        protected override void Initialize()
        {
            _trend = new int[1];
            _upBuffer = CreateDataSeries();
            _downBuffer = CreateDataSeries();
            _averageTrueRange = Indicators.AverageTrueRange(Period, SmoothedAtr ? MovingAverageType.WilderSmoothing : MovingAverageType.Simple);
        }

        public override void Calculate(int index)
        {
            // Init
            LowerBand[index] = double.NaN;
            UpperBand[index] = double.NaN;

            double median = (Bars.HighPrices[index] + Bars.LowPrices[index]) / 2;
            double atr = _averageTrueRange.Result[index];

            _upBuffer[index] = median + Multiplier * atr;
            _downBuffer[index] = median - Multiplier * atr;


            if (index < 1)
            {
                _trend[index] = 1;
                return;
            }

            Array.Resize(ref _trend, _trend.Length + 1);

            // Main Logic
            // check wheter trend direction changed
            if (Bars.ClosePrices[index] > _upBuffer[index - 1])
            {
                _trend[index] = 1;
                if (_trend[index - 1] == -1)
                    _changeofTrend = true;
            }
            else if (Bars.ClosePrices[index] < _downBuffer[index - 1])
            {
                _trend[index] = -1;
                if (_trend[index - 1] == -1)
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

            // é pra linha da trend não voltar pra cima/baixo qndo o preço voltar pro lado contrário

            if (_trend[index] < 0 && _trend[index - 1] > 0)
                _upBuffer[index] = median + (Multiplier * atr);
            else if (_trend[index] < 0 && _upBuffer[index] > _upBuffer[index - 1])
                _upBuffer[index] = _upBuffer[index - 1];

            if (_trend[index] > 0 && _trend[index - 1] < 0)
                _downBuffer[index] = median - (Multiplier * atr);
            else if (_trend[index] > 0 && _downBuffer[index] < _downBuffer[index - 1])
                _downBuffer[index] = _downBuffer[index - 1];

            // Draw Indicator
            if (_trend[index] == 1)
            {
                LowerBand[index] = _downBuffer[index];
                if (_changeofTrend)
                {
                    LowerBand[index - 1] = UpperBand[index - 1];
                    _changeofTrend = false;
                }
            }
            else if (_trend[index] == -1)
            {
                UpperBand[index] = _upBuffer[index];
                if (_changeofTrend)
                {
                    UpperBand[index - 1] = LowerBand[index - 1];
                    _changeofTrend = false;
                }
            }
        }
    }
}
