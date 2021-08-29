using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CustomCaves
{
    public class GenStep_CustomCaveHives : GenStep
    {
        private List<IntVec3> rockCells = new List<IntVec3>();
        private List<IntVec3> possibleSpawnCells = new List<IntVec3>();
        private List<Hive> spawnedHives = new List<Hive>();
        public static bool allowImpassableHives;
        public static bool allowCaveHiveReproduction;

        public override int SeedPart
        {
            get
            {
                return 349641510;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!Find.Storyteller.difficulty.allowCaveHives || (map.TileInfo.hilliness == Hilliness.Mountainous && !allowImpassableHives))
            {
                return;
            }
            MapGenFloatGrid caves = MapGenerator.Caves;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            float num1 = 0.7f;
            int num2 = 0;
            this.rockCells.Clear();
            foreach (IntVec3 allCell in map.AllCells)
            {
                if ((double)elevation[allCell] > (double)num1)
                    this.rockCells.Add(allCell);
                if ((double)caves[allCell] > 0.0)
                    ++num2;
            }
            List<IntVec3> list = map.AllCells.Where<IntVec3>((Func<IntVec3, bool>)(c => map.thingGrid.ThingsAt(c).Any<Thing>((Func<Thing, bool>)(thing => thing.Faction != null)))).ToList<IntVec3>();
            GenMorphology.Dilate(list, 50, map, (Predicate<IntVec3>)null);
            HashSet<IntVec3> intVec3Set = new HashSet<IntVec3>((IEnumerable<IntVec3>)list);
            int num3 = GenMath.RoundRandom((float)num2 / 1000f);
            GenMorphology.Erode(this.rockCells, 10, map, (Predicate<IntVec3>)null);
            this.possibleSpawnCells.Clear();
            for (int index = 0; index < this.rockCells.Count; ++index)
            {
                if ((double)caves[this.rockCells[index]] > 0.0 && !intVec3Set.Contains(this.rockCells[index]))
                    this.possibleSpawnCells.Add(this.rockCells[index]);
            }
            this.spawnedHives.Clear();
            for (int index = 0; index < num3; ++index)
                this.TrySpawnHive(map);
            this.spawnedHives.Clear();
        }

        private void TrySpawnHive(Map map)
        {
            IntVec3 spawnCell;
            if (!this.TryFindHiveSpawnCell(map, out spawnCell))
            {
                return;
            }
            this.possibleSpawnCells.Remove(spawnCell);
            Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Hive, (ThingDef)null), spawnCell, map, WipeMode.Vanish);
            hive.SetFaction(Faction.OfInsects, (Pawn)null);
            hive.PawnSpawner.aggressive = false;
            hive.GetComps<CompSpawner>().Where<CompSpawner>((Func<CompSpawner, bool>)(x => x.PropsSpawner.thingToSpawn == ThingDefOf.GlowPod)).First<CompSpawner>().TryDoSpawn();
            hive.PawnSpawner.SpawnPawnsUntilPoints(Rand.Range(200f, 500f));
            if(allowCaveHiveReproduction)
            {
                hive.PawnSpawner.canSpawnPawns = true;
                hive.GetComp<CompSpawnerHives>().canSpawnHives = true;
            }
            else
            {
                hive.PawnSpawner.canSpawnPawns = false;
                hive.GetComp<CompSpawnerHives>().canSpawnHives = false;
            }
            this.spawnedHives.Add(hive);
        }

        private bool TryFindHiveSpawnCell(Map map, out IntVec3 spawnCell)
        {
            float num1 = -1f;
            IntVec3 intVec3 = IntVec3.Invalid;
            IntVec3 result;
            for (int index1 = 0; index1 < 3 && this.possibleSpawnCells.Where<IntVec3>((Func<IntVec3, bool>)(x => x.Standable(map) && x.GetFirstItem(map) == null && x.GetFirstBuilding(map) == null && x.GetFirstPawn(map) == null)).TryRandomElement<IntVec3>(out result); ++index1)
            {
                float num2 = -1f;
                for (int index2 = 0; index2 < this.spawnedHives.Count; ++index2)
                {
                    float squared = (float)result.DistanceToSquared(this.spawnedHives[index2].Position);
                    if ((double)num2 < 0.0 || (double)squared < (double)num2)
                        num2 = squared;
                }
                if (!intVec3.IsValid || (double)num2 > (double)num1)
                {
                    intVec3 = result;
                    num1 = num2;
                }
            }
            spawnCell = intVec3;
            return spawnCell.IsValid;
        }
    }
}