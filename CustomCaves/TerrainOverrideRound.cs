using System;
using Verse;

namespace CustomCaves
{
	// Token: 0x02000004 RID: 4
	internal class TerrainOverrideRound : ITerrainOverride
	{
		// Token: 0x06000008 RID: 8 RVA: 0x000020D3 File Offset: 0x000002D3
		public TerrainOverrideRound(IntVec3 center, int radius)
		{
			this.Center = center;
			this.Radius = radius;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000020EC File Offset: 0x000002EC
		public bool IsInside(IntVec3 i, int rand = 0)
		{
			int scatterzone = this.Radius - 4;
			if (i.x <= this.Center.x - scatterzone || i.x >= this.Center.x + scatterzone || i.z <= this.Center.z - scatterzone || i.z >= this.Center.z + scatterzone)
			{
				if (Rand.Chance(0.5f))
				{
					return false;
				}
			}

			double num = (double)(i.x - this.Center.x);
			int num2 = i.z - this.Center.z;
			return Math.Pow(num, 2.0) + Math.Pow((double)num2, 2.0) < Math.Pow((double)this.Radius, 2.0);
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000A RID: 10 RVA: 0x000021BF File Offset: 0x000003BF
		public int LowX
		{
			get
			{
				return this.Center.x - this.Radius;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000B RID: 11 RVA: 0x000021D3 File Offset: 0x000003D3
		public int HighX
		{
			get
			{
				return this.Center.x + this.Radius;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600000C RID: 12 RVA: 0x000021E7 File Offset: 0x000003E7
		public int LowZ
		{
			get
			{
				return this.Center.z - this.Radius;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600000D RID: 13 RVA: 0x000021FB File Offset: 0x000003FB
		public int HighZ
		{
			get
			{
				return this.Center.z + this.Radius;
			}
		}

		// Token: 0x04000001 RID: 1
		public readonly IntVec3 Center;

		// Token: 0x04000002 RID: 2
		public readonly int Radius;
	}
}