using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.Domain.RegionalForecasts.Services.RegionalForecastInfos;

class RfParamFBlockDomainService : IRfParamFBlockDomainService
{
    private readonly ILogger<RfParamFBlockDomainService> _logger;

    public RfParamFBlockDomainService(ILogger<RfParamFBlockDomainService> logger)
    {
        _logger = logger;
    }

    // IRegionalForecastDomainService
    public Task<BlockInfo[,,]> CalcRfInfoAsync(
        DateTime estimateDt,
        RfFieldSettings fieldSettings,
        RfParamFCalcSettings fCalcSettings,
        RfSeismoActivityEstimationSettings estimationSettings,
        IReadOnlyCollection<Event> seismoEvents)
    {
        var countOfBlocksX = Convert.ToInt32(Math.Floor((fieldSettings.FieldSize.X / fCalcSettings.BlockSize) * 2)) + 1;
        var countOfBlocksY = Convert.ToInt32(Math.Floor((fieldSettings.FieldSize.Y / fCalcSettings.BlockSize) * 2)) + 1;
        var countOfBlocksZ = Convert.ToInt32(Math.Floor((fieldSettings.FieldSize.Z / fCalcSettings.BlockSize) * 2)) + 1;

        BlockInfo[,,] blocksInfo = new BlockInfo[countOfBlocksX, countOfBlocksY, countOfBlocksZ];

        for (int z = 0; z < countOfBlocksZ; z++)
        {
            for (int y = 0; y < countOfBlocksY; y++)
            {
                for (int x = 0; x < countOfBlocksX; x++)
                {
                    var x1 = fieldSettings.FieldCoords.X + x * fCalcSettings.BlockSize / 2 - fCalcSettings.BlockSize / 2;
                    var y1 = fieldSettings.FieldCoords.Y + y * fCalcSettings.BlockSize / 2 - fCalcSettings.BlockSize / 2;
                    var z1 = fieldSettings.FieldCoords.Z + z * fCalcSettings.BlockSize / 2 - fCalcSettings.BlockSize / 2;
                    
                    var x2 = x1 + fCalcSettings.BlockSize;
                    var y2 = y1 + fCalcSettings.BlockSize;
                    var z2 = z1 + fCalcSettings.BlockSize;

                    var eventsInBlock = seismoEvents
                        .Where(x => x.Dt >= estimateDt.AddDays(-1 * fCalcSettings.TimeSpanRegInDays)
                            && x.Dt <= estimateDt)
                        .Where(x => x.X >= x1 && x.X <= x2)
                        .Where(x => x.Y >= y1 && x.Y <= y2)
                        .Where(x => x.Z >= z1 && x.Z <= z2)
                        .ToList();

                    var eCurrs = CalcCurrentEnergyOfEvents(estimateDt, fCalcSettings.TimeSpanMinImpactInDays, 
                        eventsInBlock);

                    var fBlock = CalcFBlock(
                        estimateDt: estimateDt,
                        tRegDays: fCalcSettings.TimeSpanRegInDays,
                        //tMinImpactDays: fCalcSettings.TimeSpanMinImpactInDays,
                        energyBackground: fCalcSettings.BackgroundEnergyValue,
                        currentEventsInBlock: eCurrs);

                    var eSum = eventsInBlock.Select(x => x.E).Sum();
                    
                    var maxCurrEnergy = 0.0;
                    if (eCurrs.Count > 0)
                    {
                        maxCurrEnergy = eCurrs.Max(x => x.ECurr);
                    }

                    var eCount = eCurrs.Count;
                    var blockCenterCoords = new BlockCenterCoords()
                    {
                        X = x1 + fCalcSettings.BlockSize / 2,
                        Y = y1 + fCalcSettings.BlockSize / 2,
                        Z = z1 + fCalcSettings.BlockSize / 2
                    };

                    var blockAssessment = CalcStatuses(fBlock, eSum, estimationSettings);

                    var blockInfo = new BlockInfo()
                    {
                        BlockCenterCoords = blockCenterCoords,
                        F = fBlock,
                        CountOfEvents = eCount,
                        TotalEnergy = eSum,
                        MaxCurrentEnergy = maxCurrEnergy,
                        BlockAssessment = blockAssessment,
                    };

                    blocksInfo[x, y, z] = blockInfo;
                }
            }
        }       

        return Task.FromResult(blocksInfo);
    }

