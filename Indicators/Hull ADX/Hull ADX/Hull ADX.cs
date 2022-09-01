using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator("DMS", IsOverlay = false, ScalePrecision = 0, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Hull_ADX : Indicator
    {
        [Parameter("Period", DefaultValue = 90, MinValue = 1)]
        public int Period { get; set; }

        [Output("ADX", Color = Colors.Cyan)]
        public IndicatorDataSeries HullADX { get; set; }

        [Output("DI+", Color = Colors.Green)]
        public IndicatorDataSeries DiPlus { get; set; }

        [Output("Di-", Color = Colors.Red)]
        public IndicatorDataSeries DiMinus { get; set; }

        private DataSeries high, low, close;
        private IndicatorDataSeries ds, hdmPlus, hdmMinus, hadx;

        private HullMovingAverage hmaTr, hmaDmPlus, hmaDmMinus, hmaAdx;

        protected override void Initialize()
        {
            high = MarketSeries.High;
            low = MarketSeries.Low;
            close = MarketSeries.Close;

            ds = CreateDataSeries();
            hdmPlus = CreateDataSeries();
            hdmMinus = CreateDataSeries();
            hadx = CreateDataSeries();

            hmaTr = Indicators.GetIndicator<HullMovingAverage>(ds, Period);
            hmaDmPlus = Indicators.GetIndicator<HullMovingAverage>(hdmPlus, Period);
            hmaDmMinus = Indicators.GetIndicator<HullMovingAverage>(hdmMinus, Period);
            hmaAdx = Indicators.GetIndicator<HullMovingAverage>(hadx, Period);
        }
        public override void Calculate(int index)
        {


            ds[index] = Math.Max(Math.Abs(low[index] - close[index - 1]), Math.Max(Math.Abs(high[index] - close[index - 1]), high[index] - low[index]));
            hdmPlus[index] = high[index] - high[index - 1] > low[index - 1] - low[index] ? Math.Max(high[index] - high[index - 1], 0) : 0;
            hdmMinus[index] = low[index - 1] - low[index] > high[index] - high[index - 1] ? Math.Max(low[index - 1] - low[index], 0) : 0;

            DiPlus[index] = 100 * (hmaTr.Result[index] == 0 ? 0 : hmaDmPlus.Result[index] / hmaTr.Result[index]);
            DiMinus[index] = 100 * (hmaTr.Result[index] == 0 ? 0 : hmaDmMinus.Result[index] / hmaTr.Result[index]);

            hadx[index] = Math.Abs((DiPlus[index] - DiMinus[index]) / (DiPlus[index] + DiMinus[index]));
            HullADX[index] = hadx[index] == 0 ? 50 : 100 * hmaAdx.Result[index];

        }
    }
}
