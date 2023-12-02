using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

namespace RIMSPR
{
    public class Dialog_GeneResearchLibrary : Window
    {

        private static readonly List<GeneDef> geneDefs = new List<GeneDef>();
        private static readonly Dictionary<GeneDef, bool> geneShown = new Dictionary<GeneDef, bool>();
        private static readonly Dictionary<GeneCategoryDef, List<GeneDef>> categoricalGeneDefs = new Dictionary<GeneCategoryDef, List<GeneDef>>();
        private static readonly List<GeneCategoryDef> geneCategories = new List<GeneCategoryDef>();
        private static readonly CachedTexture GeneBackground_Archite = new CachedTexture("UI/Icons/Genes/GeneBackground_ArchiteGene");
        private static readonly CachedTexture GeneBackground_Xenogene = new CachedTexture("UI/Icons/Genes/GeneBackground_Xenogene");
        public static readonly CachedTexture ChemTex = new CachedTexture("Things/Item/Resource/Chemfuel");
        public static readonly CachedTexture NeutroTex = new CachedTexture("Things/Item/Resource/Neutroamine/Neutroamine_c");
        public static readonly CachedTexture ArchiteTex = new CachedTexture("Things/Item/Resource/ArchiteCapsule");
        public static readonly CachedTexture FoodTex = new CachedTexture("Things/Item/Meal/NutrientPaste/NutrientPaste_c");
        public static readonly CachedTexture WorkTex = new CachedTexture("UI/Buttons/AutoRebuild");
        private readonly Action<GeneDef, Dictionary<ThingDef, int>, float>? acceptAction;
        private readonly Action? cancelAction;
        private static float xenogenesHeight;
        private static float endogenesHeight;
        bool debugHasLogged = false;
        public static readonly Vector2 GeneSize = new Vector2(87f, 68f);
        private Vector2 scrollPosition;
        private static float scrollHeight;
        public override Vector2 InitialSize => new Vector2(736f, 700f);

        private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();
        private string searchValue;
        private bool searchEmpty = true;

        public Dialog_GeneResearchLibrary(Action<GeneDef, Dictionary<ThingDef, int>, float> acceptAction = null, Action cancelAction = null)
        {
            this.acceptAction = acceptAction;
            this.cancelAction = cancelAction;
        }
        public GeneDef? SelectedGene { get; private set; }

        public override void DoWindowContents(Rect inRect)
        {
            inRect.yMax -= CloseButSize.y;
            var rect = inRect;
            rect.xMin += 34f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, "RIMSPRLab_GeneMenu".Translate());
            Text.Font = GameFont.Small;
            Rect searchRect = new Rect(inRect.width - 300f - 20f, 11f, 300f, 24f);
            quickSearchWidget.OnGUI(searchRect, UpdateSearchResults);
            inRect.yMin += 34f;
            var zero = Vector2.zero;
            DrawGenesInfo(inRect, InitialSize.y, ref zero, ref scrollPosition);
            float sep = (inRect.width - (CloseButSize.x) * 5) / 4f;
            if (Widgets.ButtonText(
                    new Rect(inRect.xMin, inRect.yMax, CloseButSize.x,
                        CloseButSize.y), "RIMSPRLab_CancelButton".Translate()))
            {
                Close();
            }

            if(SelectedGene != null)
            {
                //Widgets.Label(new Rect(inRect.xMin + CloseButSize.x + 6f, inRect.yMax, inRect.width - CloseButSize.x * 2f - 18f, CloseButSize.y),
                //    "Cost to produce:");
                Rect workRect1 = new Rect(inRect.xMin + CloseButSize.x + 6f, inRect.yMax, 40f, 40f);
                GUI.DrawTexture(workRect1, WorkTex.Texture);
                Rect workRect2 = new Rect(workRect1.xMax + 6f, inRect.yMax, 40f, 90f);
                Widgets.Label(workRect2, RimsprUtility.CalcWorkCost(SelectedGene).ToString());
                Rect foodRect1 = new Rect(workRect2.xMax + 6f, inRect.yMax, 40f, 40f);
                GUI.DrawTexture(foodRect1, FoodTex.Texture);
                Rect foodRect2 = new Rect(foodRect1.xMax + 6f, inRect.yMax, 40f, 90f);
                Widgets.Label(foodRect2, RimsprUtility.CalcFoodCost(SelectedGene).ToString());
                Rect chemRect1 = new Rect(foodRect2.xMax + 6f, inRect.yMax, 40f, 40f);
                GUI.DrawTexture(chemRect1, ChemTex.Texture);
                Rect chemRect2 = new Rect(chemRect1.xMax + 6f, inRect.yMax, 40f, 90f);
                Widgets.Label(chemRect2, RimsprUtility.CalcChemCost(SelectedGene).ToString());
                Rect neutroRect1 = new Rect(chemRect2.xMax + 6f, inRect.yMax, 40f, 40f);
                GUI.DrawTexture(neutroRect1, NeutroTex.Texture);
                Rect neutroRect2 = new Rect(neutroRect1.xMax + 6f, inRect.yMax, 40f, 90f);
                Widgets.Label(neutroRect2, RimsprUtility.CalcNeutroCost(SelectedGene).ToString());
                Rect architeRect1 = new Rect(neutroRect2.xMax + 6f, inRect.yMax, 40f, 40f);
                GUI.DrawTexture(architeRect1, ArchiteTex.Texture);
                Rect architeRect2 = new Rect(architeRect1.xMax + 6f, inRect.yMax, 40f, 90f);
                Widgets.Label(architeRect2, RimsprUtility.CalcArchiteCost(SelectedGene).ToString());


                //GUI.DrawTexture()
            }

