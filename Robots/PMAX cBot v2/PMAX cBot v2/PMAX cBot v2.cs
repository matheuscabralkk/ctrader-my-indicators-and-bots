using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PMAXcBotv2 : Robot
    {
        // [Parameter("Label", Group = "Settings", DefaultValue = "PMAX cBot v2")]
        // public string label { get; set; }
        public string label = null;

        [Parameter("Quantity (Lots)", Group = "Settings", DefaultValue = 0.1, MinValue = 0.01, Step = 0.1)] 
        public double Quantity { get; set; }

        [Parameter("Only trade within certain hours?", Group = "HourlyTrade", DefaultValue = false)]
        public bool HourlyTrade { get; set; }

        [Parameter("Start Trade Session", Group = "HourlyTrade", DefaultValue = 4, MinValue = 0, MaxValue = 23)]
        public int StartTradeSession { get; set; }

        [Parameter("End hour", Group = "HourlyTrade", DefaultValue = 12, MinValue = 0, MaxValue = 23)]
        public int EndTradeSession { get; set; }

        [Parameter("Period", Group = "Supertrend", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int Period { get; set; }

        [Parameter("Change ATR Calculation?", Group = "Supertrend", DefaultValue = true)]
        public bool SmoothedAtr { get; set; }

        [Parameter("MA Type", Group = "Moving Average")]
        public MovingAverageType MAType { get; set; }

        [Parameter("MA Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("MA Lenght", Group = "Moving Average", DefaultValue = 200, MinValue = 1, Step = 1)]
        public int MALenght { get; set; }

        private PMAXIndicatorv2 _pmax;
        
        bool hadLongPosOpen = false;            
        bool hadShortPosOpen = false;

        public Position[] BotPositions
        {
            get { return Positions.FindAll(label, SymbolName); }
        }

        protected override void OnStart()
        {
            _pmax = Indicators.GetIndicator<PMAXIndicatorv2>(Period, SmoothedAtr, MAType, SourceSeries, MALenght);
        }

        protected override void OnBar()
        {
            double lowerBand = _pmax.LowerBand.Last(1);
            double upperBand = _pmax.UpperBand.Last(1);
            
            bool isLongSignal = GetSignal(lowerBand);
            bool isShortSignal = GetSignal(upperBand);
            
            Print("-------");
            Print(isLongSignal);
            Print(isShortSignal);

            bool noLongPosition = Positions.Find(label, SymbolName, TradeType.Buy) == null;
            bool noShortPosition = Positions.Find(label, SymbolName, TradeType.Sell) == null;
            

            //TODO ao abrir uma posição, cancela todas as outras posições pendentes
            // TODO ma lenght 0; then calcular MA indo pra cima/baixo

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
            return !double.IsNaN(band);
        }
    }
}
