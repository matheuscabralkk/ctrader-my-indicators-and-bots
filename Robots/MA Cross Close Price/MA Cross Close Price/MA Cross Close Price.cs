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
    public class MACrossClosePrice : Robot
    {
        [Parameter("Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("MA Type", Group = "Moving Average")]
        public MovingAverageType MAType { get; set; }

        [Parameter("Source", Group = "Moving Average")]
        private DataSeries SourceSeries { get; set; }

        [Parameter("Slow Periods", Group = "Moving Average", DefaultValue = 10)]
        public int Periods { get; set; } 
        
        public Position[] BotPositions
        {
            get { return Positions.FindAll(label, SymbolName); }
        }


        private MovingAverage ma;
        private const string label = "Ma Cross Close Price";
        
        bool hadLongPosOpen = false;            
        bool hadShortPosOpen = false;

        protected override void OnStart()
        {
            ma = Indicators.MovingAverage(SourceSeries, Periods, MAType);
        }

        protected override void OnBar()
        {
            bool noLongPosition = Positions.Find(label, SymbolName, TradeType.Buy) == null;
            bool noShortPosition = Positions.Find(label, SymbolName, TradeType.Sell) == null;

            bool isLongSignal = false;
            bool isShortSignal = false;
            if (ma.Result.Last(2) > Bars.ClosePrices.Last(2) && ma.Result.Last(1) < Bars.ClosePrices.Last(1)) {
                isLongSignal = true;
            }
            else if (ma.Result.Last(2) < Bars.ClosePrices.Last(2) && ma.Result.Last(1) > Bars.ClosePrices.Last(1))
            {
                isShortSignal = true;
            }
            
            
            
            //TODO ao abrir uma posição, cancela todas as outras posições pendentes
            if (isLongSignal && noLongPosition && !hadLongPosOpen)
            {
                ClosePositionsIfExists();
                OpenTrade(TradeType.Buy);
                hadShortPosOpen = false;
                hadLongPosOpen = true;
            }
            else if (isShortSignal && noShortPosition && !hadShortPosOpen)
            {
                ClosePositionsIfExists();
                OpenTrade(TradeType.Sell);
                hadLongPosOpen = false;
                hadShortPosOpen = true;
            }
            
            
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

