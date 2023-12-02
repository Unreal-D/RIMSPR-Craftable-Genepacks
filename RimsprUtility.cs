using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace RIMSPR;

[StaticConstructorOnStartup]
public static class RimsprUtility
{
    private static RimsprSettings? _settings;
    private static RimsprSettings Settings => _settings ??= LoadedModManager.GetMod<RimsprMod>().GetSettings<RimsprSettings>();

    private static int workMultiplier = Settings.workMultiplier;
    private static float foodMultiplier = Settings.foodMultiplier;
    private static int chemMultiplier = Settings.chemMultiplier;
    private static int neutroMultiplier = Settings.neutroMultiplier;
    private static int architeMultiplier = Settings.architeMultiplier;

    private static int biostatCpxValueMultiplier = Settings.biostatCpxValueMultiplier;
    private static int biostatMetValueMultiplier = Settings.biostatMetValueMultiplier;
    private static int biostatArchValueMultiplier = Settings.biostatArchValueMultiplier;

    private static int workMin = Settings.workMin;
    private static float foodMin = Settings.foodMin;
    private static int chemMin = Settings.chemMin;
    private static int neutroMin = Settings.neutroMin;


    public static float GeneValue(GeneDef Gene)
    {
        int value = (Gene.biostatCpx) * biostatCpxValueMultiplier + Math.Abs(Gene.biostatMet) * biostatMetValueMultiplier + (Gene.biostatArc * biostatArchValueMultiplier);
        if(value > 0)
        {
            return (float)value;
        }
        else if (value == 0)
        {
            return (float)Math.Sqrt(0.5); //Should result in 0.5 effective value after squaring for work and food cost
        }
        else
        {
            return -1f / ((float)value); //Convert negatives into inverses
        }
    }

    public static int CalcWorkCost(GeneDef Gene)
    {
        float value = GeneValue(Gene);
        float effectiveValue = Math.Max((value * value) * workMultiplier, (float)workMin);
        
        return (int)effectiveValue;
    }
    public static float CalcFoodCost(GeneDef Gene)
    {
        float value = GeneValue(Gene);
        float effectiveValue = Math.Max((value * value) * foodMultiplier, foodMin);

        return effectiveValue;
    }

    public static int CalcChemCost(GeneDef Gene)
    {
        float initValue = (float)Gene.biostatCpx;
        if (initValue == 0f)
        {
            initValue = 0.75f;
        }
        else if (initValue < 0f)
        {
            initValue = -1f / initValue;
        }
        int finalValue = Math.Max((int)(initValue * (float)chemMultiplier), chemMin);
        return finalValue;
    }

    public static int CalcNeutroCost(GeneDef Gene)
    {
        int initValue = Math.Abs(Gene.biostatMet) * neutroMultiplier;
        int finalValue = Math.Max(initValue, neutroMin);
        return finalValue;
    }

    public static int CalcArchiteCost(GeneDef Gene)
    {
        return Gene.biostatArc * architeMultiplier;
    }
}
