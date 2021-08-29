using System;
using Verse;

namespace CustomCaves
{
	// Token: 0x02000005 RID: 5
	internal class TerrainOverrideSquare : ITerrainOverride
	{
		// Token: 0x0600000E RID: 14 RVA: 0x0000220F File Offset: 0x0000040F
		public TerrainOverrideSquare(int lowX, int lowZ, int highX, int highZ)
		{
			this.Low = new IntVec3(lowX, 0, lowZ);
			this.High = new IntVec3(highX, 0, highZ);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002234 File Offset: 0x00000434
		public bool IsInside(IntVec3 i, int rand = 0)
		{
			return i.x >= this.Low.x + rand && i.x <= this.High.x + rand && i.z >= this.Low.z + rand && i.z <= this.High.z + rand;
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000010 RID: 16 RVA: 0x0000229A File Offset: 0x0000049A
		public int LowX
		{
			get
			{
				return this.Low.x;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000011 RID: 17 RVA: 0x000022A7 File Offset: 0x000004A7
		public int HighX
		{
			get
			{
				return this.High.x;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000022B4 File Offset: 0x000004B4
		public int LowZ
		{
			get
			{
				return this.Low.z;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000013 RID: 19 RVA: 0x000022C1 File Offset: 0x000004C1
		public int HighZ
		{
			get
			{
				return this.High.z;
			}
		}

		// Token: 0x04000003 RID: 3
		public readonly IntVec3 Low;

		// Token: 0x04000004 RID: 4
		public readonly IntVec3 High;
	}
}