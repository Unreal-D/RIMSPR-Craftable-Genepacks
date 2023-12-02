using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RIMSPR
{
    public class RimsprSettings : ModSettings
    {
        public int workMultiplier = 100;
        public float foodMultiplier = 1f;
        public int chemMultiplier = 100;
        public int neutroMultiplier = 20;
        public int architeMultiplier = 3;

        public int biostatCpxValueMultiplier = 1;
        public int biostatMetValueMultiplier = 1;
        public int biostatArchValueMultiplier = 3;

        public int workMin = 10;
        public float foodMin = 0.01f;
        public int chemMin = 10;
        public int neutroMin = 1;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref workMultiplier, nameof(workMultiplier), 100);
            Scribe_Values.Look(ref foodMultiplier, nameof(foodMultiplier), 1f);
            Scribe_Values.Look(ref chemMultiplier, nameof(chemMultiplier), 100);
            Scribe_Values.Look(ref neutroMultiplier, nameof(neutroMultiplier), 20);
            Scribe_Values.Look(ref architeMultiplier, nameof(architeMultiplier), 3);
            Scribe_Values.Look(ref biostatCpxValueMultiplier, nameof(biostatCpxValueMultiplier), 1);
            Scribe_Values.Look(ref biostatMetValueMultiplier, nameof(biostatMetValueMultiplier), 1);
            Scribe_Values.Look(ref biostatArchValueMultiplier, nameof(biostatArchValueMultiplier), 3);
            Scribe_Values.Look(ref workMin, nameof(workMin), 10);
            Scribe_Values.Look(ref foodMin, nameof(foodMin), 0.01f);
            Scribe_Values.Look(ref chemMin, nameof(chemMin), 10);
            Scribe_Values.Look(ref neutroMin, nameof(neutroMin), 1);

            base.ExposeData();
        }
    }
}
