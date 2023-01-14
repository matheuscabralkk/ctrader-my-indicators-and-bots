// -------------------------------------------------------------------------------------------------
//
//    This code is a cTrader Automate API example.
//
//    This cBot is intended to be used as a sample and does not guarantee any particular outcome or
//    profit of any kind. Use it at your own risk.
//    
//    All changes to this file might be lost on the next application update.
//    If you are going to modify this file please make a copy using the "Duplicate" command.
//
//    The "Sample Trend cBot" will buy when fast period moving average crosses the slow period moving average and sell when 
//    the fast period moving average crosses the slow period moving average. The orders are closed when an opposite signal 
//    is generated. There can only by one Buy or Sell order at any time.
//
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MACrossMATakeProfit : Robot
    {
        [Parameter("Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("Slow MA Type", Group = "Moving Average")]
        public MovingAverageType SlowMAType { get; set; }
                
        [Parameter("Fast MA Type == Slow MA Type?", Group = "Moving Average", DefaultValue = 50, MinValue = 2)]
        public bool SameMaType { get; set; }
        
        [Parameter("Fast MA Type", Group = "Moving Average")]
        public MovingAverageType FastMAType { get; set; }

        [Parameter("Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("Slow Periods", Group = "Moving Average", DefaultValue = 100, MinValue = 4)]
        public int SlowPeriods { get; set; }
        
        [Parameter("Fast Periods", Group = "Moving Average", DefaultValue = 50, MinValue = 2)]
        public int FastPeriods { get; set; }
        
        [Parameter("Reversal Mode?", Group = "Moving Average", DefaultValue = false)]
        public bool Reversal { get; set; }
        
        


        private MovingAverage slowMa;
        private MovingAverage fastMa;
        private AverageTrueRange atr;
        private const string label = "Sample Trend cBot";
        // private int ticksAfterOpenPos = 20;

        protected override void OnStart()
        {
            atr = Indicators.AverageTrueRange(14, MovingAverageType.Simple);
            slowMa = Indicators.MovingAverage(SourceSeries, SlowPeriods, SlowMAType);
            fastMa = Indicators.MovingAverage(SourceSeries, FastPeriods, SameMaType ? SlowMAType : FastMAType);
            if(!Reversal) {
                if (SlowPeriods <= FastPeriods) {
                    throw new Exception("Slow Periods must be greater than Fast Periods");
                }
            } else {
                if (SlowPeriods >= FastPeriods) {
                    throw new Exception("Fast Periods must be greater than Slow Periods");
                }
            }
        }

        protected override void OnTick()
        {
            var longPosition = Positions.Find(label, SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find(label, SymbolName, TradeType.Sell);

            var slowMaValue = slowMa.Result.Last(1);
            var fastMaValue = fastMa.Result.Last(1);
            
            bool sellSignal = slowMaValue > fastMaValue;
            bool longSignal = fastMaValue > slowMaValue;
            
            if (sellSignal && shortPosition == null)
            {
                if (longPosition != null)
                    ClosePosition(longPosition);
                    
                ExecuteMarketOrder(TradeType.Sell, SymbolName, VolumeInUnits, label);
            }
            else if (longSignal && longPosition == null)
            {
                if (shortPosition != null)
                    ClosePosition(shortPosition);
                    
                ExecuteMarketOrder(TradeType.Buy, SymbolName, VolumeInUnits, label);
            }
        }
        

        private double VolumeInUnits
        {
            get { return Symbol.QuantityToVolumeInUnits(Quantity); }
        }
    }
}
