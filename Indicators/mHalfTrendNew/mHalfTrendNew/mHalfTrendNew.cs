using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class mHalfTrendNew : Indicator
    {
        [Parameter("Period (5) ", DefaultValue = 5, Group = "Main")]
        public int inpAmplitude { get; set; }
        [Parameter("MA Smooth Type", DefaultValue = MovingAverageType.Simple, Group = "Main")]
        public MovingAverageType MaTypeSmooth { get; set; }


        [Output("Half Trend New", LineColor = "Black", PlotType = PlotType.Line, LineStyle = LineStyle.Solid, Thickness = 1)]
        public IndicatorDataSeries outHTn { get; set; }
        [Output("Half Trend New - Bullish", LineColor = "Green", PlotType = PlotType.DiscontinuousLine, LineStyle = LineStyle.Solid, Thickness = 2)]
        public IndicatorDataSeries outHTnBullish { get; set; }
        [Output("Half Trend New - Bearish", LineColor = "Red", PlotType = PlotType.DiscontinuousLine, LineStyle = LineStyle.Solid, Thickness = 2)]
        public IndicatorDataSeries outHTnBearish { get; set; }


        private MovingAverage _mah;
        private MovingAverage _mal;
        private IndicatorDataSeries _hh;
        private IndicatorDataSeries _ll;
        private IndicatorDataSeries _htn;
        private IndicatorDataSeries _htnbullish;
        private IndicatorDataSeries _htnbearish;


        protected override void Initialize()
        {
            _mah = Indicators.MovingAverage(Bars.HighPrices, inpAmplitude, MaTypeSmooth);
            _mal = Indicators.MovingAverage(Bars.LowPrices, inpAmplitude, MaTypeSmooth);
            _hh = CreateDataSeries();
            _ll = CreateDataSeries();
            _htn = CreateDataSeries();
            _htnbullish = CreateDataSeries();
            _htnbearish = CreateDataSeries();
        }

        public override void Calculate(int i)
        {
            if (i < inpAmplitude + 2)
            {
                _htn[i] = _htnbullish[i] = _htnbearish[i] = Bars.ClosePrices[i];
                outHTn[i] = _htn[i];
                outHTnBullish[i] = _htnbullish[i];
                outHTnBearish[i] = _htnbearish[i];
                return;
            }

            _hh[i] = Bars.HighPrices.Maximum(inpAmplitude - 1);
            _ll[i] = Bars.LowPrices.Minimum(inpAmplitude - 1);


            _htn[i] = _htn[i - 1];
            _htnbullish[i] = _htnbullish[i - 1];
            _htnbearish[i] = _htnbearish[i - 1];

            if (_mah.Result[i] < _htn[i - 1] && _mal.Result[i] < _htn[i - 1] && _hh[i - 1] < _htn[i - 1])
            {
                _htn[i] = _hh[i - 1];

            }
            if (_mah.Result[i] > _htn[i - 1] && _mal.Result[i] > _htn[i - 1] && _ll[i - 1] > _htn[i - 1])
            {
                _htn[i] = _ll[i - 1];

            }



            if (_htn[i] > _htn[i - 1])
            {
                _htnbullish[i] = _htn[i];
                _htnbearish[i] = double.NaN;
            }
            if (_htn[i] < _htn[i - 1])
            {
                _htnbullish[i] = double.NaN;
                _htnbearish[i] = _htn[i];
            }

            outHTn[i] = _htn[i];
            outHTnBullish[i] = _htnbullish[i];
            outHTnBearish[i] = _htnbearish[i];
        }
    }
}
