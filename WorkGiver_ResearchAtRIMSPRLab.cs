using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using RIMSPR;
using RIMSPR;

namespace RIMSPR;

public class WorkGiver_ResearchAtRIMSPRLab : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(RIMSPR_DefOfs.RimsprLab);

    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
        for (int i = 0; i < allBuildingsColonist.Count; i++)
        {
            Building building = allBuildingsColonist[i];
            if (building.def == RIMSPR_DefOfs.RimsprLab)
            {
                CompPowerTrader comp = building.GetComp<CompPowerTrader>();
                if ((comp == null || comp.PowerOn) && building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t.Faction != pawn.Faction)
        {
            return false;
        }
        if (!(t is Building_RimsprLab lab))
        {
            return false;
        }
        if (lab.IsForbidden(pawn))
        {
            return false;
        }
        if (!pawn.CanReserve(lab, 1, -1, null, forced))
        {
            return false;
        }
        if (!lab.CanResearchNow())
        {
            return false;
        }
        if (lab.Map.designationManager.DesignationOn(lab, DesignationDefOf.Uninstall) != null)
        {
            return false;
        }
        if (lab.IsBurning())
        {
            return false;
        }
        return true;
    }
    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(RIMSPR_DefOfs.GenepackResearch, t, 1500, checkOverrideOnExpiry: true);
    }
}
