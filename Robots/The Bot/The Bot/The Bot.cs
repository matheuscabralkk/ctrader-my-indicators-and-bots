using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TheBot : Robot
    {
        // [Parameter("Label", Group = "Settings", DefaultValue = "The bot")]
        // public string label { get; set; }
        public string pmaxLabel = "pmaxLabel";
        public string rsiLabel = "rsiLabel";

        [Parameter("Martigale?", Group = "Martigale", DefaultValue = false)]
        public bool Martigale { get; set; }

        [Parameter("Max Positions", Group = "Martigale", DefaultValue = 2)]
        public int MaxPositions { get; set; }

        // [Parameter("Stop Loss", Group = "Martigale", DefaultValue = 40)]
        // public int StopLoss { get; set; }

        // [Parameter("Take Profit", Group = "Martigale", DefaultValue = 40)]
        // public int TakeProfit { get; set; }

        // [Parameter("Take Profit?", Group = "Martigale", DefaultValue = false)]
        // public bool TakeProfitMode { get; set; }

        [Parameter("Quantity (Lots)", Group = "Settings", DefaultValue = 0.1, MinValue = 0.01, Step = 0.1)]
        public double Quantity { get; set; }

        [Parameter("Only trade within certain hours?", Group = "HourlyTrade", DefaultValue = false)]
        public bool HourlyTrade { get; set; }

        [Parameter("Start Trade Session", Group = "HourlyTrade", DefaultValue = 4, MinValue = 0, MaxValue = 23)]
        public int StartTradeSession { get; set; }

        [Parameter("End hour", Group = "HourlyTrade", DefaultValue = 12, MinValue = 0, MaxValue = 23)]
        public int EndTradeSession { get; set; }

        [Parameter("Period", Group = "Pmax Params", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int PmaxPeriod { get; set; }

        [Parameter("Change ATR Calculation?", Group = "Pmax Params", DefaultValue = true)]
        public bool SmoothedAtr { get; set; }

        [Parameter("MA Type", Group = "Pmax Params")]
        public MovingAverageType MAType { get; set; }

        [Parameter("MA Source", Group = "Pmax Params")]
        public DataSeries PmaxSourceSeries { get; set; }

        [Parameter("MA Lenght", Group = "Pmax Params", DefaultValue = 200, MinValue = 1, Step = 1)]
        public int MALenght { get; set; }

        [Parameter("Source", DefaultValue = "Close", Group = "RSI Params")]
        public DataSeries RsiSourceSeries { get; set; }

        [Parameter("Periods", DefaultValue = 14, Group = "RSI Params")]
        public int RsiPeriods { get; set; }
        
        [Parameter("Rsi Bars", DefaultValue = 14, Group = "RSI Params")]
        public int RsiBars { get; set; }
        
        [Parameter("Open Position Difference", DefaultValue = 20, MaxValue = 48, MinValue = 2, Group = "RSI Params")]
        public int OpenPosDifference { get; set; }
        
        [Parameter("Take Profit Position Difference", DefaultValue = 20, MaxValue = 48, MinValue = 2, Group = "RSI Params")]
        public int TakeProfitDifference { get; set; }

        private PMAXIndicatorv2 _pmax;
        private RelativeStrengthIndex _rsi;

        bool isShortSignal = false;
        bool isLongSignal = false;

        bool firstShortPosOpened = false;
        bool firstLongPosOpened = false;

        private int rsiBars = 0;
        private int rsiPosOpeneds = 0;
        

        public Position[] BotPositions(String label)
        {
            return Positions.FindAll(null, SymbolName);
        }

        protected override void OnStart()
        {
            _pmax = Indicators.GetIndicator<PMAXIndicatorv2>(PmaxPeriod, SmoothedAtr, MAType, PmaxSourceSeries, MALenght);
            _rsi = Indicators.RelativeStrengthIndex(RsiSourceSeries, RsiPeriods);
        }

        protected override void OnBar()
        {
            if (rsiPosOpeneds > 0) {
                rsiBars++;
            }

            /*
            TODO - take profit based on net profit
            var openPoisitionNetProfit = Positions.FindAll(label).Sum(position => position.NetProfit);
            Print("--> ", openPoisitionNetProfit);
            if (TakeProfitMode)
            {
                if (openPoisitionNetProfit > TakeProfit)
                {
                    ClosePositionsIfExists();
                }
            }
            */


            double lowerBand = _pmax.LowerBand.Last(1);
            double upperBand = _pmax.UpperBand.Last(1);
            double rsi = _rsi.Result.Last(1);

            isLongSignal = GetSignal(lowerBand);
            isShortSignal = GetSignal(upperBand);


            //TODO ao abrir uma posição, cancela todas as outras posições pendentes
            if (isLongSignal)
            {
                if (!firstLongPosOpened)
                {
                    // open first long position
                    firstLongPosOpened = true;
                    firstShortPosOpened = false;
                    ClosePositionsIfExists(pmaxLabel);
                    OpenTrade(TradeType.Buy, pmaxLabel);
                    rsiBars = 0;
                    rsiPosOpeneds = 0;
                }
                else {
                    if (rsi < (50 - OpenPosDifference))
                    {
                        if (rsiBars >= RsiBars || rsiPosOpeneds == 0)
                        {
                            // open another position because rsi
                            OpenTrade(TradeType.Buy, rsiLabel);
                            rsiPosOpeneds++;
                            rsiBars = 0;
                        }
                    }
                    // take profit
                    else if (rsiPosOpeneds > 0 && rsi > (50 + TakeProfitDifference)) {
                        ClosePositionsIfExists(rsiLabel);
                        rsiPosOpeneds = 0;
                        rsiBars = 0;
                    }
                }
            }
            // same but inverse
            else if (isShortSignal)
            {
                if (!firstShortPosOpened)
                {
                    // open first short position
                    firstShortPosOpened = true;
                    firstLongPosOpened = false;
                    ClosePositionsIfExists(pmaxLabel);
                    OpenTrade(TradeType.Sell, pmaxLabel);
                    rsiBars = 0;
                    rsiPosOpeneds = 0;
                }
                else {
                    if (rsi > (50 + OpenPosDifference))
                    {
                        if (rsiBars >= RsiBars || rsiPosOpeneds == 0)
                        {
                            // open another position because rsi
                            OpenTrade(TradeType.Sell, rsiLabel);
                            rsiPosOpeneds++;
                            rsiBars = 0;
                        }
                     } else if (rsiPosOpeneds > 0 && rsi < (50 - TakeProfitDifference)) {
                        // take profit
                        ClosePositionsIfExists(rsiLabel);
                        rsiPosOpeneds = 0;
                        rsiBars = 0;
                    }
                }

            }

        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        private void OpenTrade(TradeType tradeType, String label)
        {
            double positionSize;
            if (Martigale)
            {
                positionSize = VolumeInUnits * (BotPositions(null).Length + 1);
            }
            else
            {
                positionSize = VolumeInUnits;
            }

            if (HourlyTrade)
            {
                if (Server.Time.Hour >= StartTradeSession && Server.Time.Hour < EndTradeSession)
                {
                    if (MaxPositions >= BotPositions(null).Length)
                    {
                        ExecuteMarketOrder(tradeType, SymbolName, positionSize, label);
                    }
                }
            }
            else
            {
                if (MaxPositions >= BotPositions(null).Length)
                {
                    ExecuteMarketOrder(tradeType, SymbolName, positionSize, label);
                }
            }
        }

        private void ClosePositionsIfExists(string label)
        {
            foreach (var position in BotPositions(label))
            {
                if (position.Label == label) {
                    ClosePosition(position);
                }
            }
        }

        private double VolumeInUnits
        {
            get { return Symbol.QuantityToVolumeInUnits(Quantity); }
        }

        private bool GetSignal(double band)
        {
            return !double.IsNaN(band);
        }

        private bool IsInReopenRegion(double band, double maLine, bool isLong)
        {
            double insiderLowerBand;
            double insiderUpperBand;
            if (isLong)
            {
                // lower region band
                double insiderBand = (maLine - band) / 2;
                insiderLowerBand = band + insiderBand;
                return Bars.Last(1).Close <= insiderLowerBand;
            }
            else
            {
                // lower region band
                double insiderBand = (band - maLine) / 2;
                insiderUpperBand = band - insiderBand;
                return Bars.Last(1).Close >= insiderUpperBand;
            }
        }
    }
}
