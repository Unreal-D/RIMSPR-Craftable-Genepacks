using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RIMSPR;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static System.Net.Mime.MediaTypeNames;
using Random = UnityEngine.Random;

namespace RIMSPR;

public enum RIMSPERLabState
{
    Inactive,
    WaitingForNutrients,
    WaitingForIngredients,
    Active
}

[StaticConstructorOnStartup]
public class Building_RimsprLab : Building, IThingHolder, IStoreSettingsParent
{
    [Unsaved] private CompPowerTrader? cachedPowerComp;
    [Unsaved] private CompRefuelable? cachedRefuelableComp;
    //[Unsaved] private Effecter? progressBar;
    private GeneDef? selectedGene;
    public bool PowerOn => PowerTraderComp.PowerOn;
    private CompPowerTrader PowerTraderComp => cachedPowerComp ??= this.TryGetComp<CompPowerTrader>();
    private CompRefuelable RefuelableComp => cachedRefuelableComp ??= this.TryGetComp<CompRefuelable>();
    public ThingOwner<Thing> innerContainer = new ThingOwner<Thing>();
    public bool StorageTabVisible => true;

    private float workGoal = -1;
    private float workProg = 0;
    private int lastUsedTick = -99999;
    private bool workStarted = false;
    private Dictionary<ThingDef, int> ingredientCount = new Dictionary<ThingDef, int>();
    private float nutrientGoal = -1f;

    public StorageSettings inputSettings;

    public override void PostMake()
    {
        base.PostMake();
        inputSettings = new StorageSettings(this);
        if (def.building.defaultStorageSettings != null)
        {
            inputSettings.CopyFrom(def.building.defaultStorageSettings);
        }
    }
    public override void PostPostMake()
    {
        if (!ModLister.CheckBiotech("gene extractor"))
            Destroy();
        else
            base.PostPostMake();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        //sustainerWorking = null;
        //if (progressBar != null)
        //{
        //    progressBar.Cleanup();
        //    progressBar = null;
        //}

        workGoal = -1f;
        workProg = 0f;
        lastUsedTick = -99999;
        ingredientCount.Clear();
        innerContainer.Clear();
        nutrientGoal = -1f;

        base.DeSpawn(mode);
    }

    public void ResearchWorkDone(Pawn researcher, Building lab)
    {
        float statValue = researcher.GetStatValue(StatDefOf.ResearchSpeed);
        statValue *= lab.GetStatValue(StatDefOf.ResearchSpeedFactor);
        statValue *= 0.00825f;
        workProg += statValue;
        lastUsedTick = Find.TickManager.TicksGame;

        //startup
        if(workGoal < 0 && selectedGene != null)
        {
            workGoal = RimsprUtility.CalcWorkCost(selectedGene);
        }

        if(workProg > workGoal)
        {
            Finish();
            workGoal = -1f;
            workProg = 0f;
            ingredientCount.Clear();
            innerContainer.Clear();
            nutrientGoal = -1f;
        }
    }

    public RIMSPERLabState State
    {
        get
        {
            if(PowerTraderComp != null && !PowerTraderComp.PowerOn)
            {
                return RIMSPERLabState.Inactive;
            }
            if (!AllRequiredNutrientsLoaded)
            {
                return RIMSPERLabState.WaitingForNutrients;
            }
            if (!AllRequiredIngredientsLoaded)
            {
                return RIMSPERLabState.WaitingForIngredients;
            }
            return RIMSPERLabState.Active;
        }
    }
    public bool CanResearchNow()
    {
        if (PowerTraderComp != null && !PowerTraderComp.PowerOn)
        {
            return false;
        }
        if (!AllRequiredNutrientsLoaded)
        {
            return false;
        }
        if (!AllRequiredIngredientsLoaded)
        {
            return false;
        }
        if (selectedGene == null)
        {
            return false;
        }
        return true;
    }

    public bool UsedLastTick()
    {
        return lastUsedTick >= Find.TickManager.TicksGame - 1;
    }


