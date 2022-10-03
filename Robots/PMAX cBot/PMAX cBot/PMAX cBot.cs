using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PMAXcBot : Robot
    {
        // [Parameter("Label", Group = "Settings", DefaultValue = "PMAX cBot")]
        // public string label { get; set; }
        public string label = null;

        [Parameter("Quantity (Lots)", Group = "Settings", DefaultValue = 0.1, MinValue = 0.1, Step = 0.1)]
        public double Quantity { get; set; }

        [Parameter("Only trade within certain hours?", Group = "HourlyTrade", DefaultValue = false)]
        public bool HourlyTrade { get; set; }

        [Parameter("Start Trade Session", Group = "HourlyTrade", DefaultValue = 4, MinValue = 0, MaxValue = 23)]
        public int StartTradeSession { get; set; }

        [Parameter("End hour", Group = "HourlyTrade", DefaultValue = 12, MinValue = 0, MaxValue = 23)]
        public int EndTradeSession { get; set; }

        [Parameter("Period", Group = "Supertrend", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int Period { get; set; }

        [Parameter("Multiplier", Group = "Supertrend", DefaultValue = 8.0, MinValue = 0.1, Step = 0.1)]
        public double Multiplier { get; set; }

        [Parameter("Change ATR Calculation?", Group = "Supertrend", DefaultValue = true)]
        public bool SmoothedAtr { get; set; }

        [Parameter("MA Type", Group = "Moving Average")]
        public MovingAverageType MAType { get; set; }

        [Parameter("MA Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("MA Lenght", Group = "Moving Average", DefaultValue = 200, MinValue = 1, Step = 1)]
        public int MALenght { get; set; }

        private PMAXIndicator _pmax;

        public Position[] BotPositions
        {
            get { return Positions.FindAll(label, SymbolName); }
        }

        protected override void OnStart()
        {
            _pmax = Indicators.GetIndicator<PMAXIndicator>(Period, Multiplier, SmoothedAtr, MAType, SourceSeries, MALenght);
        }

        protected override void OnBar()
        {
            double lowerBand = _pmax.LowerBand.Last(1);
            double upperBand = _pmax.UpperBand.Last(1);
            
            bool isLongSignal = GetSignal(upperBand);
            bool isShortSignal = GetSignal(lowerBand);

            bool noLongPosition = Positions.Find(label, SymbolName, TradeType.Buy) == null;
            bool noShortPosition = Positions.Find(label, SymbolName, TradeType.Sell) == null;

            if (isLongSignal && noLongPosition)
            {
                ClosePositionsIfExists();
                OpenTrade(TradeType.Buy);
            }
            else if (isShortSignal && noShortPosition)
            {
                ClosePositionsIfExists();
                OpenTrade(TradeType.Sell);
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        private void OpenTrade(TradeType tradeType)
        {
            if (HourlyTrade)
            {
                if (Server.Time.Hour >= StartTradeSession && Server.Time.Hour < EndTradeSession)
                {
                    ExecuteMarketOrder(tradeType, SymbolName, VolumeInUnits, label);
                }
            }
            else
            {
                ExecuteMarketOrder(tradeType, SymbolName, VolumeInUnits, label);
            }
        }

        private void ClosePositionsIfExists()
        {
            foreach (var position in BotPositions)
            {
                ClosePosition(position);
            }
        }
        
        private double VolumeInUnits
        {
            get { return Symbol.QuantityToVolumeInUnits(Quantity); }
        }
        
        private bool GetSignal(double band) {
            return double.IsNaN(band);
        }
    }
}
