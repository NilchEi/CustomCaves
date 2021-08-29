using Verse;

namespace CustomCaves
{
    // Token: 0x02000006 RID: 6
    [StaticConstructorOnStartup]
    public class SettingsImplementer
    {
        // Token: 0x0600000F RID: 15 RVA: 0x00002750 File Offset: 0x00000950
        static SettingsImplementer()
        {
            GenStep_CustomCaveHives.allowCaveHiveReproduction = LoadedModManager.GetMod<CustomCavesSettings>().settings.repAllowCaveHiveReproduction;
            GenStep_CustomCaveHives.allowImpassableHives = LoadedModManager.GetMod<CustomCavesSettings>().settings.repAllowImpassableHives;
        }
    }
}