using System.Runtime;
using UnityEngine;
using Verse;

namespace RIMSPR;

public class RimsprMod : Mod
{

    private readonly RimsprSettings _settings;

    public RimsprMod(ModContentPack content) : base(content)
    {
        _settings = GetSettings<RimsprSettings>();

    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Widgets.Label(inRect with { width = inRect.width / 4, height = 24 }, "RIMSPR_workMultiplier".Translate());
        _settings.workMultiplier = (int)Widgets.HorizontalSlider(inRect with { x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.workMultiplier, 10f, 1000f, true, $"{_settings.workMultiplier}", "10", "1000", 1f);
        TooltipHandler.TipRegion(inRect with { height = 24 }, "RIMSPR_workMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 30, width = inRect.width / 4, height = 24 }, "RIMSPR_foodMultiplier".Translate());
        _settings.foodMultiplier = Widgets.HorizontalSlider(inRect with { y = inRect.y + 30, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.foodMultiplier, 0.1f, 10f, true, $"{_settings.foodMultiplier}", "0.1", "10", 0.1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 30, height = 24 }, "RIMSPR_foodMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 60, width = inRect.width / 4, height = 24 }, "RIMSPR_chemMultiplier".Translate());
        _settings.chemMultiplier = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 60, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.chemMultiplier, 1f, 1000f, true, $"{_settings.chemMultiplier}", "1", "1000", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 60, height = 24 }, "RIMSPR_chemMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 90, width = inRect.width / 4, height = 24 }, "RIMSPR_neutroMultiplier".Translate());
        _settings.neutroMultiplier = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 90, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.neutroMultiplier, 1f, 1000f, true, $"{_settings.neutroMultiplier}", "1", "1000", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 90, height = 24 }, "RIMSPR_neutroMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 120, width = inRect.width / 4, height = 24 }, "RIMSPR_architeMultiplier".Translate());
        _settings.architeMultiplier = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 120, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.architeMultiplier, 1f, 10f, true, $"{_settings.architeMultiplier}", "1", "10", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 120, height = 24 }, "RIMSPR_architeMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 150, width = inRect.width / 4, height = 24 }, "RIMSPR_complexValueMultiplier".Translate());
        _settings.biostatCpxValueMultiplier = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 150, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.biostatCpxValueMultiplier, 1f, 10f, true, $"{_settings.biostatCpxValueMultiplier}", "1", "10", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 150, height = 24 }, "RIMSPR_complexValueMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 180, width = inRect.width / 4, height = 24 }, "RIMSPR_metabolicValueMultiplier".Translate());
        _settings.biostatMetValueMultiplier = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 180, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.biostatMetValueMultiplier, 1f, 10f, true, $"{_settings.biostatMetValueMultiplier}", "1", "10", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 180, height = 24 }, "RIMSPR_metabolicValueMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 210, width = inRect.width / 4, height = 24 }, "RIMSPR_architeValueMultiplier".Translate());
        _settings.biostatArchValueMultiplier = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 210, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.biostatArchValueMultiplier, 1f, 10f, true, $"{_settings.biostatArchValueMultiplier}", "1", "10", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 210, height = 24 }, "RIMSPR_architeValueMultiplierTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 240, width = inRect.width / 4, height = 24 }, "RIMSPR_workMin".Translate());
        _settings.workMin = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 240, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.workMin, 1f, 1000f, true, $"{_settings.workMin}", "1", "1000", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 240, height = 24 }, "RIMSPR_workMinTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 270, width = inRect.width / 4, height = 24 }, "RIMSPR_foodMin".Translate());
        _settings.foodMin = Widgets.HorizontalSlider(inRect with { y = inRect.y + 270, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.foodMin, 0.01f, 10f, true, $"{_settings.foodMin}", "0.01", "10", 0.01f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 270, height = 24 }, "RIMSPR_foodMinTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 300, width = inRect.width / 4, height = 24 }, "RIMSPR_chemMin".Translate());
        _settings.chemMin = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 300, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.chemMin, 1f, 1000f, true, $"{_settings.chemMin}", "1", "1000", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 300, height = 24 }, "RIMSPR_chemMinTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 330, width = inRect.width / 4, height = 24 }, "RIMSPR_neutroMin".Translate());
        _settings.neutroMin = (int)Widgets.HorizontalSlider(inRect with { y = inRect.y + 330, x = inRect.width / 4, width = (inRect.width / 4) * 3, height = 24 }, _settings.neutroMin, 0.0f, 100f, true, $"{_settings.neutroMin}", "0", "100", 1f);
        TooltipHandler.TipRegion(inRect with { y = inRect.y + 330, height = 24 }, "RIMSPR_neutroMinTooltip".Translate());

        Widgets.Label(inRect with { y = inRect.y + 390, width = inRect.width, height = 48 }, "RIMSPR_restart_note".Translate());

        base.DoSettingsWindowContents(inRect);
    }


    public override string SettingsCategory()
    {
        return "RIMSPRLab".Translate();
    }
}