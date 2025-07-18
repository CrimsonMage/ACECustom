using System;
using System.Collections.Generic;
using System.Numerics;

namespace ACE.Server.Physics.Alt
{
    public class CollisionInfo
    {
        // Fields from GDLE COLLISIONINFO
        public int LastKnownContactPlaneValid { get; set; }
        public Plane? LastKnownContactPlane { get; set; }
        public int LastKnownContactPlaneIsWater { get; set; }
        public int ContactPlaneValid { get; set; }
        public Plane? ContactPlane { get; set; }
        public uint ContactPlaneCellId { get; set; }
        public uint LastKnownContactPlaneCellId { get; set; }
        public int ContactPlaneIsWater { get; set; }
        public int SlidingNormalValid { get; set; }
        public Vector SlidingNormal { get; set; }
        public int CollisionNormalValid { get; set; }
        public Vector CollisionNormal { get; set; }
        public Vector AdjustOffset { get; set; }
        public uint NumCollideObject { get; set; }
        public List<CPhysicsObj> CollideObject { get; set; } = new List<CPhysicsObj>();
        public CPhysicsObj LastCollidedObject { get; set; }
        public int CollidedWithEnvironment { get; set; }
        public int FramesStationaryFall { get; set; }

        // Additional properties for compatibility
        public bool HasCollision { get; set; }
        public Vector3 CollisionPoint { get; set; }
        public float CollisionTime { get; set; }
        public int ObjectID { get; set; }
        public void SetContactPlane(Plane plane, int isWater)
        {
            ContactPlane = plane;
            ContactPlaneValid = 1;
            ContactPlaneIsWater = isWater;
        }
        public void SetCollisionNormal(Vector normal)
        {
            CollisionNormal = normal;
            CollisionNormalValid = 1;
        }

        public CollisionInfo() { Init(); }

        public void Init()
        {
            LastKnownContactPlaneValid = 0;
            LastKnownContactPlane = null;
            LastKnownContactPlaneIsWater = 0;
            ContactPlaneValid = 0;
            ContactPlane = null;
            ContactPlaneCellId = 0;
            LastKnownContactPlaneCellId = 0;
            ContactPlaneIsWater = 0;
            SlidingNormalValid = 0;
            SlidingNormal = new Vector(0, 0, 0);
            CollisionNormalValid = 0;
            CollisionNormal = new Vector(0, 0, 0);
            AdjustOffset = new Vector(0, 0, 0);
            NumCollideObject = 0;
            CollideObject.Clear();
            LastCollidedObject = null;
            CollidedWithEnvironment = 0;
            FramesStationaryFall = 0;
            HasCollision = false;
            CollisionPoint = Vector3.Zero;
            CollisionTime = 0;
            ObjectID = 0;
        }

        public void AddObject(CPhysicsObj obj, TransitionState ts)
        {
            if (!CollideObject.Contains(obj))
            {
                CollideObject.Add(obj);
                NumCollideObject = (uint)CollideObject.Count;
                if (ts != TransitionState.OK_TS)
                    LastCollidedObject = obj;
            }
        }

        public void SetSlidingNormal(Vector normal)
        {
            SlidingNormal = normal;
            SlidingNormalValid = 1;
        }
    }
} 