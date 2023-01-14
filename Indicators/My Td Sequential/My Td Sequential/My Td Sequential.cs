using cAlgo.API;
using cAlgo.API.Indicators;
using System;
using System.Collections.Generic;

[Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
public class MyTdSequential : Indicator
{
    private int _count;
    private int _multiplier;
    
    [Output("Lower", PlotType = PlotType.Points)]
    public IndicatorDataSeries TDSequentialCount { get; set; }
    
    private List<int> _TDCountdown;
    
    [Parameter(DefaultValue = 13)]
    public int count
    {
        get { return _count; }
        set { _count = value; }
    }

    [Parameter(DefaultValue = 4)]
    public int multiplier
    {
        get { return _multiplier; }
        set { _multiplier = value; }
    }
    
    protected override void Initialize()
    {
        _TDCountdown = new List<int>();
    }
    public override void Calculate(int index)
    {
        int tdCount = 0;
        int countdown = 0;
        for (int j = 0; j < count; j++)
        {
            if (index - j < 0)
            {
                break;
            }
            if (Bars.ClosePrices[index - j] > Bars.ClosePrices[index - j - 1])
            {
                tdCount++;
            }
            else
            {
                tdCount = 0;
            }
            if (tdCount == 9)
            {
                TDSequentialCount.Add(index - j);
                break;
            }
            if (Bars.ClosePrices[index - j] < Bars.ClosePrices[index - j - 1])
            {
                countdown++;
            }
            else
            {
                countdown = 0;
            }
            if (countdown == 9)
            {
                _TDCountdown.Add(index - j * multiplier);
                break;
            }
        }
    }
}