using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using RIMSPR;

namespace RIMSPR;

public class JobDriver_GenepackResearch : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Uninstall);
        //this.FailOn(() => !job.targetA.Thing.TryGetComp<CompDeepDrill>().CanDrillNow());
        Building_RimsprLab lab = (Building_RimsprLab)job.targetA.Thing;
        this.FailOn(() => !lab.CanResearchNow());
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        Toil work = ToilMaker.MakeToil("MakeNewToils");
        work.tickAction = delegate
        {
            Pawn actor = work.actor;
            ((Building_RimsprLab)actor.CurJob.targetA.Thing).ResearchWorkDone(actor, (Building)actor.CurJob.targetA.Thing);
            actor.skills.Learn(SkillDefOf.Intellectual, 0.1f);
            actor.GainComfortFromCellIfPossible(chairsOnly: true);
        };
        work.defaultCompleteMode = ToilCompleteMode.Never;
        work.WithEffect(EffecterDefOf.Research, TargetIndex.A);
        work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        work.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        work.activeSkill = () => SkillDefOf.Intellectual;
        yield return work;
    }
}
