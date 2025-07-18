using System;
using System.Numerics; // Added for Vector3
using ACE.Server.Physics.Alt;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Port of GDLE/PhatSDK SPHEREPATH struct for physics transitions.
    /// </summary>
    public class SPHEREPATH
    {
        public enum InsertType
        {
            TRANSITION_INSERT = 0,
            PLACEMENT_INSERT = 1,
            INITIAL_PLACEMENT_INSERT = 2
        }

        // Members
        public uint NumSphere;
        public CSphere[] LocalSphere;
        public Vector LocalLowPoint;
        public CSphere[] GlobalSphere;
        public Vector GlobalLowPoint;
        public CSphere[] LocalspaceSphere;
        public Vector LocalspaceLowPoint;
        public Vector[] LocalspaceCurrCenter;
        public Vector[] GlobalCurrCenter;
        // Position and cell types are opaque for now
        public IntPtr LocalspacePos;
        public Vector LocalspaceZ;
        public IntPtr BeginCell;
        public IntPtr BeginPos;
        public IntPtr EndPos;
        public IntPtr CurrCell;
        public IntPtr CurrPos;
        public Vector GlobalOffset;
        public int StepUp;
        public Vector StepUpNormal;
        public int Collide;
        public IntPtr CheckCell;
        public IntPtr CheckPos;
        public InsertType Insert_Type;
        public int StepDown;
        public InsertType Backup { get; set; }
        public IntPtr BackupCell;
        public IntPtr BackupCheckPos;
        public int ObstructionEthereal;
        public int HitsInteriorCell;
        public int BldgCheck;
        public float WalkableAllowance;
        public float WalkInterp;
        public float StepDownAmt;
        public CSphere WalkableCheckPos;
        public IntPtr Walkable { get; set; } // CPolygon*
        public int CheckWalkable;
        public Vector WalkableUp;
        public IntPtr WalkablePos;
        public float WalkableScale;
        public int CellArrayValid;
        public int NegStepUp;
        public Vector NegCollisionNormal;
        public int NegPolyHit;
        public int PlacementAllowsSliding;

        // Additional properties for compatibility with CTransition
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public System.Collections.Generic.List<CSphere> Spheres { get; set; }
        public float WalkableZ { get; set; }
        public bool HasWalkable { get; set; }

        // TODO: Implement methods as needed
        public SPHEREPATH()
        {
            NumSphere = 0;
            LocalSphere = null;
            LocalLowPoint = new Vector(0, 0, 0);
            GlobalSphere = null;
            GlobalLowPoint = new Vector(0, 0, 0);
            LocalspaceSphere = null;
            LocalspaceLowPoint = new Vector(0, 0, 0);
            LocalspaceCurrCenter = null;
            GlobalCurrCenter = null;
            LocalspacePos = IntPtr.Zero;
            LocalspaceZ = new Vector(0, 0, 0);
            BeginCell = IntPtr.Zero;
            BeginPos = IntPtr.Zero;
            EndPos = IntPtr.Zero;
            CurrCell = IntPtr.Zero;
            CurrPos = IntPtr.Zero;
            GlobalOffset = new Vector(0, 0, 0);
            StepUp = 0;
            StepUpNormal = new Vector(0, 0, 0);
            Collide = 0;
            CheckCell = IntPtr.Zero;
            CheckPos = IntPtr.Zero;
            Insert_Type = InsertType.TRANSITION_INSERT;
            StepDown = 0;
            Backup = InsertType.TRANSITION_INSERT;
            BackupCell = IntPtr.Zero;
            BackupCheckPos = IntPtr.Zero;
            ObstructionEthereal = 0;
            HitsInteriorCell = 0;
            BldgCheck = 0;
            WalkableAllowance = 0.0f;
            WalkInterp = 0.0f;
            StepDownAmt = 0.0f;
            WalkableCheckPos = null;
            Walkable = IntPtr.Zero;
            CheckWalkable = 0;
            WalkableUp = new Vector(0, 0, 0);
            WalkablePos = IntPtr.Zero;
            WalkableScale = 0.0f;
            CellArrayValid = 0;
            NegStepUp = 0;
            NegCollisionNormal = new Vector(0, 0, 0);
            NegPolyHit = 0;
            PlacementAllowsSliding = 0;
            
            // Initialize new properties
            StartPosition = System.Numerics.Vector3.Zero;
            EndPosition = System.Numerics.Vector3.Zero;
            Spheres = new System.Collections.Generic.List<CSphere>();
        }
        public void Init()
        {
            NumSphere = 0;
            LocalSphere = null;
            LocalLowPoint = new Vector(0, 0, 0);
            GlobalSphere = null;
            GlobalLowPoint = new Vector(0, 0, 0);
            LocalspaceSphere = null;
            LocalspaceLowPoint = new Vector(0, 0, 0);
            LocalspaceCurrCenter = null;
            GlobalCurrCenter = null;
            LocalspacePos = IntPtr.Zero;
            LocalspaceZ = new Vector(0, 0, 0);
            BeginCell = IntPtr.Zero;
            BeginPos = IntPtr.Zero;
            EndPos = IntPtr.Zero;
            CurrCell = IntPtr.Zero;
            CurrPos = IntPtr.Zero;
            GlobalOffset = new Vector(0, 0, 0);
            StepUp = 0;
            StepUpNormal = new Vector(0, 0, 0);
            Collide = 0;
            CheckCell = IntPtr.Zero;
            CheckPos = IntPtr.Zero;
            Insert_Type = InsertType.TRANSITION_INSERT;
            StepDown = 0;
            Backup = InsertType.TRANSITION_INSERT;
            BackupCell = IntPtr.Zero;
            BackupCheckPos = IntPtr.Zero;
            ObstructionEthereal = 0;
            HitsInteriorCell = 0;
            BldgCheck = 0;
            WalkableAllowance = 0.0f;
            WalkInterp = 0.0f;
            StepDownAmt = 0.0f;
            WalkableCheckPos = null;
            Walkable = IntPtr.Zero;
            CheckWalkable = 0;
            WalkableUp = new Vector(0, 0, 0);
            WalkablePos = IntPtr.Zero;
            WalkableScale = 0.0f;
            CellArrayValid = 0;
            NegStepUp = 0;
            NegCollisionNormal = new Vector(0, 0, 0);
            NegPolyHit = 0;
            PlacementAllowsSliding = 0;
        }
        public void InitSphere(uint numSphere, CSphere[] sphere, float scale)
        {
            NumSphere = numSphere;
            LocalSphere = sphere;
            // Additional logic for scaling and initialization can be added as needed
        }

        public void InitPath(IntPtr beginCell, IntPtr beginPos, IntPtr endPos)
        {
            BeginCell = beginCell;
            BeginPos = beginPos;
            EndPos = endPos;
        }
        public bool IsWalkableAllowable(float zval)
        {
            return zval > WalkableAllowance;
        }
        public void CacheGlobalCurrCenter()
        {
            if (GlobalCurrCenter == null || LocalSphere == null)
                return;
            for (int i = 0; i < NumSphere && i < GlobalCurrCenter.Length && i < LocalSphere.Length; i++)
            {
                // In the real implementation, this would use a local-to-global transform
                // For now, just copy the local sphere center as a placeholder
                GlobalCurrCenter[i] = LocalSphere[i].Center;
            }
        }
        public void CacheGlobalSphere(Vector offset)
        {
            if (GlobalSphere == null || LocalSphere == null)
                return;
            for (int i = 0; i < NumSphere && i < GlobalSphere.Length && i < LocalSphere.Length; i++)
            {
                // In the real implementation, this would apply the offset to the local sphere center
                GlobalSphere[i] = new CSphere((Vector3)((Vector3)LocalSphere[i].Center + (Vector3)offset), LocalSphere[i].Radius);
            }
        }
        public void CacheLocalspaceSphere(IntPtr p, float scale)
        {
            if (LocalspaceSphere == null || LocalSphere == null)
                return;
            for (int i = 0; i < NumSphere && i < LocalspaceSphere.Length && i < LocalSphere.Length; i++)
            {
                // In the real implementation, this would use a position transform
                // For now, just scale the local sphere center and radius as a placeholder
                LocalspaceSphere[i] = new CSphere(LocalSphere[i].Center * scale, LocalSphere[i].Radius * scale);
            }
        }
        public Vector GetCurrPosCheckPosBlockOffset()
        {
            // In the real implementation, this would compute the offset between current and check positions
            // For now, just return the GlobalOffset as a placeholder
            return GlobalOffset;
        }
        public void AddOffsetToCheckPos(Vector offset)
        {
            // In the real implementation, this would update the check position
            // For now, just add the offset to GlobalOffset as a placeholder
            GlobalOffset += offset;
        }

        public void AddOffsetToCheckPos(Vector offset, float radius)
        {
            // In the real implementation, this would update the check position with radius consideration
            // For now, just add the offset and radius to GlobalOffset as a placeholder
            GlobalOffset += offset + new Vector(radius, radius, radius);
        }
        public void SetCollide(Vector collisionNormal)
        {
            // In the real implementation, this would set collision state and normal
            // For now, just set Collide to 1 as a placeholder
            Collide = 1;
        }
        public TransitionState StepUpSlide(OBJECTINFO obj, ACE.Server.Physics.Alt.CollisionInfo collisions)
        {
            // Ported from C++ SPHEREPATH::step_up_slide
            collisions.ContactPlaneValid = 0;
            collisions.ContactPlaneIsWater = 0;
            StepUp = 0;
            if (GlobalSphere != null && GlobalSphere.Length > 0 && GlobalCurrCenter != null && GlobalCurrCenter.Length > 0)
            {
                return GlobalSphere[0].SlideSphere(this, collisions, StepUpNormal);
            }
            // If not available, return COLLIDED_TS as a fallback
            return TransitionState.COLLIDED_TS;
        }
        public void RestoreCheckPos()
        {
            // Stub: No-op for now
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::check_walkables
        public int CheckWalkables(BSPTree bspTree, CSphere checkPos, float scale)
        {
            // In C++: return (root_node->hits_walkable(path, &valid_pos, &path->localspace_z) != 0) + 1;
            if (bspTree == null || checkPos == null)
                return 0;
            // Assume BSPTree has a HitsWalkable method matching the signature
            var validPos = checkPos; // In C++: valid_pos = *check_pos
            int result = bspTree.HitsWalkable(this, validPos, LocalspaceZ) != 0 ? 2 : 1;
            return result;
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::set_check_pos
        public void SetCheckPos(IntPtr p, IntPtr cell)
        {
            CheckPos = p;
            CheckCell = cell;
            CellArrayValid = 0;
            // Optionally, cache global sphere if needed
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::save_check_pos
        public void SaveCheckPos()
        {
            // Save the current check position and cell to backup fields
            BackupCheckPos = CheckPos;
            BackupCell = CheckCell;
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::set_walkable
        public void SetWalkable(CSphere sphere, IntPtr poly, Vector zaxis, IntPtr localPos, float scale)
        {
            WalkableCheckPos = sphere;
            Walkable = poly;
            WalkableUp = zaxis;
            WalkablePos = localPos;
            WalkableScale = scale;
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::set_neg_poly_hit
        public void SetNegPolyHit(int stepUp, Vector collisionNormal)
        {
            NegStepUp = stepUp;
            NegPolyHit = 1;
            NegCollisionNormal = new Vector(-collisionNormal.X, -collisionNormal.Y, -collisionNormal.Z);
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::adjust_check_pos (UNFINISHED stub)
        public void AdjustCheckPos(uint cellId)
        {
            // UNFINISHED: need for cell position adjustment
            // TODO: Implement full logic for adjusting check position based on cell ID
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::get_walkable_pos
        public IntPtr GetWalkablePos()
        {
            return WalkablePos;
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::set_walkable_check_pos
        public void SetWalkableCheckPos(CSphere sphere)
        {
            WalkableCheckPos = sphere;
        }
        // 1:1 port from GDLE/PhatSDK SPHEREPATH::precipice_slide (UNFINISHED stub)
        public TransitionState PrecipiceSlide(ACE.Server.Physics.Alt.CollisionInfo collisions)
        {
            /*
            // Ported from C++ SPHEREPATH::precipice_slide
            // NOTE: This assumes Walkable is a Polygon and WalkableCheckPos is a CSphere
            if (Walkable != IntPtr.Zero && WalkableCheckPos != null)
            {
                // TODO: Replace with actual Polygon and Vector logic
                // For now, just simulate a crossed edge and normal
                Vector normal = new Vector(0, 0, 1.0f); // Placeholder
                bool crossedEdge = true; // Placeholder for Walkable->find_crossed_edge
                if (crossedEdge)
                {
                    Walkable = IntPtr.Zero;
                    StepUp = 0;
                    // TODO: normal = WalkablePos.Frame.LocalToGlobalVec(normal);
                    // TODO: blockOffset = GetCurrPosCheckPosBlockOffset();
                    // TODO: someOffset = GlobalSphere[0].Center - GlobalCurrCenter[0];
                    // TODO: if (normal.Dot(blockOffset + someOffset) > 0.0f) normal = normal * -1.0f;
                    if (GlobalSphere != null && GlobalSphere.Length > 0 && GlobalCurrCenter != null && GlobalCurrCenter.Length > 0)
                    {
                        return GlobalSphere[0].SlideSphere(this, collisions, normal, GlobalCurrCenter[0]);
                    }
                }
                else
                {
                    Walkable = IntPtr.Zero;
                    return TransitionState.COLLIDED_TS;
                }
            }
            return TransitionState.COLLIDED_TS;
            */
            // UNFINISHED: need for precipice slide logic
            // TODO: Implement full logic for precipice sliding
            return TransitionState.OK_TS;
        }
    }
} 