            if (Widgets.ButtonText(
                new Rect(inRect.xMax - CloseButSize.x - 6f, inRect.yMax, CloseButSize.x,
                    CloseButSize.y), "RIMSPRLab_SelectButton".Translate(), active: SelectedGene != null))
            {
                Dictionary<ThingDef, int> ingredients = new Dictionary<ThingDef, int>();
                float food = 0f;
                if(SelectedGene != null)
                {
                    ingredients.Add(ThingDefOf.Chemfuel, RimsprUtility.CalcChemCost(SelectedGene));
                    ingredients.Add(RIMSPR_DefOfs.Neutroamine, RimsprUtility.CalcNeutroCost(SelectedGene));
                    ingredients.Add(ThingDefOf.ArchiteCapsule, RimsprUtility.CalcArchiteCost(SelectedGene));
                    food = RimsprUtility.CalcFoodCost(SelectedGene);
                }
                acceptAction?.Invoke(SelectedGene!, ingredients!, food!);
                Close();
            }

        }

        private void DrawGenesInfo(
            Rect rect,
            float initialHeight,
            ref Vector2 size,
            ref Vector2 scrollPosition)
        {
            var rect1 = rect;
            var position = rect1.ContractedBy(10f);
            GUI.BeginGroup(position);
            var height = 0;
            var rect2 = new Rect(0.0f, 0.0f, position.width, (float)(position.height - (double)height - 12.0));
            DrawGeneSections(rect2, ref scrollPosition);
            var rect3 = new Rect(0.0f, rect2.yMax + 6f, (float)(position.width - 140.0 - 4.0), height);
            rect3.yMax = (float)(rect2.yMax + (double)height + 6.0);
            rect3.width = position.width;

            if (Event.current.type == EventType.Layout)
            {
                var a = (float)(endogenesHeight + (double)xenogenesHeight + height + 12.0 + 70.0);
                size.y = a <= (double)initialHeight
                    ? initialHeight
                    : Mathf.Min(a, (float)(UI.screenHeight - 35 - 165.0 - 30.0));
                xenogenesHeight = 0.0f;
                endogenesHeight = 0.0f;
            }

            GUI.EndGroup();
        }

        private void DrawGeneSections(
            Rect rect,
            ref Vector2 scrollPosition)
        {
            RecacheGenes();
            GUI.BeginGroup(rect);
            var viewRect = new Rect(0.0f, 0.0f, rect.width - 16f, scrollHeight);
            var curY = 0.0f;
            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);
            var containingRect = viewRect;
            containingRect.y = scrollPosition.y;
            containingRect.height = rect.height;

            DrawSection(rect, false, ref curY, ref xenogenesHeight,
                (i, r) => DrawGeneDef(geneDefs[i], r, GeneType.Endogene, null), containingRect);

