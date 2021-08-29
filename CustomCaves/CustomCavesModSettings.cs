using Verse;

namespace CustomCaves
{
    // Token: 0x02000004 RID: 4
    public class CustomCavesModSettings : ModSettings
    {
        // Token: 0x0600000A RID: 10 RVA: 0x0000249C File Offset: 0x0000069C
        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.repAllowImpassableHives, "repAllowImpassableHives", false, false);
            Scribe_Values.Look<bool>(ref this.repAllowCaveHiveReproduction, "repAllowCaveHiveReproduction", false, false);
            base.ExposeData();
        }

        // Token: 0x04000006 RID: 6
        public bool repAllowImpassableHives = false;
        public bool repAllowCaveHiveReproduction = false;
    }
}