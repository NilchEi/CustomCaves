using UnityEngine;
using Verse;

namespace CustomCaves
{
    // Token: 0x02000005 RID: 5
    public class CustomCavesSettings : Mod
    {
        // Token: 0x0600000C RID: 12 RVA: 0x0000255C File Offset: 0x0000075C
        public CustomCavesSettings(ModContentPack content) : base(content)
        {
            this.settings = base.GetSettings<CustomCavesModSettings>();
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002574 File Offset: 0x00000774
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            Text.Font = GameFont.Tiny;
            listing_Standard.Label("CustomCavesRestartWarning".Translate(), -1f, null);
            listing_Standard.Gap(12f);
            Text.Font = GameFont.Small;
            listing_Standard.CheckboxLabeled("RepAllowImpassableHivesExp".Translate(), ref this.settings.repAllowImpassableHives, null);
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowCaveHiveReproductionExp".Translate(), ref this.settings.repAllowCaveHiveReproduction, null);
            listing_Standard.End();
            base.DoSettingsWindowContents(inRect);
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002738 File Offset: 0x00000938
        public override string SettingsCategory()
        {
            return "Mountainous Cave System";
        }

        // Token: 0x04000007 RID: 7
        public readonly CustomCavesModSettings settings;
    }
}