            if (Event.current.type == EventType.Layout)
                scrollHeight = curY;
            Widgets.EndScrollView();
            GUI.EndGroup();
        }
        private void RecacheGenes()
        {
            geneDefs.Clear();
            geneShown.Clear();
            geneCategories.Clear();
            categoricalGeneDefs.Clear();

            geneDefs.AddRange(GeneUtility.GenesInOrder);
            geneDefs.SortGeneDefs();

            geneCategories.AddRange(DefDatabase<GeneCategoryDef>.AllDefs);
            geneCategories.SortBy(x => 0 - x.displayPriorityInXenotype);

            foreach (GeneCategoryDef geneCategory in geneCategories)
            {
                categoricalGeneDefs.Add(geneCategory, new List<GeneDef>());
            }

            foreach (GeneDef g in geneDefs)
            {
                if ((!searchEmpty && !g.label.ToLower().Contains(searchValue.ToLower())) || (
                    g.biostatArc > 0 && !RIMSPR_DefOfs.RIMSPR_researchArchogeneticEngineering.IsFinished)
                    )
                {
                    geneShown.Add(g, false);
                }
                else
                {
                    geneShown.Add(g, true);
                }

                categoricalGeneDefs[g.displayCategory].Add(g);
            }
        }

        private void DrawSection(
            Rect rect,
            bool xeno,
            ref float curY,
            ref float sectionHeight,
            Action<int, Rect> drawer,
            Rect containingRect)
        {
            var num1 = curY;
            var rect1 = new Rect(rect.x, curY, rect.width, sectionHeight);
            int catCount = geneCategories.Count();

            Widgets.DrawMenuSection(rect1);
            var num2 = (float)((rect.width - 12.0 - 630.0 - 36.0) / 2.0);
            curY += num2;
            bool drawn = true;

            for (var catIndex = 0; catIndex < catCount; catIndex++)
            {
                int genCount = categoricalGeneDefs[geneCategories[catIndex]].Count();
                Widgets.Label(10f, ref curY, rect.width, geneCategories[catIndex].label.CapitalizeFirst());

                bool firstValidDrawn = false;
                var num3 = 0;
                var num4 = 0;

                for (int genIndex = 0; genIndex < genCount; genIndex++)
                {
                    if (geneShown[categoricalGeneDefs[geneCategories[catIndex]][genIndex]]){
                        if (drawn)
                        {
                            if(num4 >= 6)
                            {
                                num4 = 0;
                                ++num3;
                            }
                            else if (firstValidDrawn)
                            {
                                ++num4;
                            }
                            else
                            {
                                firstValidDrawn = true;
                            }
                        }


                        var other = new Rect((float)(num2 + num4 * 90.0 + num4 * 6.0), (float)(curY + num3 * 90.0 + num3 * 6.0), 90f, 90f);
                        if (containingRect.Overlaps(other)) drawn = DrawGeneDef(categoricalGeneDefs[geneCategories[catIndex]][genIndex], other, GeneType.Endogene, null);
                    }
                }
                if (firstValidDrawn)
                    curY += (float)((num3 + 1) * 90.0 + num3 * 6.0) + num2;
            }

            if (Event.current.type != EventType.Layout)
                return;
            sectionHeight = curY - num1;
        }

        public bool DrawGeneDef(
            GeneDef gene,
            Rect geneRect,
            GeneType geneType,
            string extraTooltip)
        {
            bool res = DrawGeneBasics(gene, geneRect, geneType);
            if (res && !Mouse.IsOver(geneRect))
                return res;
            TooltipHandler.TipRegion(geneRect, (Func<string>)(() =>
            {
                var str = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.DescriptionFull;
                if (!extraTooltip.NullOrEmpty())
                    str = str + "\n\n" + extraTooltip.Colorize(ColorLibrary.RedReadable);
                return str;
            }), 795135468);
            return res;
        }

        private bool DrawGeneBasics(
            GeneDef gene,
            Rect geneRect,
            GeneType geneType)
        {

            //Color c = SettingsRef.red;
            //if (SettingsRef.ignoredGenes.Contains(gene))
            //{
            //    if (PatchGeneLibrary.hideState > 2) { return false; }
            //    c = SettingsRef.gray;
            //}
            //else if (geneShown.ContainsKey(gene))
            //{
            //    if (geneShown[gene] == 1)
            //    {
            //        if (PatchGeneLibrary.hideState % 3 == 2) { return false; }
            //        c = SettingsRef.yellow;
            //    }
            //    else if (geneShown[gene] == 2)
            //    {
            //        if (PatchGeneLibrary.hideState % 3 != 0) { return false; }
            //        c = SettingsRef.green;
            //    }
            //}
            GUI.BeginGroup(geneRect);
            var rect1 = geneRect.AtZero();
            //Widgets.DrawBoxSolid(rect1, c);

            GUI.color = new Color(1f, 1f, 1f, 0.05f);
            Widgets.DrawBox(rect1);
            GUI.color = Color.white;

            var num = rect1.width - Text.LineHeight;
            var rect2 = new Rect((float)(geneRect.width / 2.0 - num / 2.0), 0.0f, num, num);
            var iconColor = gene.IconColor;

            if (gene.biostatArc != 0)
                GUI.DrawTexture(rect2, GeneBackground_Archite.Texture);
            else
                GUI.DrawTexture(rect2, GeneBackground_Xenogene.Texture);

            Widgets.DefIcon(rect2, gene, scale: 0.9f, color: iconColor);

            Text.Font = GameFont.Tiny;
            var height = Text.CalcHeight((string)gene.LabelCap, rect1.width);
            var rect3 = new Rect(0.0f, rect1.yMax - height, rect1.width, height);
            GUI.DrawTexture(new Rect(rect3.x, rect3.yMax - height, rect3.width, height), TexUI.GrayTextBG);
            Text.Anchor = TextAnchor.LowerCenter;
            if (height < (Text.LineHeight - 2.0) * 2.0)
                rect3.y -= 3f;
            Widgets.Label(rect3, gene.LabelCap);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonInvisible(rect1))
                SelectedGene = gene;
            if (string.Equals(gene.defName, SelectedGene?.defName))
                Widgets.DrawHighlight(rect1);

            GUI.EndGroup();
            return true;
        }

        private void UpdateSearchResults()
        {
            searchValue = quickSearchWidget.filter.Text;
            if(searchValue == null)
            {
                searchEmpty = true;
            }
            else
            {
                searchEmpty = false;
            }
        }
    }
}
