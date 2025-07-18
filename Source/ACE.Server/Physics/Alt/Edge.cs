using System;
using System.Numerics;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Represents an edge in a polygon
    /// </summary>
    public class Edge
    {
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }

        public Edge()
        {
            Start = Vector3.Zero;
            End = Vector3.Zero;
        }

        public Edge(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Get the direction vector of this edge
        /// </summary>
        public Vector3 Direction => Vector3.Normalize(End - Start);

        /// <summary>
        /// Get the length of this edge
        /// </summary>
        public float Length => Vector3.Distance(Start, End);

        /// <summary>
        /// Get the center point of this edge
        /// </summary>
        public Vector3 Center => (Start + End) * 0.5f;

        /// <summary>
        /// Check if a point is on this edge
        /// </summary>
        public bool ContainsPoint(Vector3 point, float tolerance = 0.001f)
        {
            var edgeVector = End - Start;
            var pointVector = point - Start;
            
            var edgeLengthSquared = edgeVector.LengthSquared();
            if (edgeLengthSquared < tolerance * tolerance)
                return false;

            var projection = Vector3.Dot(pointVector, edgeVector) / edgeLengthSquared;
            
            if (projection < 0 || projection > 1)
                return false;

            var projectedPoint = Start + edgeVector * projection;
            return Vector3.DistanceSquared(point, projectedPoint) <= tolerance * tolerance;
        }

        /// <summary>
        /// Get the closest point on this edge to a given point
        /// </summary>
        public Vector3 GetClosestPoint(Vector3 point)
        {
            var edgeVector = End - Start;
            var pointVector = point - Start;
            
            var edgeLengthSquared = edgeVector.LengthSquared();
            if (edgeLengthSquared < 0.0001f)
                return Start;

            var projection = Vector3.Dot(pointVector, edgeVector) / edgeLengthSquared;
            projection = Math.Max(0, Math.Min(1, projection)); // Clamp to edge

            return Start + edgeVector * projection;
        }

        /// <summary>
        /// Check if this edge intersects with another edge
        /// </summary>
        public bool Intersects(Edge other, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.Zero;
            
            if (other == null)
                return false;

            // Check if edges are in the same plane (simplified 2D intersection)
            var a1 = Start;
            var a2 = End;
            var b1 = other.Start;
            var b2 = other.End;

            var ua_t = (b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X);
            var ub_t = (a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X);
            var u_b = (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);

            if (Math.Abs(u_b) < 0.0001f)
                return false; // Parallel lines

            var ua = ua_t / u_b;
            var ub = ub_t / u_b;

            if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
                return false; // Intersection outside edge bounds

            intersectionPoint = a1 + (a2 - a1) * ua;
            return true;
        }
    }
} 