    public IReadOnlyList<ParamFBlockInfo> CreateParamFBlockInfo(RegionalForecastBlocksInfo rfBlocksInfo)
    {        
        var result = new List<ParamFBlockInfo>();

        for (int x = 0; x < rfBlocksInfo.BlocksInfo.GetLength(0); x++)
        {
            for (int y = 0; y < rfBlocksInfo.BlocksInfo.GetLength(1); y++)
            {
                var indexOfMax = 0;
                var max = rfBlocksInfo.BlocksInfo[x, y, indexOfMax].F;                

                for (int z = 1; z < rfBlocksInfo.BlocksInfo.GetLength(2); z++)
                {
                    if (rfBlocksInfo.BlocksInfo[x, y, z].F > max)
                    {
                        max = rfBlocksInfo.BlocksInfo[x, y, z].F;
                        indexOfMax = z;
                    }
                }

                var tmp = rfBlocksInfo.BlocksInfo[x, y, indexOfMax];

                if (tmp.F > 0)
                {
                    result.Add(new ParamFBlockInfo(
                            x: tmp.BlockCenterCoords.X,
                            y: tmp.BlockCenterCoords.Y,
                            paramF: tmp.F));
                }
            }
        }

        return result;
    }

    public IReadOnlyList<CurrentEnergyBlockInfo> CreateCurrentEnergyBlockInfo(RegionalForecastBlocksInfo rfBlocksInfo)
    {
        var result = new List<CurrentEnergyBlockInfo>();

        for (int x = 0; x < rfBlocksInfo.BlocksInfo.GetLength(0); x++)
        {
            for (int y = 0; y < rfBlocksInfo.BlocksInfo.GetLength(1); y++)
            {
                var indexOfMax = 0;
                var max = rfBlocksInfo.BlocksInfo[x, y, indexOfMax].MaxCurrentEnergy;

                for (int z = 1; z < rfBlocksInfo.BlocksInfo.GetLength(2); z++)
                {
                    if (rfBlocksInfo.BlocksInfo[x, y, z].MaxCurrentEnergy > max)
                    {
                        max = rfBlocksInfo.BlocksInfo[x, y, z].MaxCurrentEnergy;
                        indexOfMax = z;
                    }
                }

                var tmp = rfBlocksInfo.BlocksInfo[x, y, indexOfMax];

                if (tmp.MaxCurrentEnergy >= 0)
                {
                    result.Add(new CurrentEnergyBlockInfo(
                            x: tmp.BlockCenterCoords.X,
                            y: tmp.BlockCenterCoords.Y,
                            maxCurrentEnergy: tmp.MaxCurrentEnergy));
                }
            }
        }

        return result;
    }

    public RfBlocksAssessment CalcRfAllBlocksAssessment(BlockInfo[,,] blocksInfo)
    {
        var rfFStatus = RfBlockStatus.Uncertain;
        var rfCurrEnergyStatus = RfBlockStatus.Uncertain;

        for (int z = 0; z < blocksInfo.GetLength(2); z++)
        {
            for (int y = 0; y < blocksInfo.GetLength(1); y++)
            {
                for (int x = 0; x < blocksInfo.GetLength(0); x++)
                {
                    if (blocksInfo[x, y, z].BlockAssessment.ParamFStatus > rfFStatus)
                    {
                        rfFStatus = blocksInfo[x, y, z].BlockAssessment.ParamFStatus;

                        if (rfFStatus == RfBlockStatus.Level4)
                            break;
                    }
                }

                if (rfFStatus == RfBlockStatus.Level4)
                    break;
            }

            if (rfFStatus == RfBlockStatus.Level4)
                break;
        }

        for (int z = 0; z < blocksInfo.GetLength(2); z++)
        {
            for (int y = 0; y < blocksInfo.GetLength(1); y++)
            {
                for (int x = 0; x < blocksInfo.GetLength(0); x++)
                {
                    if (blocksInfo[x, y, z].BlockAssessment.CurrentEnergyStatus > rfCurrEnergyStatus)
                    {
                        rfCurrEnergyStatus = blocksInfo[x, y, z].BlockAssessment.CurrentEnergyStatus;

                        if (rfCurrEnergyStatus == RfBlockStatus.Level4)
                            break;
                    }
                }

                if (rfCurrEnergyStatus == RfBlockStatus.Level4)
                    break;
            }

            if (rfCurrEnergyStatus == RfBlockStatus.Level4)
                break;
        }

        var generalStatus = (RfBlockStatus)Math.Max((int)rfFStatus, (int)rfCurrEnergyStatus);

        var result = new RfBlocksAssessment(generalStatus: generalStatus, paramfStatus: rfFStatus,
            currentEnergyStatus: rfCurrEnergyStatus);

        return result;
    }
    

