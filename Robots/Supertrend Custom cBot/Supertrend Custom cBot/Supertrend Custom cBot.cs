using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System;
using System.Linq;

namespace cAlgo.Robots
{
    // This sample cBot shows how to use the Supertrend indicator
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SupertrendCustomcBot : Robot
    {
        [Parameter("Quantity (Lots)", Group = "Quantidade", DefaultValue = 1, MinValue = 0.1, Step = 0.1)]
        public double Quantity { get; set; }

        [Parameter("Period", Group = "Supertrend", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int Period { get; set; }

        [Parameter("Multiplier", Group = "Supertrend", DefaultValue = 3, MinValue = 0.1, Step = 0.1)]
        public double Multiplier { get; set; }

        [Parameter("Smoothed ATR?", Group = "Supertrend", DefaultValue = true)]
        public bool SmoothedAtr { get; set; }

        [Parameter("Label", DefaultValue = "Supertrend cBot")]
        public string label { get; set; }

        private SupertrendCustom supertrend;

        public Position[] BotPositions
        {
            get { return Positions.FindAll(label); }
        }

        protected override void OnStart()
        {
            supertrend = Indicators.GetIndicator<SupertrendCustom>(Period, Multiplier, SmoothedAtr);
        }
        protected override void OnBar()
        {
            double currentUpperBand = supertrend.UpperBand.Last(1);
            double currentLowerBand = supertrend.LowerBand.Last(1);
            
            bool noLongPosition = Positions.Find(label, SymbolName, TradeType.Buy) == null;
            bool noShortPosition = Positions.Find(label, SymbolName, TradeType.Sell) == null;
            
            if (double.IsNaN(currentUpperBand) && noLongPosition)
            {
                ClosePositionsIfExists();
                ExecuteMarketOrder(TradeType.Buy, SymbolName, VolumeInUnits, label);
            }
            else if (double.IsNaN(currentLowerBand) && noShortPosition)
            {
                ClosePositionsIfExists();
                ExecuteMarketOrder(TradeType.Sell, SymbolName, VolumeInUnits, label);
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
    }
}
