using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace CustomCaves
{
	public class GenStep_CustomCavesTerrain : GenStep
	{
		public override int SeedPart
		{
			get
			{
				return 1921024373;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!Find.World.HasCaves(map.Tile))
			{
				return;
			}

			Perlin perlin = new Perlin(0.079999998211860657, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
			Perlin perlin2 = new Perlin(0.15999999642372131, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
			MapGenFloatGrid caves = MapGenerator.Caves;
			foreach (IntVec3 current in map.AllCells)
			{
				if (caves[current] > 0f)
				{
					TerrainDef terrain = current.GetTerrain(map);
					if (!terrain.IsRiver)
					{
						float num = (float)perlin.GetValue((double)current.x, 0.0, (double)current.z);
						float num2 = (float)perlin2.GetValue((double)current.x, 0.0, (double)current.z);
						if (num > 0.93f)
						{
							map.terrainGrid.SetTerrain(current, TerrainDefOf.WaterShallow);
						}
						else if (num2 > 0.55f)
						{
							map.terrainGrid.SetTerrain(current, TerrainDefOf.Gravel);
						}
					}
				}
				else if(map.TileInfo.hilliness == Hilliness.Mountainous)
				{
					TerrainDef terrain = current.GetTerrain(map);
					if(!terrain.IsRiver)
					{
						if (current.CloseToEdge(map, 30))
						{
							map.terrainGrid.SetTerrain(current, TerrainDefOf.FlagstoneSandstone);
							map.terrainGrid.SetUnderTerrain(current, TerrainDefOf.Gravel);
						}
						else
						{
							if (current.InHorDistOf(GenStep_CustomCaves.pondmiddle, GenStep_CustomCaves.pondradius - 9))
							{
								map.terrainGrid.SetTerrain(current, TerrainDefOf.WaterDeep);
							}
							else
							{
								if (current.InHorDistOf(GenStep_CustomCaves.pondmiddle, GenStep_CustomCaves.pondradius - 8) && Rand.Chance(0.5f))
								{
									map.terrainGrid.SetTerrain(current, TerrainDefOf.WaterDeep);
								}
								else if (current.InHorDistOf(GenStep_CustomCaves.pondmiddle, GenStep_CustomCaves.pondradius))
								{
									map.terrainGrid.SetTerrain(current, TerrainDefOf.WaterShallow);
								}
							}
						}
					}
				}
			}
		}
	}
}