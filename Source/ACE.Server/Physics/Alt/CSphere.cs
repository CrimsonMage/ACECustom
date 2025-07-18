using System;
using System.Numerics;
using ACE.Entity;
using System.Collections.Generic;
using System.Linq;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Simplified CSphere for GDLE physics integration
    /// This is a placeholder implementation that will be gradually expanded
    /// </summary>
    public class CSphere
    {
        // Core properties
        public Vector3 Center { get; set; }
        public float Radius { get; set; }
        
        // Constants
        public const float DEFAULT_RADIUS = 1.0f;
        public const float EPSILON = 0.001f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CSphere()
        {
            InitializeDefaults();
        }

        /// <summary>
        /// Constructor with center and radius
        /// </summary>
        public CSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = Math.Max(radius, EPSILON);
        }

        /// <summary>
        /// Initialize default values
        /// </summary>
        private void InitializeDefaults()
        {
            Center = Vector3.Zero;
            Radius = DEFAULT_RADIUS;
        }

        /// <summary>
        /// Check if sphere intersects with another sphere
        /// </summary>
        public bool Intersects(CSphere other)
        {
            if (other == null)
                return false;

            float distanceSquared = Vector3.DistanceSquared(Center, other.Center);
            float radiusSum = Radius + other.Radius;
            return distanceSquared <= radiusSum * radiusSum;
        }

        /// <summary>
        /// Check if sphere contains point
        /// </summary>
        public bool Contains(Vector3 point)
        {
            float distanceSquared = Vector3.DistanceSquared(Center, point);
            return distanceSquared <= Radius * Radius;
        }

        /// <summary>
        /// Check if sphere intersects with plane
        /// </summary>
        public bool IntersectsPlane(Vector3 planeNormal, float planeDistance)
        {
            float distance = Vector3.Dot(Center, planeNormal) - planeDistance;
            return Math.Abs(distance) <= Radius;
        }

        /// <summary>
        /// Get sphere volume
        /// </summary>
        public float GetVolume()
        {
            return (4.0f / 3.0f) * (float)Math.PI * Radius * Radius * Radius;
        }

        /// <summary>
        /// Get sphere surface area
        /// </summary>
        public float GetSurfaceArea()
        {
            return 4.0f * (float)Math.PI * Radius * Radius;
        }

        /// <summary>
        /// Get bounding box
        /// </summary>
        public BoundingBox GetBoundingBox()
        {
            Vector3 min = Center - new Vector3(Radius);
            Vector3 max = Center + new Vector3(Radius);
            return new BoundingBox { Min = min, Max = max };
        }

        /// <summary>
        /// Transform sphere by matrix
        /// </summary>
        public CSphere Transform(Matrix4x4 matrix)
        {
            Vector3 transformedCenter = Vector3.Transform(Center, matrix);
            
            // For uniform scaling, we can use the scale factor
            // For non-uniform scaling, we need to compute the maximum scale
            float scaleX = new Vector3(matrix.M11, matrix.M12, matrix.M13).Length();
            float scaleY = new Vector3(matrix.M21, matrix.M22, matrix.M23).Length();
            float scaleZ = new Vector3(matrix.M31, matrix.M32, matrix.M33).Length();
            float maxScale = Math.Max(Math.Max(scaleX, scaleY), scaleZ);
            
            float transformedRadius = Radius * maxScale;
            
            return new CSphere(transformedCenter, transformedRadius);
        }

        /// <summary>
        /// Get distance to point
        /// </summary>
        public float GetDistanceToPoint(Vector3 point)
        {
            float distance = Vector3.Distance(Center, point);
            return Math.Max(0, distance - Radius);
        }

        /// <summary>
        /// Get closest point on sphere to given point
        /// </summary>
        public Vector3 GetClosestPoint(Vector3 point)
        {
            Vector3 direction = point - Center;
            float distance = direction.Length();
            
            if (distance < EPSILON)
                return Center + new Vector3(Radius, 0, 0);
            
            direction = Vector3.Normalize(direction);
            return Center + direction * Radius;
        }

        /// <summary>
        /// Check if sphere intersects with ray
        /// </summary>
        public bool IntersectsRay(Vector3 rayOrigin, Vector3 rayDirection)
        {
            Vector3 toCenter = Center - rayOrigin;
            float projection = Vector3.Dot(toCenter, rayDirection);
            
            if (projection < 0)
                return toCenter.LengthSquared() <= Radius * Radius;
            
            Vector3 closestPoint = rayOrigin + rayDirection * projection;
            return Vector3.DistanceSquared(closestPoint, Center) <= Radius * Radius;
        }

        /// <summary>
        /// Get intersection with ray
        /// </summary>
        public bool GetRayIntersection(Vector3 rayOrigin, Vector3 rayDirection, out float t1, out float t2)
        {
            t1 = t2 = 0;
            
            Vector3 toCenter = Center - rayOrigin;
            float projection = Vector3.Dot(toCenter, rayDirection);
            Vector3 closestPoint = rayOrigin + rayDirection * projection;
            
            float distanceSquared = Vector3.DistanceSquared(closestPoint, Center);
            float radiusSquared = Radius * Radius;
            
            if (distanceSquared > radiusSquared)
                return false;
            
            float halfChord = (float)Math.Sqrt(radiusSquared - distanceSquared);
            t1 = projection - halfChord;
            t2 = projection + halfChord;
            
            return true;
        }

        /// <summary>
        /// Expand sphere to include point
        /// </summary>
        public void ExpandToInclude(Vector3 point)
        {
            float distanceSquared = Vector3.DistanceSquared(Center, point);
            float currentRadiusSquared = Radius * Radius;
            
            if (distanceSquared > currentRadiusSquared)
            {
                float distance = (float)Math.Sqrt(distanceSquared);
                Radius = distance;
            }
        }

        /// <summary>
        /// Expand sphere to include another sphere
        /// </summary>
        public void ExpandToInclude(CSphere other)
        {
            if (other == null)
                return;
                
            float distance = Vector3.Distance(Center, other.Center);
            float newRadius = distance + other.Radius;
            
            if (newRadius > Radius)
            {
                Vector3 direction = Vector3.Normalize(other.Center - Center);
                Center = other.Center - direction * other.Radius;
                Radius = newRadius;
            }
        }

        /// <summary>
        /// Get sphere from points
        /// </summary>
        public static CSphere FromPoints(Vector3[] points)
        {
            if (points == null || points.Length == 0)
                return new CSphere();
            
            if (points.Length == 1)
                return new CSphere(points[0], EPSILON);
            
            // Find bounding box
            Vector3 min = points[0];
            Vector3 max = points[0];
            
            for (int i = 1; i < points.Length; i++)
            {
                min = Vector3.Min(min, points[i]);
                max = Vector3.Max(max, points[i]);
            }
            
            Vector3 center = (min + max) * 0.5f;
            float radius = Vector3.Distance(center, max);
            
            return new CSphere(center, radius);
        }

        /// <summary>
        /// Get minimum bounding sphere
        /// </summary>
        public static CSphere GetMinimumBoundingSphere(Vector3[] points)
        {
            if (points == null || points.Length == 0)
                return new CSphere();
            
            if (points.Length == 1)
                return new CSphere(points[0], EPSILON);
            
            // Simple approximation: use the center of the bounding box
            Vector3 min = points[0];
            Vector3 max = points[0];
            
            for (int i = 1; i < points.Length; i++)
            {
                min = Vector3.Min(min, points[i]);
                max = Vector3.Max(max, points[i]);
            }
            
            Vector3 center = (min + max) * 0.5f;
            float maxDistance = 0;
            
            for (int i = 0; i < points.Length; i++)
            {
                float distance = Vector3.Distance(center, points[i]);
                maxDistance = Math.Max(maxDistance, distance);
            }
            
            return new CSphere(center, maxDistance);
        }

        /// <summary>
        /// Clone sphere
        /// </summary>
        public CSphere Clone()
        {
            return new CSphere
            {
                Center = Center,
                Radius = Radius
            };
        }

        /// <summary>
        /// Check equality
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is CSphere other)
            {
                return Center.Equals(other.Center) && Math.Abs(Radius - other.Radius) < EPSILON;
            }
            return false;
        }

        /// <summary>
        /// Get hash code
        /// </summary>
        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Radius.GetHashCode();
        }

        /// <summary>
        /// To string
        /// </summary>
        public override string ToString()
        {
            return $"CSphere(Center={Center}, Radius={Radius})";
        }

        /// <summary>
        /// Slide sphere along surface (placeholder implementation)
        /// </summary>
        public bool SlideSphere(Vector3 normal, float distance)
        {
            // Slide sphere along normal by distance
            Center += normal * distance;
            return true;
        }

        /// <summary>
        /// Slide sphere along collision normal
        /// </summary>
        public TransitionState SlideSphere(SPHEREPATH path, ACE.Server.Physics.Alt.CollisionInfo collisions, Vector collisionNormal)
        {
            // TODO: Implement proper slide logic
            // For now, just return OK_TS
            return TransitionState.OK_TS;
        }

        /// <summary>
        /// Slide sphere with 4 parameters (for compatibility with GDLE)
        /// </summary>
        public TransitionState SlideSphere(SPHEREPATH path, ACE.Server.Physics.Alt.CollisionInfo collisions, Vector collisionNormal, Vector oldDisp)
        {
            // TODO: Implement proper slide logic with old displacement
            // For now, just return OK_TS
            return TransitionState.OK_TS;
        }

        public float FindTimeOfCollision(CSphere other, Vector3 velocity)
        {
            // Calculate time of collision between two spheres
            Vector3 relativeVelocity = velocity;
            Vector3 relativePosition = other.Center - Center;
            float radiusSum = Radius + other.Radius;
            
            // Quadratic equation: |relativePosition + relativeVelocity * t| = radiusSum
            float a = relativeVelocity.LengthSquared();
            float b = 2 * Vector3.Dot(relativePosition, relativeVelocity);
            float c = relativePosition.LengthSquared() - radiusSum * radiusSum;
            
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return float.MaxValue; // No collision
            
            float t1 = (-b - (float)Math.Sqrt(discriminant)) / (2 * a);
            float t2 = (-b + (float)Math.Sqrt(discriminant)) / (2 * a);
            
            // Return the earliest positive collision time
            if (t1 >= 0 && t2 >= 0)
                return Math.Min(t1, t2);
            else if (t1 >= 0)
                return t1;
            else if (t2 >= 0)
                return t2;
            else
                return float.MaxValue; // No future collision
        }

        /// <summary>
        /// Find time of collision with movement and displacement
        /// </summary>
        public double FindTimeOfCollision(Vector movement, Vector oldDisp, float radsum)
        {
            // TODO: Implement proper collision time calculation
            // For now, return a placeholder value
            return 0.5;
        }

        /// <summary>
        /// Check if sphere intersects with cylinder (placeholder implementation)
        /// </summary>
        public bool IntersectsCylinder(Vector3 cylinderCenter, Vector3 cylinderAxis, float cylinderRadius, float cylinderHeight)
        {
            // Project sphere center onto cylinder axis
            Vector3 toCenter = Center - cylinderCenter;
            float projectionLength = Vector3.Dot(toCenter, cylinderAxis);
            
            // Clamp projection to cylinder bounds
            projectionLength = Math.Max(-cylinderHeight * 0.5f, Math.Min(cylinderHeight * 0.5f, projectionLength));
            
            // Get closest point on cylinder axis
            Vector3 closestPointOnAxis = cylinderCenter + cylinderAxis * projectionLength;
            
            // Check distance from sphere center to cylinder axis
            float distanceToAxis = Vector3.Distance(Center, closestPointOnAxis);
            
            // Check if sphere intersects cylinder
            return distanceToAxis <= Radius + cylinderRadius;
        }

        /// <summary>
        /// Get the diameter of this sphere
        /// </summary>
        public float GetDiameter()
        {
            return Radius * 2.0f;
        }

        /// <summary>
        /// Get the circumference of this sphere
        /// </summary>
        public float GetCircumference()
        {
            return 2.0f * (float)Math.PI * Radius;
        }

        /// <summary>
        /// Create a sphere from three points
        /// </summary>
        public static CSphere FromThreePoints(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Calculate the circumcenter of the triangle
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p1;
            
            float v1DotV1 = Vector3.Dot(v1, v1);
            float v2DotV2 = Vector3.Dot(v2, v2);
            float v1DotV2 = Vector3.Dot(v1, v2);
            
            float denominator = 2.0f * (v1DotV1 * v2DotV2 - v1DotV2 * v1DotV2);
            if (Math.Abs(denominator) < EPSILON)
            {
                // Points are collinear, create a sphere with diameter equal to max distance
                float d1 = Vector3.Distance(p1, p2);
                float d2 = Vector3.Distance(p2, p3);
                float d3 = Vector3.Distance(p1, p3);
                float maxDistance = Math.Max(d1, Math.Max(d2, d3));
                
                Vector3 centerPoint = (p1 + p2 + p3) / 3.0f;
                return new CSphere(centerPoint, maxDistance * 0.5f);
            }
            
            float alpha = (v2DotV2 * Vector3.Dot(v1, v1 - v2)) / denominator;
            float beta = (v1DotV1 * Vector3.Dot(v2, v2 - v1)) / denominator;
            
            Vector3 calculatedCenter = p1 + alpha * v1 + beta * v2;
            float radius = Vector3.Distance(calculatedCenter, p1);
            
            return new CSphere(calculatedCenter, radius);
        }

        /// <summary>
        /// Create a sphere from a collection of points
        /// </summary>
        public static CSphere FromPoints(IEnumerable<Vector3> points)
        {
            if (points == null || !points.Any())
                return new CSphere();
            
            var pointArray = points.ToArray();
            return FromPoints(pointArray);
        }
    }
} 