using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RSIRangeRobot : Robot
    {
        // [Parameter("Label", Group = "Settings", DefaultValue = "PMAX cBot v2")]
        // public string label { get; set; }
        public string label = null;
            
        [Parameter("Source", DefaultValue = "Close")]
        public DataSeries Source { get; set; }
        
        [Parameter("Periods", DefaultValue = 14)]
        public int Periods { get; set; }
        
        [Parameter("Difference", DefaultValue = 20, MaxValue = 48, MinValue = 2)]
        public int Difference { get; set; }
        
        // [Parameter("Upper Value", DefaultValue = 70, MaxValue = 95, MinValue = 51)]
        // public int UpperValue { get; set; }
        
        // [Parameter("Lower Value", DefaultValue = 30, MaxValue = 49, MinValue = 5)]
        // public int LowerValue { get; set; }
        
        [Parameter("Quantity (Lots)", Group = "Settings", DefaultValue = 0.1, MinValue = 0.01, Step = 0.1)] 
        public double Quantity { get; set; }
        
        private RelativeStrengthIndex _rsi;
        
        public Position[] BotPositions
        {
            get { return Positions.FindAll(label, SymbolName); }
        }
        
        protected override void OnStart()
        {
            _rsi = Indicators.RelativeStrengthIndex(Source, Periods);
        }
        
        protected override void OnTick()
        {
            bool noLongPosition = Positions.Find(label, SymbolName, TradeType.Buy) == null;
            bool noShortPosition = Positions.Find(label, SymbolName, TradeType.Sell) == null;
            
            if (_rsi.Result.LastValue < (50 - Difference) && noLongPosition)
            {
                ClosePositionsIfExists();
                OpenTrade(TradeType.Buy);
            }
            else if (_rsi.Result.LastValue > (50 + Difference) && noShortPosition)
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
            ExecuteMarketOrder(tradeType, SymbolName, VolumeInUnits, label);
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
    }
}
