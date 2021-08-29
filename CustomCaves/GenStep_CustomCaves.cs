using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CustomCaves
{
    public class GenStep_CustomCaves : GenStep
    {
        private static HashSet<IntVec3> tmpGroupSet = new HashSet<IntVec3>();
        private static readonly FloatRange BranchedTunnelWidthOffset = new FloatRange(0.2f, 0.4f);
        private static readonly FloatRange BranchedTunnelWidthOffsetImpassable = new FloatRange(0f, 0.2f);
        private static readonly SimpleCurve TunnelsWidthPerRockCount = new SimpleCurve()
    {
      {
        new CurvePoint(100f, 2f),
        true
      },
      {
        new CurvePoint(300f, 4f),
        true
      },
      {
        new CurvePoint(3000f, 5.5f),
        true
      }
    };

        private static List<IntVec3> tmpCells = new List<IntVec3>();
        private static HashSet<IntVec3> groupSet = new HashSet<IntVec3>();
        private static HashSet<IntVec3> groupVisited = new HashSet<IntVec3>();
        private static List<IntVec3> subGroup = new List<IntVec3>();
        private ModuleBase directionNoise;

        public static IntVec3 pondmiddle = new IntVec3();
        public static float pondradius;

        public override int SeedPart
        {
            get
            {
                return 647814558;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {          
            if (!Find.World.HasCaves(map.Tile))
                return;

            // Fill with earth
            if (map.TileInfo.hilliness == Hilliness.Mountainous)
            {
                int radius = (int)(((float)map.Size.x + (float)map.Size.z) * 0.25f);
                int middleWallSmoothness = 10;
                System.Random random;
                random = new System.Random((Find.World.info.name + map.Tile.ToString()).GetHashCode());
                ITerrainOverride terrainOverride = null;
                terrainOverride = GenStep_CustomCaves.GenerateMiddleArea(random, map);
                ITerrainOverride terrainOverride2 = null;
                terrainOverride2 = GenStep_CustomCaves.GenerateEdgeAreaLeft(random, map);
                ITerrainOverride terrainOverride3 = null;
                terrainOverride3 = GenStep_CustomCaves.GenerateEdgeAreaRight(random, map);
                MapGenFloatGrid fertility = MapGenerator.Fertility;
                MapGenFloatGrid elevation1 = MapGenerator.Elevation;
                foreach (IntVec3 intVec in map.AllCells)
                {
                    float value;
                    value = float.MaxValue;

                    int rand = (middleWallSmoothness == 0) ? 0 : random.Next(middleWallSmoothness);
                    if (terrainOverride.IsInside(intVec, rand))
                    {
                        value = 0f;
                    }
                    int rand2 = (middleWallSmoothness == 0) ? 0 : random.Next(middleWallSmoothness);
                    if (terrainOverride2.IsInside(intVec, rand2))
                    {
                        value = 0f;
                    }
                    int rand3 = (middleWallSmoothness == 0) ? 0 : random.Next(middleWallSmoothness);
                    if (terrainOverride3.IsInside(intVec, rand3))
                    {
                        value = 0f;
                    }
                    elevation1[intVec] = value;
                }
            }
            
            // Build caves
            this.directionNoise = (ModuleBase)new Perlin(0.00205000001005828, 2.0, 0.5, 4, Rand.Int, Verse.Noise.QualityMode.Medium);
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            BoolGrid visited = new BoolGrid(map);
            List<IntVec3> group = new List<IntVec3>();
            foreach (IntVec3 allCell in map.AllCells)
            {
                if (!visited[allCell] && this.IsRock(allCell, elevation, map))
                {
                    group.Clear();
                    map.floodFiller.FloodFill(allCell, (Predicate<IntVec3>)(x => this.IsRock(x, elevation, map)), (Action<IntVec3>)(x =>
                    {
                        visited[x] = true;
                        group.Add(x);
                    }), int.MaxValue, false, (IEnumerable<IntVec3>)null);
                    this.Trim(group, map);
                    this.RemoveSmallDisconnectedSubGroups(group, map);
                    if (group.Count >= 300)
                    {
                        this.DoOpenTunnels(group, map);
                        this.DoClosedTunnels(group, map);
                    }
                }
            }
        }

        private void Trim(List<IntVec3> group, Map map)
        {
            GenMorphology.Open(group, 6, map);
        }

        private bool IsRock(IntVec3 c, MapGenFloatGrid elevation, Map map)
        {
            return c.InBounds(map) && (double)elevation[c] > 0.699999988079071;
        }

        private void DoOpenTunnels(List<IntVec3> group, Map map)
        {
            int max1 = Mathf.Min(GenMath.RoundRandom((float)((double)group.Count * (double)Rand.Range(0.9f, 1.1f) * 5.80000019073486 / 10000.0)), 3);
            if (max1 > 0)
                max1 = Rand.RangeInclusive(1, max1);
            float max2 = GenStep_CustomCaves.TunnelsWidthPerRockCount.Evaluate((float)group.Count);

            if (map.TileInfo.hilliness == Hilliness.Mountainous && group.Count >= 20000) // Skip open tunnel count limit for impassable
            {
                max1 = Mathf.Max((int)((double)group.Count * (double)Rand.Range(1.0f, 1.1f) * 5.80000019073486 / 10000.0)/5, 5);
                max2 = 9.5f; // Return large width for impassable
            }

            for (int index1 = 0; index1 < max1; ++index1)
            {
                IntVec3 start = IntVec3.Invalid;
                float num1 = -1f;
                float dir = -1f;
                float num2 = -1f;
                for (int index2 = 0; index2 < 10; ++index2)
                {
                    IntVec3 edgeCellForTunnel = this.FindRandomEdgeCellForTunnel(group, map);
                    float distToCave = this.GetDistToCave(edgeCellForTunnel, group, map, 40f, false);
                    float dist;
                    float bestInitialDir = this.FindBestInitialDir(edgeCellForTunnel, group, out dist);
                    if (!start.IsValid || (double)distToCave > (double)num1 || (double)distToCave == (double)num1 && (double)dist > (double)num2)
                    {
                        start = edgeCellForTunnel;
                        num1 = distToCave;
                        dir = bestInitialDir;
                        num2 = dist;
                    }
                }
                float width = Rand.Range(max2 * 0.8f, max2);
                this.Dig(start, dir, width, group, map, false, (HashSet<IntVec3>)null);
            }
        }

        private void DoClosedTunnels(List<IntVec3> group, Map map)
        {
            int max1 = Mathf.Min(GenMath.RoundRandom((float)((double)group.Count * (double)Rand.Range(0.9f, 1.1f) * 2.5 / 10000.0)), 1);
            if (max1 > 0)
                max1 = Rand.RangeInclusive(0, max1);
            float max2 = GenStep_CustomCaves.TunnelsWidthPerRockCount.Evaluate((float)group.Count);
            for (int index1 = 0; index1 < max1; ++index1)
            {
                IntVec3 start = IntVec3.Invalid;
                float num = -1f;
                for (int index2 = 0; index2 < 7; ++index2)
                {
                    IntVec3 cell = group.RandomElement<IntVec3>();
                    float distToCave = this.GetDistToCave(cell, group, map, 30f, true);
                    if (!start.IsValid || (double)distToCave > (double)num)
                    {
                        start = cell;
                        num = distToCave;
                    }
                }
                float width = Rand.Range(max2 * 0.8f, max2);
                this.Dig(start, Rand.Range(0.0f, 360f), width, group, map, true, (HashSet<IntVec3>)null);
            }
        }

        private IntVec3 FindRandomEdgeCellForTunnel(List<IntVec3> group, Map map)
        {
            MapGenFloatGrid caves = MapGenerator.Caves;
            IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
            GenStep_CustomCaves.tmpCells.Clear();
            GenStep_CustomCaves.tmpGroupSet.Clear();
            GenStep_CustomCaves.tmpGroupSet.AddRange<IntVec3>(group);
            for (int index1 = 0; index1 < group.Count; ++index1)
            {
                if (group[index1].DistanceToEdge(map) >= 3 && (double)caves[group[index1]] <= 0.0)
                {
                    for (int index2 = 0; index2 < 4; ++index2)
                    {
                        IntVec3 intVec3 = group[index1] + cardinalDirections[index2];
                        if (!GenStep_CustomCaves.tmpGroupSet.Contains(intVec3))
                        {
                            GenStep_CustomCaves.tmpCells.Add(group[index1]);
                            break;
                        }
                    }
                }
            }
            if (GenStep_CustomCaves.tmpCells.Any<IntVec3>())
                return GenStep_CustomCaves.tmpCells.RandomElement<IntVec3>();
            Log.Warning("Could not find any valid edge cell.", false);
            return group.RandomElement<IntVec3>();
        }

        private float FindBestInitialDir(IntVec3 start, List<IntVec3> group, out float dist)
        {
            float distToNonRock1 = (float)this.GetDistToNonRock(start, group, IntVec3.East, 40);
            float distToNonRock2 = (float)this.GetDistToNonRock(start, group, IntVec3.West, 40);
            float distToNonRock3 = (float)this.GetDistToNonRock(start, group, IntVec3.South, 40);
            float distToNonRock4 = (float)this.GetDistToNonRock(start, group, IntVec3.North, 40);
            float distToNonRock5 = (float)this.GetDistToNonRock(start, group, IntVec3.NorthWest, 40);
            float distToNonRock6 = (float)this.GetDistToNonRock(start, group, IntVec3.NorthEast, 40);
            float distToNonRock7 = (float)this.GetDistToNonRock(start, group, IntVec3.SouthWest, 40);
            float distToNonRock8 = (float)this.GetDistToNonRock(start, group, IntVec3.SouthEast, 40);
            dist = Mathf.Max(distToNonRock1, distToNonRock2, distToNonRock3, distToNonRock4, distToNonRock5, distToNonRock6, distToNonRock7, distToNonRock8);
            return GenMath.MaxByRandomIfEqual<float>(0.0f, (float)((double)distToNonRock1 + (double)distToNonRock8 / 2.0 + (double)distToNonRock6 / 2.0), 45f, (float)((double)distToNonRock8 + (double)distToNonRock3 / 2.0 + (double)distToNonRock1 / 2.0), 90f, (float)((double)distToNonRock3 + (double)distToNonRock8 / 2.0 + (double)distToNonRock7 / 2.0), 135f, (float)((double)distToNonRock7 + (double)distToNonRock3 / 2.0 + (double)distToNonRock2 / 2.0), 180f, (float)((double)distToNonRock2 + (double)distToNonRock7 / 2.0 + (double)distToNonRock5 / 2.0), 225f, (float)((double)distToNonRock5 + (double)distToNonRock4 / 2.0 + (double)distToNonRock2 / 2.0), 270f, (float)((double)distToNonRock4 + (double)distToNonRock6 / 2.0 + (double)distToNonRock5 / 2.0), 315f, (float)((double)distToNonRock6 + (double)distToNonRock4 / 2.0 + (double)distToNonRock1 / 2.0), 0.0001f);
        }

        private void Dig(
          IntVec3 start,
          float dir,
          float width,
          List<IntVec3> group,
          Map map,
          bool closed,
          HashSet<IntVec3> visited = null)
        {
            Vector3 vector3Shifted = start.ToVector3Shifted();
            IntVec3 intVec3 = start;
            float num1 = 0.0f;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid caves = MapGenerator.Caves;
            bool flag1 = false;
            bool flag2 = false;
            if (visited == null)
                visited = new HashSet<IntVec3>();
            GenStep_CustomCaves.tmpGroupSet.Clear();
            GenStep_CustomCaves.tmpGroupSet.AddRange<IntVec3>(group);
            int num2 = 0;
            while (true)
            {
                if (closed)
                {
                    int num3 = GenRadial.NumCellsInRadius((float)((double)width / 2.0 + 1.5));
                    for (int index1 = 0; index1 < num3; ++index1)
                    {
                        IntVec3 index2 = intVec3 + GenRadial.RadialPattern[index1];
                        if (!visited.Contains(index2) && (!GenStep_CustomCaves.tmpGroupSet.Contains(index2) || (double)caves[index2] > 0.0))
                            return;
                    }
                }

                if (map.TileInfo.hilliness == Hilliness.Mountainous) // Less extreme branching for mountainous
                {
                    if (num2 >= 15 && (double)width > 1.39999997615814 + (double)GenStep_CustomCaves.BranchedTunnelWidthOffsetImpassable.max)
                    {
                        if (!flag1 && Rand.Chance(0.05f))
                        {
                            this.DigInBestDirection(intVec3, dir, new FloatRange(40f, 90f), width - GenStep_CustomCaves.BranchedTunnelWidthOffsetImpassable.RandomInRange, group, map, closed, visited);
                            flag1 = true;
                        }
                        if (!flag2 && Rand.Chance(0.05f))
                        {
                            this.DigInBestDirection(intVec3, dir, new FloatRange(-90f, -40f), width - GenStep_CustomCaves.BranchedTunnelWidthOffsetImpassable.RandomInRange, group, map, closed, visited);
                            flag2 = true;
                        }
                    }
                }
                else
                {
                    if (num2 >= 15 && (double)width > 1.39999997615814 + (double)GenStep_CustomCaves.BranchedTunnelWidthOffset.max)
                    {
                        if (!flag1 && Rand.Chance(0.1f))
                        {
                            this.DigInBestDirection(intVec3, dir, new FloatRange(40f, 90f), width - GenStep_CustomCaves.BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
                            flag1 = true;
                        }
                        if (!flag2 && Rand.Chance(0.1f))
                        {
                            this.DigInBestDirection(intVec3, dir, new FloatRange(-90f, -40f), width - GenStep_CustomCaves.BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
                            flag2 = true;
                        }
                    }
                }

                bool hitAnotherTunnel;
                this.SetCaveAround(intVec3, width, map, visited, out hitAnotherTunnel);
                if (!hitAnotherTunnel)
                {
                    while (vector3Shifted.ToIntVec3() == intVec3)
                    {
                        vector3Shifted += Vector3Utility.FromAngleFlat(dir) * 0.5f;
                        num1 += 0.5f;
                    }
                    if (GenStep_CustomCaves.tmpGroupSet.Contains(vector3Shifted.ToIntVec3()))
                    {
                        IntVec3 c = new IntVec3(intVec3.x, 0, vector3Shifted.ToIntVec3().z);
                        if (this.IsRock(c, elevation, map))
                        {
                            caves[c] = Mathf.Max(caves[c], width);
                            visited.Add(c);
                        }
                        intVec3 = vector3Shifted.ToIntVec3();
                        dir += (float)this.directionNoise.GetValue((double)num1 * 60.0, (double)start.x * 200.0, (double)start.z * 200.0) * 8f;
                        width -= 0.034f;
                        if ((double)width >= 1.39999997615814)
                            ++num2;
                        else
                            goto label_24;
                    }
                    else
                        goto label_15;
                }
                else
                    break;
            }
            return;
        label_15:
            return;
        label_24:;
        }

        private void DigInBestDirection(
          IntVec3 curIntVec,
          float curDir,
          FloatRange dirOffset,
          float width,
          List<IntVec3> group,
          Map map,
          bool closed,
          HashSet<IntVec3> visited = null)
        {
            int num = -1;
            float dir1 = -1f;
            for (int index = 0; index < 6; ++index)
            {
                float dir2 = curDir + dirOffset.RandomInRange;
                int distToNonRock = this.GetDistToNonRock(curIntVec, group, dir2, 50);
                if (distToNonRock > num)
                {
                    num = distToNonRock;
                    dir1 = dir2;
                }
            }
            if (num < 18)
                return;
            this.Dig(curIntVec, dir1, width, group, map, closed, visited);
        }

        private void SetCaveAround(
          IntVec3 around,
          float tunnelWidth,
          Map map,
          HashSet<IntVec3> visited,
          out bool hitAnotherTunnel)
        {
            hitAnotherTunnel = false;
            int num = GenRadial.NumCellsInRadius(tunnelWidth / 2f);
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid caves = MapGenerator.Caves;
            for (int index = 0; index < num; ++index)
            {
                IntVec3 c = around + GenRadial.RadialPattern[index];
                if (this.IsRock(c, elevation, map))
                {
                    if ((double)caves[c] > 0.0 && !visited.Contains(c))
                        hitAnotherTunnel = true;
                    caves[c] = Mathf.Max(caves[c], tunnelWidth);
                    visited.Add(c);
                }
            }
        }

        private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, IntVec3 offset, int maxDist)
        {
            GenStep_CustomCaves.groupSet.Clear();
            GenStep_CustomCaves.groupSet.AddRange<IntVec3>(group);
            for (int index = 0; index <= maxDist; ++index)
            {
                IntVec3 intVec3 = from + offset * index;
                if (!GenStep_CustomCaves.groupSet.Contains(intVec3))
                    return index;
            }
            return maxDist;
        }

        private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, float dir, int maxDist)
        {
            GenStep_CustomCaves.groupSet.Clear();
            GenStep_CustomCaves.groupSet.AddRange<IntVec3>(group);
            Vector3 vector3 = Vector3Utility.FromAngleFlat(dir);
            for (int index = 0; index <= maxDist; ++index)
            {
                IntVec3 intVec3 = (from.ToVector3Shifted() + vector3 * (float)index).ToIntVec3();
                if (!GenStep_CustomCaves.groupSet.Contains(intVec3))
                    return index;
            }
            return maxDist;
        }

        private float GetDistToCave(
          IntVec3 cell,
          List<IntVec3> group,
          Map map,
          float maxDist,
          bool treatOpenSpaceAsCave)
        {
            MapGenFloatGrid caves = MapGenerator.Caves;
            GenStep_CustomCaves.tmpGroupSet.Clear();
            GenStep_CustomCaves.tmpGroupSet.AddRange<IntVec3>(group);
            int num = GenRadial.NumCellsInRadius(maxDist);
            IntVec3[] radialPattern = GenRadial.RadialPattern;
            for (int index1 = 0; index1 < num; ++index1)
            {
                IntVec3 index2 = cell + radialPattern[index1];
                if (treatOpenSpaceAsCave && !GenStep_CustomCaves.tmpGroupSet.Contains(index2) || index2.InBounds(map) && (double)caves[index2] > 0.0)
                    return cell.DistanceTo(index2);
            }
            return maxDist;
        }

        private void RemoveSmallDisconnectedSubGroups(List<IntVec3> group, Map map)
        {
            GenStep_CustomCaves.groupSet.Clear();
            GenStep_CustomCaves.groupSet.AddRange<IntVec3>(group);
            GenStep_CustomCaves.groupVisited.Clear();
            for (int index1 = 0; index1 < group.Count; ++index1)
            {
                if (!GenStep_CustomCaves.groupVisited.Contains(group[index1]) && GenStep_CustomCaves.groupSet.Contains(group[index1]))
                {
                    GenStep_CustomCaves.subGroup.Clear();
                    map.floodFiller.FloodFill(group[index1], (Predicate<IntVec3>)(x => GenStep_CustomCaves.groupSet.Contains(x)), (Action<IntVec3>)(x =>
                    {
                        GenStep_CustomCaves.subGroup.Add(x);
                        GenStep_CustomCaves.groupVisited.Add(x);
                    }), int.MaxValue, false, (IEnumerable<IntVec3>)null);
                    if (GenStep_CustomCaves.subGroup.Count < 300 || (double)GenStep_CustomCaves.subGroup.Count < 0.0500000007450581 * (double)group.Count)
                    {
                        for (int index2 = 0; index2 < GenStep_CustomCaves.subGroup.Count; ++index2)
                            GenStep_CustomCaves.groupSet.Remove(GenStep_CustomCaves.subGroup[index2]);
                    }
                }
            }
            group.Clear();
            group.AddRange((IEnumerable<IntVec3>)GenStep_CustomCaves.groupSet);
        }

        // Impassable Map Code
        private static ITerrainOverride GenerateMiddleArea(System.Random r, Map map)
        {
            int num = GenStep_CustomCaves.RandomBasePatch(r, map.Size.x);
            int num2 = GenStep_CustomCaves.RandomBasePatch(r, map.Size.z);
            GenStep_CustomCaves.pondradius = Rand.Range((map.Size.x/12)-4, map.Size.x/12);
            GenStep_CustomCaves.pondmiddle = new IntVec3(num, 0, num2);
            return new TerrainOverrideRound(new IntVec3(num, 0, num2), (int)pondradius);
        }

        private static ITerrainOverride GenerateEdgeAreaLeft(System.Random r, Map map)
        {
            if (Rand.Chance(0.5f))
            {
                return new TerrainOverrideSquare(-10, -10, 20, 20);
            }
            else
            {
                return new TerrainOverrideSquare(-10, map.Size.z - 30, 20, map.Size.z);
            }
        }

        private static ITerrainOverride GenerateEdgeAreaRight(System.Random r, Map map)
        {
            if (Rand.Chance(0.5f))
            {
                return new TerrainOverrideSquare(map.Size.x - 30, -10, map.Size.x, 20);
            }
            else
            {
                return new TerrainOverrideSquare(map.Size.x - 30, map.Size.z - 30, map.Size.x, map.Size.z);
            }
        }

        private static int RandomBasePatch(System.Random r, int size)
        {
            int num = size / 2;
            int num2 = r.Next((int)(0.01 * (double)20f * (double)num));
            if (r.Next(2) == 0)
            {
                num2 *= -1;
            }
            return num + num2;
        }
    }
}
