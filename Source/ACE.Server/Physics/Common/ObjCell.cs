using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ACE.Entity.Enum;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Combat;
using ACE.Server.Physics.Managers;

using log4net;

namespace ACE.Server.Physics.Common
{
    public class ObjCell: PartCell, IEquatable<ObjCell>
    {
        //private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public uint ID;
        public LandDefs.WaterType WaterType;
        public Position Pos;
        private ConcurrentDictionary<uint, PhysicsObj> _ObjectList;
        public List<PhysicsObj> ObjectList { get { return _ObjectList.Values.ToList(); } }
        //public List<int> LightList;
        public List<ShadowObj> ShadowObjectList;
        //public List<uint> ShadowObjectIDs;
        public uint RestrictionObj;
        //public List<int> ClipPlanes;
        //public int NumStabs;
        //public List<DatLoader.Entity.Stab> VisibleCells;
        public bool SeenOutside;
        public List<uint> VoyeurTable;
        public int? VariationId;

        public Landblock CurLandblock;


        /// <summary>
        /// TODO: This is a temporary locking mechanism, Mag-nus 2019-10-20
        /// TODO: The objective here is to allow multi-threading of physics, divided by landblock groups
        /// TODO: This solves the issue of a player leaving one landblock group and trying to insert itself a target landblock group while that target landblock group is also in processing
        /// TODO: In the future, the object should be removed from the landblock group and added to a queue of items that need to be inserted into a target
        /// TODO: That list should then be processed in a single thread.
        /// TODO: The above solution should remove the need for ObjCell access locking, and also increase performance
        /// </summary>
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static readonly ObjCell EmptyCell = new ObjCell();

        public ObjCell(): base()
        {
            Init();
        }

        public ObjCell(uint cellID): base()
        {
            ID = cellID;
            Init();
        }

        public void AddObject(PhysicsObj obj)
        {

            bool res = _ObjectList.TryAdd(obj.ID, obj);
            
            if (obj.ID == 0 || obj.Parent != null || obj.State.HasFlag(PhysicsState.Hidden) || VoyeurTable == null || !res)
                return;

            foreach (var voyeur_id in VoyeurTable)
            {
                if (voyeur_id != obj.ID && voyeur_id != 0)
                {
                    var voyeur = PhysicsObj.GetObjectA(voyeur_id);
                    if (voyeur == null) continue;

                    //var info = new DetectionInfo(obj.ID, DetectionType.EnteredDetection);
                    voyeur.receive_detection_update();
                }
            }
        }

        public void AddShadowObject(ShadowObj shadowObj)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                ShadowObjectList.Add(shadowObj);                
                shadowObj.Cell = this;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }


        public bool Equals(ObjCell objCell)
        {
            if (objCell == null)
                return false;

            return ID.Equals(objCell.ID);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjCell);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public virtual TransitionState FindCollisions(Transition transition)
        {
            return TransitionState.Invalid;
        }

        public virtual TransitionState FindEnvCollisions(Transition transition)
        {
            return TransitionState.Invalid;
        }

        public TransitionState FindObjCollisions(Transition transition)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                var path = transition.SpherePath;

                if (path.InsertType == InsertType.InitialPlacement)
                    return TransitionState.OK;

                //var target = transition.ObjectInfo.Object.ProjectileTarget;

                // If we use the following: foreach (var shadowObj in ShadowObjectList), an InvalidOperationException is thrown.
                // Very rarely though, as we iterate through it, the collection will change.
                // To avoid the InvalidOperationException, we use a for loop.
                // We do not yet know why the collection changes.
                for (int i = ShadowObjectList.Count - 1; i >= 0; i--)
                {
                    var shadowObj = ShadowObjectList[i];

                    var obj = shadowObj.PhysicsObj;

                    if (obj.Parent != null || obj.Equals(transition.ObjectInfo.Object))
                        continue;

                    // clip through dynamic non-target objects
                    // now uses ObjectInfo.TargetId in FindObjCollisions / MissileIgnore
                    //if (target != null && !obj.Equals(target) && /*!obj.State.HasFlag(PhysicsState.Static)*/
                        //obj.WeenieObj.IsCreature())
                        //continue;

                    var state = obj.FindObjCollisions(transition);
                    if (state != TransitionState.OK)
                    {
                        // custom: fix hellfire spawn colliding with volcano heat, and possibly other placements
                        if (path.InsertType == InsertType.Placement && (obj.State & PhysicsState.Ethereal) != 0)
                            continue;

                        return state;
                    }

                }

                return TransitionState.OK;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public static ObjCell GetVisible(uint cellID, int? variationId)
        {
            if (cellID == 0) return null;

            // is this supposed to return a list?
            /*if ((cellID & 0xFFFF) >= 0x100)
               return EnvCell.get_visible(cellID);
            else
                return LandCell.Get(cellID);*/
            return LScape.get_landcell(cellID, variationId);
        }

