using System;
using System.Collections.Generic;
using System.Numerics; // Added for Vector3
using ACE.Server.Physics.Alt;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Represents a polygon for collision and walkability (ported from GDLE/PhatSDK Polygon).
    /// </summary>
    public class Polygon
    {
        public List<Vector> Vertices { get; set; } = new List<Vector>();
        public List<Edge> Edges { get; set; } = new List<Edge>();
        public Plane Plane;
        public int PolyId;
        public uint ID { get; set; } = 0; // Added for BSPTree compatibility
        public int NumPoints => Vertices.Count;

        // Global polygon array for BSP deserialization (1:1 with C++ pack_poly)
        public static List<Polygon> PackPolys { get; private set; } = new List<Polygon>();
        public static void SetPackPolys(List<Polygon> polys)
        {
            PackPolys = polys;
        }

        public Polygon(List<Vector> vertices, int polyId = -1)
        {
            Vertices = vertices;
            PolyId = polyId;
            MakePlane();
        }

        public void MakePlane()
        {
            // Calculate the plane from the vertices (assume convex polygon)
            if (Vertices.Count < 3)
                throw new InvalidOperationException("Polygon must have at least 3 vertices");

            Vector norm = new Vector(0, 0, 0);
            Vector p0 = Vertices[0];
            for (int i = 1; i < Vertices.Count - 1; i++)
            {
                Vector v1 = Vertices[i] - p0;
                Vector v2 = Vertices[i + 1] - p0;
                norm += v1.Cross(v2);
            }
            norm = norm.Normalize();
            float distsum = 0;
            foreach (var v in Vertices)
                distsum += norm.Dot(v);
            Plane = new Plane(norm, -(distsum / Vertices.Count));
        }

        public bool PointInPolygon(Vector point)
        {
            // Check if point is inside the polygon (projected onto the plane)
            Vector last = Vertices[NumPoints - 1];
            for (int i = 0; i < NumPoints; i++)
            {
                Vector curr = Vertices[i];
                Vector cross = Plane.Normal.Cross(curr - last);
                float dot = cross.Dot(point - last);
                if (dot < 0.0f)
                    return false;
                last = curr;
            }
            return true;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::point_in_poly2D
        public enum Sidedness { Negative = 0, Positive = 1 }

        public bool PointInPoly2D(Vector point, Sidedness side)
        {
            int prevVertexIndex = 0;
            for (int i = NumPoints - 1; i >= 0; i--)
            {
                Vector prevVertex = Vertices[prevVertexIndex];
                Vector currVertex = Vertices[i];

                float yOffset = prevVertex.Y - currVertex.Y;
                float xOffset = currVertex.X - prevVertex.X;

                float someVal = -(yOffset * currVertex.X) - (xOffset * currVertex.Y) + (xOffset * point.Y) + (yOffset * point.X);
                if (side == Sidedness.Positive)
                {
                    if (someVal < 0.0f)
                        return false;
                }
                else if (someVal > 0.0f)
                {
                    return false;
                }

                prevVertexIndex = i;
            }
            return true;
        }

        public bool IntersectsRay(Vector rayOrigin, Vector rayDir, out float t)
        {
            // Ported from C++ polygon_hits_ray
            t = 0;
            float denom = Vector3.Dot((Vector3)Plane.Normal, (Vector3)rayDir);
            if (Math.Abs(denom) < 1e-6f)
                return false;
            float numer = -(Vector3.Dot((Vector3)Plane.Normal, (Vector3)rayOrigin) + Plane.Distance);
            t = numer / denom;
            if (t < 0)
                return false;
            Vector intersection = rayOrigin + rayDir * t;
            return PointInPolygon(intersection);
        }

        public bool PolygonHitsSphere(Vector center, float radius, out Vector contactPoint)
        {
            // Ported from C++ polygon_hits_sphere
            float dist = Plane.Dot(center);
            float radEps = radius - 1e-6f;
            contactPoint = center - Plane.Normal * dist;
            if (radEps < Math.Abs(dist))
                return false;
            float someVal2 = radEps * radEps - dist * dist;
            Vector last = Vertices[NumPoints - 1];
            bool result = true;
            for (int i = 0; i < NumPoints; i++)
            {
                Vector curr = Vertices[i];
                Vector edge = curr - last;
                Vector disp = contactPoint - last;
                Vector cross = Plane.Normal.Cross(edge);
                float someNewDp = disp.Dot(cross);
                if (someNewDp < 0.0f)
                {
                    if ((cross.Dot(cross) * someVal2) < (someNewDp * someNewDp))
                        return false;
                    float dispEdgeDot = disp.Dot(edge);
                    if (dispEdgeDot >= 0.0f && dispEdgeDot <= edge.Dot(edge))
                        return true;
                    result = false;
                }
                if (disp.Dot(disp) <= someVal2)
                    return true;
                last = curr;
            }
            return result;
        }

        public bool WalkableHitsSphere(SPHEREPATH path, CSphere sphere, Vector up)
        {
            // Use the walkable sphere intersection method
            Vector contactPoint;
            bool hit = WalkableHitsSphereSlowButSure(sphere, out contactPoint);
            if (hit)
            {
                // Update path with walkable information
                path.WalkableZ = contactPoint.Z;
                path.HasWalkable = true;
            }
            return hit;
        }

        public bool AdjustSphereToPlane(SPHEREPATH path, Vector3 movement, Vector3 normal, float distance)
        {
            // Adjust sphere position based on plane collision
            var sphere = path.Spheres[0]; // Use first sphere
            var sphereToPlane = Vector3.Dot(normal, sphere.Center) - distance;
            
            if (Math.Abs(sphereToPlane) < 0.001f)
                return false; // Already on plane

            // Adjust sphere position to be on the plane
            var adjustedCenter = sphere.Center - normal * sphereToPlane;
            sphere.Center = adjustedCenter;
            
            return true;
        }

        public bool FindCrossedEdge(CSphere sphere, Vector up, out Vector normal)
        {
            normal = Vector.Zero;
            
            // Check each edge of the polygon
            for (int i = 0; i < NumPoints; i++)
            {
                Vector pPrevVertex = Vertices[i];
                Vector pCurrVertex = Vertices[(i + 1) % NumPoints];
                Vector edge = pCurrVertex - pPrevVertex;
                Vector disp = (Vector)sphere.Center - pPrevVertex;
                Vector cross = Plane.Normal.Cross(edge);
                if (disp.Dot(cross) < 0.0f)
                {
                    normal = cross.Normalize();
                    return true;
                }
            }
            return false;
        }

        public bool IsWalkableForNormal(Vector normal)
        {
            // Consider walkable if the polygon's plane normal is sufficiently upright
            return Plane.Normal.Z >= 0.866f;
        }

        public bool IsWalkableForObject(OBJECTINFO obj)
        {
            // Use the object's walkable criteria
            return obj.IsValidWalkable(Plane.Normal);
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::check_walkable
        public bool CheckWalkable(CSphere sphere, Vector up)
        {
            float dp = up.Dot(Plane.Normal);
            if (dp < 1e-6f)
                return false;

            bool result = true;
            Vector center = (Vector)((Vector3)sphere.Center - ((Vector3)(up * (Plane.Dot((Vector3)sphere.Center) / dp))));
            float radMag = sphere.Radius * sphere.Radius;

            int prevVertex = NumPoints - 1;
            for (int i = 0; i < NumPoints; i++)
            {
                Vector pPrevVertex = Vertices[prevVertex];
                prevVertex = i;
                Vector pCurrVertex = Vertices[i];

                Vector edge = pCurrVertex - pPrevVertex;
                Vector disp = center - pPrevVertex;
                Vector cross = Plane.Normal.Cross(edge);
                float someNewDp = disp.Dot(cross);
                if (someNewDp < 0.0f)
                {
                    if ((cross.Dot(cross) * radMag) < (someNewDp * someNewDp))
                        return false;

                    float dispEdgeDot = disp.Dot(edge);
                    if (dispEdgeDot >= 0.0f && dispEdgeDot <= edge.Dot(edge))
                        return true;

                    result = false;
                }
                if (disp.Dot(disp) <= radMag)
                    return true;
            }
            return result;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::check_small_walkable
        public bool CheckSmallWalkable(CSphere sphere, Vector up)
        {
            float dp = up.Dot(Plane.Normal);
            if (dp < 1e-6f)
                return false;

            bool result = true;
            Vector center = (Vector)((Vector3)sphere.Center - ((Vector3)(up * (Plane.Dot((Vector3)sphere.Center) / dp))));
            float radMag = sphere.Radius * sphere.Radius * 0.25f;

            int prevVertex = NumPoints - 1;
            for (int i = 0; i < NumPoints; i++)
            {
                Vector pPrevVertex = Vertices[prevVertex];
                prevVertex = i;
                Vector pCurrVertex = Vertices[i];

                Vector edge = pCurrVertex - pPrevVertex;
                Vector disp = center - pPrevVertex;
                Vector cross = Plane.Normal.Cross(edge);
                float someNewDp = disp.Dot(cross);
                if (someNewDp < 0.0f)
                {
                    if ((cross.Dot(cross) * radMag) < (someNewDp * someNewDp))
                        return false;

                    float dispEdgeDot = disp.Dot(edge);
                    if (dispEdgeDot >= 0.0f && dispEdgeDot <= edge.Dot(edge))
                        return true;

                    result = false;
                }
                if (disp.Dot(disp) <= radMag)
                    return true;
            }
            return result;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::pos_hits_sphere
        public bool PosHitsSphere(CSphere sphere, Vector movement, out Vector contactPoint, out Polygon struckPoly)
        {
            // Use the slow but sure method for polygon-sphere intersection
            bool hit = PolygonHitsSphereSlowButSure(sphere, out contactPoint);
            struckPoly = null;
            if (hit)
                struckPoly = this;
            if (movement.Dot(Plane.Normal) >= 0.0f)
                return false;
            return hit;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::polygon_hits_sphere_slow_but_sure
        public bool PolygonHitsSphereSlowButSure(CSphere sphere, out Vector contactPoint)
        {
            double dp = Plane.Dot((Vector3)sphere.Center);
            float radMag = (sphere.Radius - 1e-6f);
            contactPoint = (Vector)((Vector3)sphere.Center - (Plane.Normal * (float)dp));
            if (radMag >= Math.Abs(dp))
            {
                float someVal = (radMag * radMag) - (float)(dp * dp);
                if (NumPoints > 0)
                {
                    int prevVertex = NumPoints - 1;
                    for (int i = 0; i < NumPoints; i++)
                    {
                        Vector pPrevVertex = Vertices[prevVertex];
                        prevVertex = i;
                        Vector pCurrVertex = Vertices[i];
                        Vector voffset = pCurrVertex - pPrevVertex;
                        Vector cross = Plane.Normal.Cross(voffset);
                        double someDp = Vector3.Dot((Vector3)contactPoint - (Vector3)pPrevVertex, (Vector3)cross);
                        if (someDp < 0.0)
                        {
                            prevVertex = NumPoints - 1;
                            for (int j = 0; j < NumPoints; j++)
                            {
                                Vector pPrevVertex2 = Vertices[prevVertex];
                                prevVertex = j;
                                Vector pCurrVertex2 = Vertices[j];
                                Vector edge = pCurrVertex2 - pPrevVertex2;
                                Vector disp = contactPoint - pPrevVertex2;
                                cross = Plane.Normal.Cross(edge);
                                someDp = Vector3.Dot((Vector3)disp, (Vector3)cross);
                                if (someDp < 0.0)
                                {
                                    if ((cross.Dot(cross) * someVal) < (someDp * someDp))
                                        return false;
                                    double someOtherDp = Vector3.Dot((Vector3)disp, (Vector3)edge);
                                    if (someOtherDp >= 0.0 && someOtherDp <= edge.Dot(edge))
                                        return true;
                                }
                                if (disp.Dot(disp) <= someVal)
                                    return true;
                            }
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::polygon_hits_sphere_slow_but_sure (for walkable)
        public bool WalkableHitsSphereSlowButSure(CSphere sphere, out Vector contactPoint)
        {
            // This is a duplicate of PolygonHitsSphereSlowButSure, but kept for parity and future walkable-specific logic
            double dp = Plane.Dot((Vector3)sphere.Center);
            float radMag = (sphere.Radius - 1e-6f);
            contactPoint = (Vector)((Vector3)sphere.Center - (Plane.Normal * (float)dp));
            if (radMag >= Math.Abs(dp))
            {
                float someVal = (radMag * radMag) - (float)(dp * dp);
                if (NumPoints > 0)
                {
                    int prevVertex = NumPoints - 1;
                    for (int i = 0; i < NumPoints; i++)
                    {
                        Vector pPrevVertex = Vertices[prevVertex];
                        prevVertex = i;
                        Vector pCurrVertex = Vertices[i];
                        Vector voffset = pCurrVertex - pPrevVertex;
                        Vector cross = Plane.Normal.Cross(voffset);
                        double someDp = Vector3.Dot((Vector3)contactPoint - (Vector3)pPrevVertex, (Vector3)cross);
                        if (someDp < 0.0)
                        {
                            prevVertex = NumPoints - 1;
                            for (int j = 0; j < NumPoints; j++)
                            {
                                Vector pPrevVertex2 = Vertices[prevVertex];
                                prevVertex = j;
                                Vector pCurrVertex2 = Vertices[j];
                                Vector edge = pCurrVertex2 - pPrevVertex2;
                                Vector disp = contactPoint - pPrevVertex2;
                                cross = Plane.Normal.Cross(edge);
                                someDp = Vector3.Dot((Vector3)disp, (Vector3)cross);
                                if (someDp < 0.0)
                                {
                                    if ((cross.Dot(cross) * someVal) < (someDp * someDp))
                                        return false;
                                    double someOtherDp = Vector3.Dot((Vector3)disp, (Vector3)edge);
                                    if (someOtherDp >= 0.0 && someOtherDp <= edge.Dot(edge))
                                        return true;
                                }
                                if (disp.Dot(disp) <= someVal)
                                    return true;
                            }
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::adjust_sphere_to_poly
        public double AdjustSphereToPoly(CSphere checkPos, Vector currPos, Vector movement)
        {
            // UNFINISHED: need for collision
            // TODO: Implement full logic for collision adjustment
            return 0;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::adjust_to_placement_poly
        public void AdjustToPlacementPoly(CSphere struckSphere, CSphere otherSphere, float radius, int centerSolid, int solidCheck)
        {
            double dp = Plane.Dot((Vector3)struckSphere.Center);
            double v7;

            if (solidCheck != 0)
            {
                double rad = radius;
                if (centerSolid != 0)
                    rad *= -1.0;
                if (dp <= 0.0)
                    rad *= -1.0;
                v7 = rad - dp;
            }
            else
            {
                v7 = radius - dp;
            }

            Vector3 v = (Vector3)((Vector3)Plane.Normal * (float)v7);
            struckSphere.Center += v;
            otherSphere.Center += v;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::find_walkable (UNFINISHED stub)
        public void FindWalkable(SPHEREPATH path, CSphere validPos, out Polygon polygon, Vector movement, Vector up, out bool changed)
        {
            // UNFINISHED: need for walkable search
            // TODO: Implement full logic for walkable polygon search
            polygon = null;
            changed = false;
            return;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::hits_sphere
        public bool HitsSphere(CSphere sphere)
        {
            Vector contactPoint;
            return PolygonHitsSphereSlowButSure(sphere, out contactPoint);
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::polygon_hits_ray
        public bool PolygonHitsRay(Vector rayOrigin, Vector rayDir, out float t)
        {
            t = 0;
            // If the polygon is single-sided and the ray is on the wrong side, return false
            // (Assume single-sided for now; extend as needed)
            if (Plane.Normal.Dot(rayDir) > 0)
                return false;

            // Compute intersection time with the plane
            float denom = Plane.Normal.Dot(rayDir);
            if (Math.Abs(denom) < 1e-6f)
                return false;
            float numer = -(Plane.Normal.Dot(rayOrigin) + Plane.Distance);
            t = numer / denom;
            if (t < 0)
                return false;
            Vector intersection = rayOrigin + rayDir * t;
            return PointInPolygon(intersection);
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::peafixed_polygon_hits_ray
        public bool PeafixedPolygonHitsRay(Vector rayOrigin, Vector rayDir, out float depth)
        {
            depth = float.MaxValue;
            bool hit = false;
            // For each triangle in the polygon (fan triangulation)
            for (int i = 1; i < NumPoints - 1; )
            {
                Vector v0 = Vertices[0];
                Vector v1 = Vertices[i];
                Vector v2 = Vertices[++i];
                float u, v, t;
                if (RayIntersectsTriangle(rayOrigin, rayDir, v0, v1, v2, out t, out u, out v))
                {
                    if (t < depth)
                    {
                        depth = t;
                        hit = true;
                    }
                }
            }
            return hit;
        }

        // Möller–Trumbore ray-triangle intersection
        private bool RayIntersectsTriangle(Vector orig, Vector dir, Vector v0, Vector v1, Vector v2, out float t, out float u, out float v)
        {
            t = u = v = 0;
            Vector edge1 = v1 - v0;
            Vector edge2 = v2 - v0;
            Vector pvec = dir.Cross(edge2);
            float det = edge1.Dot(pvec);
            if (Math.Abs(det) < 1e-6f) return false;
            float invDet = 1.0f / det;
            Vector tvec = orig - v0;
            u = tvec.Dot(pvec) * invDet;
            if (u < 0 || u > 1) return false;
            Vector qvec = tvec.Cross(edge1);
            v = dir.Dot(qvec) * invDet;
            if (v < 0 || u + v > 1) return false;
            t = edge2.Dot(qvec) * invDet;
            return t > 1e-6f;
        }

        // 1:1 port from GDLE/PhatSDK CPolygon::is_valid_walkable (static)
        public static bool IsValidWalkable(Vector normal)
        {
            // Consider walkable if the normal's Z is sufficiently upright
            return normal.Z >= 0.866f;
        }

        public static void LoadPolysFromDat(string filePath)
        {
            var polys = new List<Polygon>();
            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            using (var br = new System.IO.BinaryReader(fs))
            {
                int numPolys = br.ReadInt32();
                for (int i = 0; i < numPolys; i++)
                {
                    int numVerts = br.ReadInt32();
                    var verts = new List<Vector>(numVerts);
                    for (int v = 0; v < numVerts; v++)
                    {
                        float x = br.ReadSingle();
                        float y = br.ReadSingle();
                        float z = br.ReadSingle();
                        verts.Add(new Vector(x, y, z));
                    }
                    int polyId = br.ReadInt32();
                    polys.Add(new Polygon(verts, polyId));
                }
            }
            SetPackPolys(polys);
        }

        // TODO: Add more methods as needed (walkable checks, ray intersection, etc.)
        // Already implemented: PosHitsSphere
        // Next: Implement UnPack if not present
        // 1:1 port from GDLE/PhatSDK CPolygon::UnPack (stub)
        public bool UnPack(byte[] data, int size)
        {
            // TODO: Implement polygon unpacking from binary data if needed
            return false;
        }

        public bool ContainsPoint(Vector3 point)
        {
            if (Vertices.Count < 3)
                return false;

            // Use ray casting algorithm to determine if point is inside polygon
            var ray = new Vector3(1, 0, 0); // Ray pointing in X direction
            var intersections = 0;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var current = (Vector3)Vertices[i];
                var next = (Vector3)Vertices[(i + 1) % Vertices.Count];

                // Check if ray intersects with edge
                if (RayIntersectsEdge(point, ray, current, next))
                {
                    intersections++;
                }
            }

            // Point is inside if number of intersections is odd
            return (intersections % 2) == 1;
        }

        private bool RayIntersectsEdge(Vector3 rayOrigin, Vector3 rayDirection, Vector3 edgeStart, Vector3 edgeEnd)
        {
            // Project to 2D (XZ plane) for simplicity
            var rayOrigin2D = new Vector2(rayOrigin.X, rayOrigin.Z);
            var rayDirection2D = new Vector2(rayDirection.X, rayDirection.Z);
            var edgeStart2D = new Vector2(edgeStart.X, edgeStart.Z);
            var edgeEnd2D = new Vector2(edgeEnd.X, edgeEnd.Z);

            // Check if ray intersects with edge in 2D
            var edgeVector = edgeEnd2D - edgeStart2D;
            var rayToEdge = edgeStart2D - rayOrigin2D;

            var cross1 = Vector2.Cross(rayDirection2D, edgeVector);
            var cross2 = Vector2.Cross(rayToEdge, rayDirection2D);

            if (Math.Abs(cross1) < 0.0001f)
                return false; // Parallel lines

            var t1 = Vector2.Cross(rayToEdge, edgeVector) / cross1;
            var t2 = cross2 / cross1;

            return t1 >= 0 && t2 >= 0 && t2 <= 1;
        }

        public bool IntersectsSphere(CSphere sphere)
        {
            // Check if sphere intersects with polygon plane
            var plane = Plane;
            var sphereToPlane = Vector3.Dot(plane.Normal, sphere.Center) - plane.Distance;
            
            if (Math.Abs(sphereToPlane) > sphere.Radius)
                return false; // Sphere doesn't intersect plane

            // Project sphere center onto polygon plane
            var projectedCenter = (Vector3)sphere.Center - ((Vector3)plane.Normal * sphereToPlane);
            
            // Check if projected point is inside polygon
            if (ContainsPoint(projectedCenter))
                return true;

            // Check if sphere intersects with any polygon edge
            for (int i = 0; i < Vertices.Count; i++)
            {
                var current = (Vector3)Vertices[i];
                var next = (Vector3)Vertices[(i + 1) % Vertices.Count];
                
                if (SphereIntersectsEdge(sphere, current, next))
                    return true;
            }

            return false;
        }

        private bool SphereIntersectsEdge(CSphere sphere, Vector3 edgeStart, Vector3 edgeEnd)
        {
            var edgeVector = edgeEnd - edgeStart;
            var sphereToEdge = sphere.Center - edgeStart;
            
            var edgeLengthSquared = edgeVector.LengthSquared();
            if (edgeLengthSquared < 0.0001f)
                return false;

            // Project sphere center onto edge
            var projection = Vector3.Dot(sphereToEdge, edgeVector) / edgeLengthSquared;
            projection = Math.Max(0, Math.Min(1, projection)); // Clamp to edge

            var closestPoint = edgeStart + edgeVector * projection;
            var distanceSquared = Vector3.DistanceSquared(sphere.Center, closestPoint);
            
            return distanceSquared <= sphere.Radius * sphere.Radius;
        }

        public Vector3 GetClosestPoint(Vector3 point)
        {
            // First, project point onto polygon plane
            var plane = Plane;
            var pointToPlane = Vector3.Dot(plane.Normal, point) - plane.Distance;
            var projectedPoint = point - plane.Normal * pointToPlane;

            // Check if projected point is inside polygon
            if (ContainsPoint(projectedPoint))
                return projectedPoint;

            // Find closest point on polygon boundary
            var closestPoint = projectedPoint;
            var minDistance = float.MaxValue;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var current = (Vector3)Vertices[i];
                var next = (Vector3)Vertices[(i + 1) % Vertices.Count];
                
                var edgeClosestPoint = GetClosestPointOnEdge(projectedPoint, current, next);
                var distance = Vector3.DistanceSquared(projectedPoint, edgeClosestPoint);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = edgeClosestPoint;
                }
            }

            return closestPoint;
        }

        private Vector3 GetClosestPointOnEdge(Vector3 point, Vector3 edgeStart, Vector3 edgeEnd)
        {
            var edgeVector = edgeEnd - edgeStart;
            var pointToEdge = point - edgeStart;
            
            var edgeLengthSquared = edgeVector.LengthSquared();
            if (edgeLengthSquared < 0.0001f)
                return edgeStart;

            // Project point onto edge
            var projection = Vector3.Dot(pointToEdge, edgeVector) / edgeLengthSquared;
            projection = Math.Max(0, Math.Min(1, projection)); // Clamp to edge

            return edgeStart + edgeVector * projection;
        }

        public bool IsWalkable
        {
            get
            {
                // A polygon is walkable if its normal is mostly upward
                var plane = Plane;
                var upVector = new Vector3(0, 0, 1);
                var dotProduct = Vector3.Dot(plane.Normal, upVector);
                
                // Consider walkable if angle is less than 45 degrees
                return dotProduct > 0.707f; // cos(45°) ≈ 0.707
            }
        }

        public float GetWalkableZ()
        {
            if (!IsWalkable)
                return float.MinValue;

            // Return the Z coordinate of the walkable surface
            return Plane.Distance;
        }

        public bool AdjustSphereToPlane(CSphere sphere, out Vector3 adjustedPosition)
        {
            adjustedPosition = sphere.Center;
            
            var plane = Plane;
            var sphereToPlane = Vector3.Dot(plane.Normal, sphere.Center) - plane.Distance;
            
            if (Math.Abs(sphereToPlane) < 0.001f)
                return true; // Already on plane

            // Adjust sphere position to be on the plane
            adjustedPosition = sphere.Center - plane.Normal * sphereToPlane;
            
            // Check if adjusted position is inside polygon
            return ContainsPoint(adjustedPosition);
        }

        public bool FindWalkableIntersection(Vector3 startPoint, Vector3 endPoint, out Vector3 intersectionPoint)
        {
            intersectionPoint = endPoint;
            
            if (!IsWalkable)
                return false;

            // Check if movement line intersects with polygon
            var movement = endPoint - startPoint;
            var plane = Plane;
            
            var movementDotNormal = Vector3.Dot(movement, plane.Normal);
            if (Math.Abs(movementDotNormal) < 0.0001f)
                return false; // Moving parallel to plane

            var startToPlane = Vector3.Dot(plane.Normal, startPoint) - plane.Distance;
            var intersectionTime = -startToPlane / movementDotNormal;
            
            if (intersectionTime < 0 || intersectionTime > 1)
                return false; // Intersection outside movement range

            intersectionPoint = startPoint + movement * intersectionTime;
            
            // Check if intersection point is inside polygon
            return ContainsPoint(intersectionPoint);
        }

        public Edge FindCrossedEdge(Vector3 startPoint, Vector3 endPoint)
        {
            var movement = endPoint - startPoint;
            
            for (int i = 0; i < Edges.Count; i++)
            {
                var edge = Edges[i];
                var edgeVector = (Vector3)edge.End - (Vector3)edge.Start;
                
                // Check if movement crosses this edge
                var cross1 = Vector3.Cross(movement, edgeVector);
                var cross2 = Vector3.Cross(startPoint - (Vector3)edge.Start, edgeVector);
                
                if (Math.Abs(cross1.LengthSquared()) < 0.0001f)
                    continue; // Parallel lines

                var t1 = Vector3.Cross(startPoint - (Vector3)edge.Start, edgeVector).Length() / cross1.Length();
                var t2 = cross2.Length() / cross1.Length();

                if (t1 >= 0 && t2 >= 0 && t2 <= 1)
                {
                    return edge;
                }
            }

            return null;
        }

        public bool IsConvex()
        {
            if (Vertices.Count < 3)
                return false;

            var firstCross = Vector3.Cross((Vector3)Vertices[1] - (Vector3)Vertices[0], (Vector3)Vertices[2] - (Vector3)Vertices[0]);
            var firstSign = Math.Sign(Vector3.Dot(firstCross, (Vector3)Plane.Normal));

            for (int i = 1; i < Vertices.Count; i++)
            {
                var current = Vertices[i];
                var next = Vertices[(i + 1) % Vertices.Count];
                var nextNext = Vertices[(i + 2) % Vertices.Count];
                
                var cross = Vector3.Cross((Vector3)next - (Vector3)current, (Vector3)nextNext - (Vector3)current);
                var sign = Math.Sign(Vector3.Dot(cross, (Vector3)Plane.Normal));
                
                if (sign != firstSign)
                    return false;
            }

            return true;
        }

        public BoundingBox GetBoundingBox()
        {
            if (Vertices.Count == 0)
                return new BoundingBox();

            var min = (Vector3)Vertices[0];
            var max = (Vector3)Vertices[0];

            for (int i = 1; i < Vertices.Count; i++)
            {
                var vertex = (Vector3)Vertices[i];
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            var boundingBox = new BoundingBox();
            boundingBox.Min = min;
            boundingBox.Max = max;
            boundingBox.CalcSize();
            return boundingBox;
        }

        public float GetArea()
        {
            if (Vertices.Count < 3)
                return 0;

            var area = 0.0f;
            for (int i = 0; i < Vertices.Count; i++)
            {
                var current = Vertices[i];
                var next = Vertices[(i + 1) % Vertices.Count];
                
                area += current.X * next.Z - next.X * current.Z;
            }

            return Math.Abs(area) * 0.5f;
        }

        public Vector3 GetCentroid()
        {
            if (Vertices.Count == 0)
                return Vector3.Zero;

            var centroid = Vector3.Zero;
            foreach (var vertex in Vertices)
            {
                centroid += (Vector3)vertex;
            }

            return centroid / Vertices.Count;
        }
    }
} 