    // Private
    private double CalcFBlock(
        DateTime estimateDt,
        int tRegDays,
        //int tMinImpactDays,
        double energyBackground,
        IReadOnlyList<(DateTime Dt, double ECurr)> currentEventsInBlock)
    {
        var countOfEvents = currentEventsInBlock.Count;
        //var eCurrs = CalcCurrentEnergyOfEvents(estimateDt, tMinImpactDays, eventsInBlock);

        var dBlock = CalcDBlock(estimateDt, tRegDays, energyBackground, currentEventsInBlock);

        var result = countOfEvents + dBlock;

        return result;
    }

    private double CalcDBlock(
        DateTime estimateDt,
        int tBlockRegDays,
        double energyBackground,
        IReadOnlyList<(DateTime Dt, double ECurr)> eCurrs)
    {
        var dBlock = 0.0;

        for (int i = 1; i < tBlockRegDays + 1; i++)
        {
            var dtWindow = estimateDt.AddDays(-1 * i);
            var eBlock = eCurrs.Where(x => x.Dt >= dtWindow && x.Dt <= estimateDt)
                .Select(x => x.ECurr)
                .Sum();

            dBlock += Math.Sqrt(eBlock / energyBackground);
        }

        return dBlock;
    }

    private IReadOnlyList<(DateTime Dt, double ECurr)> CalcCurrentEnergyOfEvents(
        DateTime estimateDt,
        int tMinImpactDays,
        IReadOnlyCollection<Event> eventsInBlock)
    {
        var result = new List<(DateTime Dt, double ECurr)>();

        foreach (var e in eventsInBlock)
        {
            var deltaD = CalcDeltaDays(estimateDt, e.Dt);
            var eCurr = CalcCurrentEnergyOfEvent(e.E, deltaD, tMinImpactDays);
            result.Add((e.Dt, eCurr));
        }

        return result;
    }

    private double CalcCurrentEnergyOfEvent(double eventEnergy, int deltaDays, int tMinImpactDays)
    {
        var result = Math.Exp(-1 * 0.1 * deltaDays) * (eventEnergy - deltaDays * eventEnergy / tMinImpactDays);

        return result;
    }

    private int CalcDeltaDays(DateTime estimateDt, DateTime eventDt)
    {
        if (estimateDt < eventDt)
            throw new ArgumentException($"Момент времени оценки {estimateDt}, " +
                $"должен быть >= момента времени события {eventDt}");

        var result = (estimateDt - eventDt).Days;

        return result;
    }

    private RfBlocksAssessment CalcStatuses(
        double f, 
        double energy, 
        RfSeismoActivityEstimationSettings estimationSettings)
    {
        var fStatus = RfBlockStatus.Uncertain;
        var eStatus = RfBlockStatus.Uncertain;

        if (f >= estimationSettings.FLevel3)
            fStatus = RfBlockStatus.Level4;
        else if (f >= estimationSettings.FLevel2)
            fStatus = RfBlockStatus.Level3;
        else if (f >= estimationSettings.FLevel1)
            fStatus = RfBlockStatus.Level2;
        else if (f >= estimationSettings.FBackgound)
            fStatus = RfBlockStatus.Level1;
        else
            fStatus = RfBlockStatus.LevelBackground;

        if(energy >= estimationSettings.ECurrentLevel3)
            eStatus = RfBlockStatus.Level4;
        else if(energy >= estimationSettings.ECurrentLevel2)
            eStatus = RfBlockStatus.Level3;
        else if(energy >= estimationSettings.ECurrentLevel1)
            eStatus = RfBlockStatus.Level2;
        else if(energy >= estimationSettings.ECurrentBackground)
            eStatus = RfBlockStatus.Level1;
        else
            eStatus = RfBlockStatus.LevelBackground;

        var generalStatus = (RfBlockStatus)Math.Max((int)fStatus, (int)eStatus);

        var result = new RfBlocksAssessment(generalStatus: generalStatus, paramfStatus: fStatus, 
            currentEnergyStatus: eStatus);

        return result;
    }
}