        public void Init()
        {
            Pos = new Position();
            _ObjectList = new ConcurrentDictionary<uint, PhysicsObj>();
            ShadowObjectList = new List<ShadowObj>();
            VoyeurTable = new List<uint>();
        }

        public void RemoveObject(PhysicsObj obj)
        {

            _ObjectList.TryRemove(new KeyValuePair<uint, PhysicsObj>(obj.ID, obj));
                
            update_all_voyeur(obj, DetectionType.LeftDetection);

        }

        public bool check_collisions(PhysicsObj obj)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                foreach (var shadowObj in ShadowObjectList)
                {
                    var pObj = shadowObj.PhysicsObj;
                    if (pObj.Parent == null && !pObj.Equals(obj) && pObj.check_collision(obj))
                        return true;
                }

                return false;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public TransitionState check_entry_restrictions(Transition transition)
        {
            // custom - acclient checks for entry restrictions (housing barriers)
            // for each tick in the transition, regardless if there is a cell change

            // optimizing for server here, to only check unverified cell changes

            if (!transition.ObjectInfo.Object.IsPlayer || transition.CollisionInfo.VerifiedRestrictions || transition.SpherePath.BeginCell?.ID == ID)
            {
                return TransitionState.OK;
            }

            if (transition.ObjectInfo.Object == null)
                return TransitionState.Collided;

            var weenieObj = transition.ObjectInfo.Object.WeenieObj;

            // TODO: handle DatObject
            if (weenieObj != null)
            {
                //if (transition.ObjectInfo.State.HasFlag(ObjectInfoState.IsPlayer))
                if (transition.ObjectInfo.Object.IsPlayer)
                {
                    if (RestrictionObj != 0 && !weenieObj.CanBypassMoveRestrictions())
                    {
                        var restrictionObj = ServerObjectManager.GetObjectA(RestrictionObj);

                        if (restrictionObj?.WeenieObj == null)
                            return TransitionState.Collided;

                        if (!restrictionObj.WeenieObj.CanMoveInto(weenieObj))
                        {
                            handle_move_restriction(transition);
                            return TransitionState.Collided;
                        }
                        else
                            transition.CollisionInfo.VerifiedRestrictions = true;
                    }
                }
            }
            return TransitionState.OK;
        }

        public virtual bool handle_move_restriction(Transition transition)
        {
            // empty base?
            return false;
        }

        public static void find_cell_list(Position position, int numSphere, List<Sphere> sphere, CellArray cellArray, ref ObjCell currCell, SpherePath path, int? variation)
        {
            //cellArray.NumCells = 0;
            cellArray.AddedOutside = false;            

            var visibleCell = GetVisible(position.ObjCellID, variation);

            if ((position.ObjCellID & 0xFFFF) >= 0x100)
            {
                if (path != null)
                    path.HitsInteriorCell = true;

                cellArray.add_cell(position.ObjCellID, visibleCell);
            }
            else
                LandCell.add_all_outside_cells(position, numSphere, sphere, cellArray);

            if (visibleCell != null && numSphere != 0)
            {
                for (var i = 0; i < cellArray.Cells.Count; i++)
                {
                    var cell = cellArray.Cells.Values.ElementAt(i);
                    if (cell == null) continue;

                    cell.find_transit_cells(position, numSphere, sphere, cellArray, path);
                }
                //var checkCells = cellArray.Cells.Values.ToList();
                //foreach (var cell in checkCells)
                    //cell.find_transit_cells(position, numSphere, sphere, cellArray, path);

                if (currCell != null)
                {
                    currCell = null;
                    foreach (var cell in cellArray.Cells.Values)
                    {
                        if (cell == null) continue;

                        var blockOffset = LandDefs.GetBlockOffset(position.ObjCellID, cell.ID);
                        var localPoint = sphere[0].Center - blockOffset;

                        if (cell.point_in_cell(localPoint))
                        {
                            currCell = cell;
                            if ((cell.ID & 0xFFFF) >= 0x100)
                            {
                                if (path != null) path.HitsInteriorCell = true;
                                return;     // break?
                            }
                        }
                    }
                }
            }
        }

        public static void find_cell_list(Position position, int numCylSphere, List<CylSphere> cylSphere, CellArray cellArray, SpherePath path, int? variation)
        {
            if (numCylSphere > 10)
                numCylSphere = 10;

            var spheres = new List<Sphere>();

            for (var i = 0; i < numCylSphere; i++)
            {
                var sphere = new Sphere();
                sphere.Center = position.LocalToGlobal(cylSphere[i].LowPoint);
                sphere.Radius = cylSphere[i].Radius;
                spheres.Add(sphere);
            }

            ObjCell empty = null;
            find_cell_list(position, numCylSphere, spheres, cellArray, ref empty, path, variation);
        }

