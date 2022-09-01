using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PMAXcBotModified : Robot
    {
        [Parameter("Label", Group = "Settings", DefaultValue = "PMAX cBot Modified")]
        public string label { get; set; }

        [Parameter("Quantity (Lots)", Group = "Settings", DefaultValue = 0.1, MinValue = 0.1, Step = 0.1)]
        public double Quantity { get; set; }

        [Parameter("Only trade within certain hours?", Group = "HourlyTrade", DefaultValue = false)]
        public bool HourlyTrade { get; set; }

        [Parameter("Start Trade Session", Group = "HourlyTrade", DefaultValue = 4, MinValue = 0, MaxValue = 23)]
        public int StartTradeSession { get; set; }

        [Parameter("End hour", Group = "HourlyTrade", DefaultValue = 12, MinValue = 0, MaxValue = 23)]
        public int EndTradeSession { get; set; }

        [Parameter("Period", Group = "Supertrend", DefaultValue = 3, MinValue = 1, Step = 1)]
        public int Period { get; set; }

        [Parameter("Multiplier", Group = "Supertrend", DefaultValue = 2.0, MinValue = 0.1, Step = 0.1)]
        public double Multiplier { get; set; }

        [Parameter("Change ATR Calculation?", Group = "Supertrend", DefaultValue = true)]
        public bool SmoothedAtr { get; set; }

        [Parameter("MA Type", Group = "Moving Average")]
        public MovingAverageType MAType { get; set; }

        [Parameter("MA Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("MA Lenght", Group = "Moving Average", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int MALenght { get; set; }

        private PMAXIndicator _pmax;
        private int canReopenPositionCount = 0;
        private int reopenPositionBarCount = 8;

        public Position[] BotPositions
        {
            get { return Positions.FindAll(label); }
        }

        protected override void OnStart()
        {
            _pmax = Indicators.GetIndicator<PMAXIndicator>(Period, Multiplier, SmoothedAtr, MAType, SourceSeries, MALenght);
        }

        protected override void OnBar()
        {
            double lowerBand = _pmax.LowerBand.Last(1);
            double upperBand = _pmax.UpperBand.Last(1);
            
            double maLine = _pmax.MALine.Last(1);

            var longPosition = Positions.Find(label, SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find(label, SymbolName, TradeType.Sell);


            // TODO se tiver perto do band, abre mais uma operação
            canReopenPositionCount++;
            if (double.IsNaN(upperBand))
            {
                double canReopenRegion = GetReopenRegion(lowerBand, maLine, false);
                if (longPosition == null) {
                    canReopenPositionCount = 0;
                    ClosePositionsIfExists();
                    OpenTrade(TradeType.Buy);
                } else if (canReopenPositionCount > reopenPositionBarCount && Bars.Last(1).Close <= canReopenRegion) {
                    canReopenPositionCount = 0;
                    OpenTrade(TradeType.Buy);
                }
            }
            else if (double.IsNaN(lowerBand))
            {
                double canReopenRegion = GetReopenRegion(upperBand, maLine, true);
                if (shortPosition == null) {
                    canReopenPositionCount = 0;
                    ClosePositionsIfExists();
                    OpenTrade(TradeType.Sell);
                } else if (canReopenPositionCount > reopenPositionBarCount && Bars.Last(1).Close >= canReopenRegion) {
                    canReopenPositionCount = 0;
                    OpenTrade(TradeType.Sell);
                }
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
        
        private double GetReopenRegion(double band, double maLine, bool upper)
        {
            if (!upper) {
                // is lower band (long position opened)
                double add = (maLine - band) / 2;
                return band + add;
            } else {
                // is upper band (short position opened)
                double remove = (band - maLine) / 2;
                return band - remove;
            }
        }
    }
}
