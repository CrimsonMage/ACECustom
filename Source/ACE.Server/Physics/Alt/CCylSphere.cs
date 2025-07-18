using System;
using System.Numerics;
using ACE.Server.Physics.Alt;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Physics cylinder-sphere primitive (ported from GDLE/PhatSDK CCylSphere).
    /// </summary>
    public class CCylSphere
    {
        public Vector LowPt;
        public float Height;
        public float Radius;

        public CCylSphere()
        {
            LowPt = new Vector(0, 0, 0);
            Height = 0.0f;
            Radius = 0.0f;
        }

        // TODO: Implement serialization methods
        public int PackSize()
        {
            // 3 floats for LowPt (X, Y, Z) + 1 float for Height + 1 float for Radius = 20 bytes
            return 20;
        }
        public bool UnPack(byte[] data, int size)
        {
            // Unpack 3 floats for LowPt, 1 float for Height, 1 float for Radius
            if (data == null || size < 20)
                return false;
            LowPt = new Vector(
                BitConverter.ToSingle(data, 0),
                BitConverter.ToSingle(data, 4),
                BitConverter.ToSingle(data, 8));
            Height = BitConverter.ToSingle(data, 12);
            Radius = BitConverter.ToSingle(data, 16);
            return true;
        }

        public bool Pack(byte[] data, int offset)
        {
            if (data == null || data.Length < offset + 20)
                return false;
            Buffer.BlockCopy(BitConverter.GetBytes(LowPt.X), 0, data, offset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(LowPt.Y), 0, data, offset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(LowPt.Z), 0, data, offset + 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Height), 0, data, offset + 12, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Radius), 0, data, offset + 16, 4);
            return true;
        }

        // TODO: Implement collision/transition methods
        public TransitionState IntersectsSphere(CTransition transition)
        {
            // Ported from C++ CCylSphere::intersects_sphere(CTransition*)
            var path = transition.SpherePath;
            var sphere = path.GlobalSphere[0];
            Vector disp = (Vector)(sphere.Center - (Vector3)LowPt);
            float radsum = (Radius + sphere.Radius) - 1e-6f; // F_EPSILON

            // Ethereal or placement insert
            if (path.ObstructionEthereal != 0 || path.Insert_Type == SPHEREPATH.InsertType.PLACEMENT_INSERT)
            {
                if (CollidesWithSphere((CSphere)(object)sphere, (Vector)(sphere.Center - (Vector3)LowPt), radsum))
                    return TransitionState.COLLIDED_TS;
                if (path.NumSphere >= 2)
                {
                    var sphere1 = path.GlobalSphere[1];
                    Vector disp2 = (Vector)(sphere1.Center - (Vector3)LowPt);
                    if (CollidesWithSphere((CSphere)(object)sphere1, (Vector)(sphere1.Center - (Vector3)LowPt), radsum))
                        return TransitionState.COLLIDED_TS;
                }
                return TransitionState.OK_TS;
            }
            // Step down
            else if (path.StepDown != 0)
            {
                // TODO: If this is a creature, return OK_TS (not yet implemented)
                return StepSphereDown(transition.ObjectInfo, path, transition.CollisionInfo, sphere, disp, radsum);
            }
            // Walkable check
            else if (path.CheckWalkable != 0)
            {
                if (CollidesWithSphere((CSphere)(object)sphere, (Vector)(sphere.Center - (Vector3)LowPt), radsum))
                    return TransitionState.COLLIDED_TS;
                if (path.NumSphere >= 2)
                {
                    var sphere1 = path.GlobalSphere[1];
                    Vector disp2 = (Vector)(sphere1.Center - (Vector3)LowPt);
                    if (CollidesWithSphere((CSphere)(object)sphere1, (Vector)(sphere1.Center - (Vector3)LowPt), radsum))
                        return TransitionState.COLLIDED_TS;
                }
                return TransitionState.OK_TS;
            }
            // Advanced collision logic
            else
            {
                // CONTACT_OI or ON_WALKABLE_OI
                if ((transition.ObjectInfo.State & ((int)OBJECTINFO.ObjectInfoEnum.CONTACT_OI | (int)OBJECTINFO.ObjectInfoEnum.ON_WALKABLE_OI)) != 0)
                {
                    if (CollidesWithSphere((CSphere)(object)sphere, (Vector)(sphere.Center - (Vector3)LowPt), radsum))
                    {
                        return StepSphereUp(transition, sphere, disp, radsum);
                    }
                    if (path.NumSphere >= 2)
                    {
                        var sphere1 = path.GlobalSphere[1];
                        Vector disp2 = (Vector)(sphere1.Center - (Vector3)LowPt);
                        if (CollidesWithSphere((CSphere)(object)sphere1, (Vector)(sphere1.Center - (Vector3)LowPt), radsum))
                        {
                            return SlideSphere(transition.ObjectInfo, path, transition.CollisionInfo, sphere1, disp2, radsum, 1);
                        }
                    }
                    return TransitionState.OK_TS;
                }
                // PATH_CLIPPED_OI
                else if ((transition.ObjectInfo.State & (int)OBJECTINFO.ObjectInfoEnum.PATH_CLIPPED_OI) != 0)
                {
                    if (CollidesWithSphere((CSphere)(object)sphere, (Vector)(sphere.Center - (Vector3)LowPt), radsum))
                    {
                        return CollideWithPoint(transition.ObjectInfo, path, transition.CollisionInfo, sphere, disp, radsum, 0);
                    }
                    return TransitionState.OK_TS;
                }
                // Default: land or collide
                else
                {
                    if (CollidesWithSphere((CSphere)(object)sphere, (Vector)(sphere.Center - (Vector3)LowPt), radsum))
                    {
                        return LandOnCylinder(transition.ObjectInfo, path, transition.CollisionInfo, sphere, disp, radsum);
                    }
                    else if (path.NumSphere >= 2)
                    {
                        var sphere1 = path.GlobalSphere[1];
                        Vector disp2 = (Vector)(sphere1.Center - (Vector3)LowPt);
                        if (CollidesWithSphere((CSphere)(object)sphere1, (Vector)(sphere1.Center - (Vector3)LowPt), radsum))
                        {
                            return CollideWithPoint(transition.ObjectInfo, path, transition.CollisionInfo, sphere1, disp2, radsum, 1);
                        }
                    }
                    return TransitionState.OK_TS;
                }
            }
        }
        // TODO: Port logic from C++ CCylSphere::intersects_sphere(Position*, float, CTransition*)
        public TransitionState IntersectsSphere(IntPtr position, float scale, CTransition transition)
        {
            // Ported from C++ CCylSphere::intersects_sphere(Position*, float, CTransition*)
            // This method would check for intersection between this CCylSphere and a position (with scale) in the context of a transition.
            // Since Position is opaque, we assume position points to a CSphere or similar structure for now.
            // If not, this should be replaced with the correct logic once Position is ported.

            // Placeholder: Try to get a CSphere from the IntPtr (not implemented)
            // In real code, would need to marshal or cast IntPtr to the correct type
            // For now, throw NotImplementedException
            throw new NotImplementedException("IntersectsSphere(Position*, float, CTransition*) logic not yet ported: Position type is opaque in C#");
        }
        // 1:1 port from GDLE/PhatSDK CCylSphere::collides_with_sphere
        public bool CollidesWithSphere(CSphere checkPos, Vector disp, float radsum)
        {
            // Returns true if the 2D (XY) distance and Z overlap conditions are met
            float xyDistSq = disp.X * disp.X + disp.Y * disp.Y;
            float radsumSq = radsum * radsum;
            if (radsumSq < xyDistSq)
                return false;
            float zHalf = Height * 0.5f;
            float zDelta = Math.Abs(zHalf - disp.Z);
            if ((checkPos.Radius - 1e-6f + zHalf) >= zDelta)
                return true;
            return false;
        }
        public TransitionState LandOnCylinder(
            OBJECTINFO objectInfo,
            SPHEREPATH path,
            ACE.Server.Physics.Alt.CollisionInfo collisions,
            CSphere checkPos,
            Vector disp,
            float radsum)
        {
            // Ported from C++ CCylSphere::land_on_cylinder
            Vector collisionNormal = new Vector();
            int definite = NormalOfCollision(path, checkPos, disp, radsum, 0, ref collisionNormal);
            // If the collision normal is small (zero vector), return COLLIDED_TS
            if (collisionNormal.X == 0 && collisionNormal.Y == 0 && collisionNormal.Z == 0)
                return TransitionState.COLLIDED_TS;
            // Set collision state and walkable allowance
            path.SetCollide(collisionNormal);
            path.WalkableAllowance = 0.0871557f; // z_for_landing_2
            return TransitionState.ADJUSTED_TS;
        }

        public int NormalOfCollision(
            SPHEREPATH path,
            CSphere checkPos,
            Vector disp,
            float radsum,
            int sphereNum,
            ref Vector normal)
        {
            // Ported from C++ CCylSphere::normal_of_collision
            Vector oldDisp = (Vector)(path.GlobalCurrCenter[sphereNum] - (Vector3)LowPt);

            if ((radsum * radsum) < (oldDisp.X * oldDisp.X + oldDisp.Y * oldDisp.Y))
            {
                normal = new Vector(oldDisp.X, oldDisp.Y, 0.0f);
                if ((checkPos.Radius - 1e-6f + (Height * 0.5f)) >= Math.Abs(Height * 0.5f - oldDisp.Z) || Math.Abs(oldDisp.Z - disp.Z) <= 1e-6f)
                {
                    return 1;
                }
                return 0;
            }

            if ((disp.Z - oldDisp.Z) <= 0.0)
            {
                normal = new Vector(0, 0, 1);
                return 1;
            }

            normal = new Vector(0, 0, -1);
            return 1;
        }

        public TransitionState StepSphereDown(
            OBJECTINFO objectInfo,
            SPHEREPATH path,
            ACE.Server.Physics.Alt.CollisionInfo collisions,
            CSphere checkPos,
            Vector disp,
            float radsum)
        {
            // Ported from C++ CCylSphere::step_sphere_down
            if (!CollidesWithSphere(checkPos, disp, radsum))
            {
                if (path.NumSphere <= 1)
                    return TransitionState.OK_TS;

                var sphere1 = path.GlobalSphere[1];
                Vector disp2 = (Vector)(sphere1.Center - (Vector3)LowPt);
                if (!CollidesWithSphere(sphere1, disp2, radsum))
                    return TransitionState.OK_TS;
            }

            float stepDownInterp = path.StepDownAmt * path.WalkInterp;
            if (Math.Abs(stepDownInterp) < 1e-6f) // F_EPSILON
                return TransitionState.COLLIDED_TS;

            double v12 = Height + checkPos.Radius - disp.Z;
            double v13 = (1.0 - v12 / stepDownInterp) * path.WalkInterp;

            if (v13 >= path.WalkInterp || v13 < -0.1)
                return TransitionState.COLLIDED_TS;

            Vector contactPt = new Vector(checkPos.Center.X, checkPos.Center.Y, checkPos.Center.Z + (float)(v12 - checkPos.Radius));
            Vector normal = new Vector(0, 0, 1.0f);

            // Create the contact plane at the point of contact with the normal
            Plane contactPlane = new Plane(normal, contactPt);
            // Set the contact plane in the collisions object (assuming SetContactPlane exists)
            collisions.SetContactPlane(contactPlane, 1);

            // Set the contact plane cell ID if available
            // TODO: This assumes path.CheckPos has an ObjcellId property; if not, stub or document
            // collisions.ContactPlaneCellId = path.CheckPos.ObjcellId;
            // For now, if not available, leave as a stub
            path.WalkInterp = (float)v13;
            normal = new Vector(0, 0, (float)v12);
            // Adjust the check position by the normal and checkPos radius
            path.AddOffsetToCheckPos(normal, checkPos.Radius);

            return TransitionState.ADJUSTED_TS;
        }

        public TransitionState StepSphereUp(
            CTransition transition,
            CSphere checkPos,
            Vector disp,
            float radsum)
        {
            // Ported from C++ CCylSphere::step_sphere_up
            if (transition.ObjectInfo.StepUpHeight < (checkPos.Radius + Height - disp.Z))
            {
                // Call SlideSphere for this case
                return SlideSphere(
                    transition.ObjectInfo,
                    transition.SpherePath,
                    transition.CollisionInfo,
                    checkPos,
                    disp,
                    radsum,
                    0);
            }
            else
            {
                // Compute the collision normal using NormalOfCollision
                Vector collisionNormal = new Vector();
                int definite = NormalOfCollision(transition.SpherePath, checkPos, disp, radsum, 0, ref collisionNormal);
                // Use the collisionNormal in the subsequent logic

                // Transform the collision normal to global coordinates if needed
                // TODO: Implement localtoglobalvec transformation if required by the original logic
                // For now, use collisionNormal as-is

                // TODO: Use localtoglobalvec if needed
                // Attempt to step up using the collision normal
                if (transition.StepUp)
                    return TransitionState.OK_TS;
                else
                    return transition.SpherePath.StepUpSlide(transition.ObjectInfo, transition.CollisionInfo);
            }
        }

        public TransitionState SlideSphere(
            OBJECTINFO objectInfo,
            SPHEREPATH path,
            ACE.Server.Physics.Alt.CollisionInfo collisions,
            CSphere checkPos,
            Vector disp,
            float radsum,
            int sphereNum)
        {
            // Ported from C++ CCylSphere::slide_sphere
            Vector collisionNormal = new Vector();
            int definite = NormalOfCollision(path, checkPos, disp, radsum, sphereNum, ref collisionNormal);
            // Check if the collision normal is too small (zero or near-zero vector)
            if (NormalizeCheckSmall(collisionNormal))
                return TransitionState.COLLIDED_TS;

            // Call CSphere.SlideSphere
            return checkPos.SlideSphere(path, collisions, collisionNormal);
        }

        public TransitionState CollideWithPoint(
            OBJECTINFO objectInfo,
            SPHEREPATH path,
            ACE.Server.Physics.Alt.CollisionInfo collisions,
            CSphere checkPos,
            Vector disp,
            float radsum,
            int sphereNum)
        {
            // Ported from C++ CCylSphere::collide_with_point
            Vector collisionNormal = new Vector();
            int definite = NormalOfCollision(path, checkPos, disp, radsum, sphereNum, ref collisionNormal);
            // Check if the collision normal is too small (zero or near-zero vector)
            if (NormalizeCheckSmall(collisionNormal))
                return TransitionState.COLLIDED_TS;
            if ((objectInfo.State & (int)OBJECTINFO.ObjectInfoEnum.PERFECT_CLIP_OI) == 0)
            {
                collisions.SetCollisionNormal(collisionNormal);
                return TransitionState.COLLIDED_TS;
            }
            // Advanced movement/collision logic for PERFECT_CLIP_OI
            float radsuma = radsum + 1e-6f; // F_EPSILON
            Vector movement = (Vector)(checkPos.Center - (Vector3)path.GlobalCurrCenter[0]);
            Vector oldDisp2 = (Vector)(path.GlobalCurrCenter[0] - (Vector3)LowPt);
            double coltime = checkPos.FindTimeOfCollision(movement, oldDisp2, radsuma);
            if (coltime < 1e-6f || coltime > 1.0f)
            {
                return TransitionState.COLLIDED_TS;
            }
            else
            {
                Vector offset = (movement * (float)coltime) - movement;
                Vector newCollisionNormal = ((offset + (Vector)checkPos.Center) - (Vector3)LowPt) * (1.0f / radsum);
                collisions.SetCollisionNormal(newCollisionNormal);
                // Adjust the check position by the computed offset and checkPos radius
                path.AddOffsetToCheckPos(offset, checkPos.Radius);
                return TransitionState.ADJUSTED_TS;
            }
        }

        // Helper method for checking if a vector is too small (magnitude near zero)
        private bool NormalizeCheckSmall(Vector v)
        {
            // Ported from C++: returns true if the vector is too small to be a valid normal
            const float epsilon = 1e-6f;
            return v.Magnitude() < epsilon;
        }
    }
} 