        public static void find_cell_list(Position position, Sphere sphere, CellArray cellArray, SpherePath path, int? variation)
        {
            var globalSphere = new Sphere();
            globalSphere.Center = position.LocalToGlobal(sphere.Center);
            globalSphere.Radius = sphere.Radius;

            ObjCell empty = null;
            find_cell_list(position, 1, globalSphere, cellArray, ref empty, path, variation);
        }

        public static void find_cell_list(CellArray cellArray, ref ObjCell checkCell, SpherePath path, int? variation)
        {
            find_cell_list(path.CheckPos, path.NumSphere, path.GlobalSphere, cellArray, ref checkCell, path, variation);
        }

        public static void find_cell_list(Position position, int numSphere, Sphere sphere, CellArray cellArray, ref ObjCell currCell, SpherePath path, int? variation)
        {
            find_cell_list(position, numSphere, new List<Sphere>() { sphere }, cellArray, ref currCell, path, variation);
        }

        public virtual void find_transit_cells(int numParts, List<PhysicsPart> parts, CellArray cellArray)
        {
            // empty base
        }

        public virtual void find_transit_cells(Position position, int numSphere, List<Sphere> sphere, CellArray cellArray, SpherePath path)
        {
            // empty base
        }

        public LandDefs.WaterType get_block_water_type()
        {
            if (CurLandblock != null)
                return CurLandblock.WaterType;

            return LandDefs.WaterType.NotWater;
        }

        public float get_water_depth(Vector3 point)
        {
            if (WaterType == LandDefs.WaterType.NotWater)
                return 0.0f;

            if (WaterType == LandDefs.WaterType.EntirelyWater)
                return 0.89999998f;

            if (CurLandblock != null)
                return CurLandblock.calc_water_depth(ID, point);

            return 0.1f;
        }

        public void hide_object(PhysicsObj obj)
        {
            update_all_voyeur(obj, DetectionType.LeftDetection);
        }

        public void init_objects()
        {
            Parallel.ForEach(_ObjectList,
                obj =>
                {
                    if (!obj.Value.State.HasFlag(PhysicsState.Static) && !obj.Value.is_completely_visible())
                        obj.Value.recalc_cross_cells();
                });
        }

        public virtual bool point_in_cell(Vector3 point)
        {
            return false;
        }

        public void release_shadow_objs()
        {
            foreach (var shadowObj in ShadowObjectList)
                shadowObj.PhysicsObj.ShadowObjects.Remove(ID);
        }

        public void release_objects()
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                while (ShadowObjectList.Count > 0)
                {
                    var shadowObj = ShadowObjectList[0];
                    remove_shadow_object(shadowObj);

                    shadowObj.PhysicsObj.remove_parts(this);
                }

                //if (NumObjects > 0 && ObjMaint != null)
                //ObjMaint.ReleaseObjCell(this);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void remove_shadow_object(ShadowObj shadowObj)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                // multiple shadows?
                ShadowObjectList.Remove(shadowObj);
                shadowObj.Cell = null;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }


        public void update_all_voyeur(PhysicsObj obj, DetectionType type, bool checkDetection = true)
        {
            if (obj.ID == 0 || obj.Parent != null || VoyeurTable == null)
                return;

            if (obj.State.HasFlag(PhysicsState.Hidden) && (checkDetection ? type == DetectionType.EnteredDetection : true))
                return;

            foreach (var voyeur_id in VoyeurTable)
            {
                if (voyeur_id != obj.ID && voyeur_id != 0)
                {
                    var voyeur = PhysicsObj.GetObjectA(voyeur_id);
                    if (voyeur == null) continue;

                    //var info = new DetectionInfo(obj.ID, type);
                    voyeur.receive_detection_update();
                }
            }
        }

        public bool IsVisible(ObjCell cell)
        {
            if (ID == cell.ID) return true;

            if ((ID & 0xFFFF) >= 0x100)
            {
                if (this is not EnvCell envCell)
                {
                    Console.WriteLine($"{ID:X8}.IsVisible({cell.ID:X8}): {ID:X8} not detected as EnvCell");
                    return false;
                }
                return envCell.IsVisibleIndoors(cell);
            }
            else if ((cell.ID & 0xFFFF) >= 0x100)
            {
                if (cell is not EnvCell envCell)
                {
                    Console.WriteLine($"{ID:X8}.IsVisible({cell.ID:X8}): {cell.ID:X8} not detected as EnvCell");
                    return false;
                }
                return envCell.IsVisibleIndoors(this);
            }
            else
            {
                // outdoors
                return IsVisibleOutdoors(cell);
            }
        }

        public bool IsVisibleOutdoors(ObjCell cell)
        {
            var blockDist = PhysicsObj.GetBlockDist(ID, cell.ID);
            return blockDist <= 1;
        }

        public void AddObjectListTo(List<PhysicsObj> target)
        {
            target.AddRange(ObjectList);
        }
    }
}
