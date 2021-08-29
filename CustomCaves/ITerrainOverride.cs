using Verse;

namespace CustomCaves
{
	// Token: 0x02000003 RID: 3
	internal interface ITerrainOverride
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3
		int HighZ { get; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000004 RID: 4
		int LowZ { get; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000005 RID: 5
		int HighX { get; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000006 RID: 6
		int LowX { get; }

		// Token: 0x06000007 RID: 7
		bool IsInside(IntVec3 i, int rand = 0);
	}
}