    //public override void Tick()
    //{
    //    Log.Message("[RIMSPR]: Tick");
    //    base.Tick();
    //    Finish();
    //}
    private void Cancel()
    {
        selectedGene = null;
        workGoal = -1f;
        workProg = 0f;
        ingredientCount.Clear();
        innerContainer.Clear();
        nutrientGoal = -1f;
    }

    //public void GetChildHolders(List<IThingHolder> outChildren)
    //{
    //    ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    //}

    //public ThingOwner GetDirectlyHeldThings()
    //{
    //    return innerContainer;
    //}

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var lab = this;
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;
        if (selectedGene == null)
        {
            var commandAction1 = new Command_Action
            {
                defaultLabel = "RIMSPRLab_SelectGene".Translate(),
                defaultDesc = "RIMSPRLab_SelectGeneDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/ViewGenes"),
                action = new Action(() => Find.WindowStack.Add(new Dialog_GeneResearchLibrary( (gene, ingredients, food) => 
                { 
                    selectedGene = gene;
                    ingredientCount = ingredients;
                    nutrientGoal = food;
                }))),
                activateSound = SoundDefOf.Designate_Cancel
            };
            yield return commandAction1;
        }
        else
        {
            var commandAction2 = new Command_Action
            {
                defaultLabel = "RIMSPRLab_CancelProject".Translate(),
                defaultDesc = "RIMSPRLab_CancelProjectDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                action = lab.Cancel,
                activateSound = SoundDefOf.Designate_Cancel
            };
            yield return commandAction2;
        }
        //if (selectedGene != null) Finish();
    }


    private void Finish()
    {
        Log.Message("[RIMSPR] Finish: "+ (selectedGene != null).ToString());
        if (selectedGene is null)
        {
            Cancel();
            return;
        }
        Log.Message("[RIMSPR] Finish: " + selectedGene.LabelCap);

        var genesToAdd = new List<GeneDef> { selectedGene };
        selectedGene = null;

        var genePack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);

        genePack.Initialize(genesToAdd);
        var intVec3 = def.hasInteractionCell ? InteractionCell : Position;
        GenPlace.TryPlaceThing(genePack, intVec3, Map, ThingPlaceMode.Near);
    }

    public override string GetInspectString()
    {
        //return base.GetInspectString();
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());

        string text;

        if (selectedGene != null)
        {
            if(workGoal < 0)
            {
                text = "RIMSPRLab_StartResearch".Translate() + selectedGene.LabelCap;
            }
            else
            {
                text = "RIMSPRLab_ResearchInProgress".Translate() + selectedGene.label + ": " + workProg.ToString("0.00") + " / " + workGoal.ToString("0");
            }
        }
        else
        {
            text = "RIMSPRLab_NoGeneSelected".Translate();
        }

        if (!text.NullOrEmpty())
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.AppendLine();
            }

            stringBuilder.Append(text);
        }
        if(selectedGene != null)
        {
            AppendNutrients(stringBuilder);
            AppendIngredientsList(stringBuilder);
        }

        return stringBuilder.ToString();

    }
    public void GetChildHolders(List<IThingHolder> outChildren)
    {
    }


    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }
    public bool AllRequiredIngredientsLoaded
    {
        get
        {
            if (!workStarted)
            {
                //for (int i = 0; i < def.building.subcoreScannerFixedIngredients.Count; i++)
                //{
                //    if (GetRequiredCountOf(def.building.subcoreScannerFixedIngredients[i].FixedIngredient) > 0)
                //    {
                //        return false;
                //    }
                //}
                foreach (KeyValuePair<ThingDef, int> ingredient in ingredientCount)
                {
                    if (GetRequiredCountOf(ingredient.Key) > 0)
                    {
                        return false;
                    }
                }

                return true;
            }
            return true;
        }
    }

    public bool AllRequiredNutrientsLoaded
    {
        get
        {
            if(GetRequiredNutrition() > 0f)
            {
                return false;
            }
            return true;
        }
    }

    public int GetRequiredCountOf(ThingDef thingDef)
    {
        //for (int i = 0; i < def.building.subcoreScannerFixedIngredients.Count; i++)
        //{
        //    if (def.building.subcoreScannerFixedIngredients[i].FixedIngredient == thingDef)
        //    {
        //        int num = innerContainer.TotalStackCountOfDef(def.building.subcoreScannerFixedIngredients[i].FixedIngredient);
        //        return (int)def.building.subcoreScannerFixedIngredients[i].GetBaseCount() - num;
        //    }
        //}
        foreach(KeyValuePair<ThingDef, int> ingredient in ingredientCount)
        {
            if (ingredient.Key == thingDef)
            {
                int num = innerContainer.TotalStackCountOfDef(ingredient.Key);
                return (int)ingredient.Value - num;
            }
        }

        return 0;
    }
    public bool CanAcceptIngredient(Thing thing)
    {
        return GetRequiredCountOf(thing.def) > 0;
    }

    public float GetRequiredNutrition()
    {
        float num = 0f;
        num = GetNutritionTotal();
        return nutrientGoal - num;
    }

    private float GetNutritionTotal()
    {
        float num = 0f;
        foreach (Thing thing in innerContainer)
        {
            if (thing is Corpse corpse)
            {
                num += FoodUtility.GetBodyPartNutrition(corpse, corpse.InnerPawn.RaceProps.body.corePart);
            }
            else if (thing.def.IsIngestible)
            {
                num += (thing.def.ingestible.CachedNutrition) * (float)thing.stackCount;
            }
        }
        return num;
    }

    public bool CanAcceptNutrition()
    {
        return GetRequiredNutrition() > 0;
    }

    public bool PassNutritionFilter(Thing thing)
    {
        return inputSettings.AllowedToAccept(thing);
    }


    private void AppendIngredientsList(StringBuilder sb)
    {
        //for (int i = 0; i < def.building.subcoreScannerFixedIngredients.Count; i++)
        //{
        //    IngredientCount ingredientCount = def.building.subcoreScannerFixedIngredients[i];
        //    int num = innerContainer.TotalStackCountOfDef(ingredientCount.FixedIngredient);
        //    int num2 = (int)ingredientCount.GetBaseCount();
        //    sb.AppendInNewLine($" - {ingredientCount.FixedIngredient.LabelCap} {num} / {num2}");
        //}

        foreach (KeyValuePair<ThingDef, int> ingredient in ingredientCount)
        {
            int num = innerContainer.TotalStackCountOfDef(ingredient.Key);
            int num2 = ingredient.Value;
            sb.AppendInNewLine($" - {ingredient.Key.LabelCap} {num} / {num2}");
        }
    }


    private void AppendNutrients(StringBuilder sb)
    {
        float num = GetNutritionTotal();
        float num2 = nutrientGoal;
        sb.AppendInNewLine($" - {"RIMSPRLab_Nutrition".Translate()} {num} / {num2}");
        //foreach (KeyValuePair<ThingDef, int> ingredient in ingredientCount)
        //{
        //    int num = innerContainer.TotalStackCountOfDef(ingredient.Key);
        //    int num2 = ingredient.Value;
        //    sb.AppendInNewLine($" - {ingredient.Key.LabelCap} {num} / {num2}");
        //}
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref selectedGene, "selectedGene");
        Scribe_Values.Look(ref workGoal, "workGoal");
        Scribe_Values.Look(ref workProg, "workProg");
        Scribe_Collections.Look(ref ingredientCount, "ingredientCount");
        Scribe_Values.Look(ref nutrientGoal, "nutrientGoal");
        Scribe_Deep.Look(ref innerContainer, "innerContainer");
        Scribe_Deep.Look(ref inputSettings, "inputSettings", this);
    }

    public override void Tick()
    {
        base.Tick();
        if(RefuelableComp != null)
        {
            while(GetRequiredCountOf(ThingDefOf.Chemfuel) > 0 && RefuelableComp.Fuel > 0f)
            {
                RefuelableComp.ConsumeFuel(1f);
                Thing thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel);
                this.innerContainer.TryAdd(thing);
            }

        }
    }

    public StorageSettings GetStoreSettings()
    {
        return inputSettings;
    }

    public StorageSettings GetParentStoreSettings()
    {
        return def.building.fixedStorageSettings;
    }

    public void Notify_SettingsChanged()
    {

    }
}