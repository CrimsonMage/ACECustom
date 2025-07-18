using System;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Represents a plane in 3D space (ported from GDLE/PhatSDK MathLib).
    /// </summary>
    public struct Plane
    {
        public Vector Normal;
        public float Distance;

        public Plane(Vector normal, float distance)
        {
            Normal = normal;
            Distance = distance;
        }

        public Plane(Vector normal, Vector point)
        {
            Normal = normal;
            Distance = -normal.Dot(point);
        }

        public float Dot(Vector point) => Normal.Dot(point) + Distance;

        public Sidedness WhichSide(Vector point, float epsilon = 1e-6f)
        {
            float dp = Dot(point);
            if (dp > epsilon) return Sidedness.Positive;
            if (dp < -epsilon) return Sidedness.Negative;
            return Sidedness.InPlane;
        }

        public bool ComputeTimeOfIntersection(Ray ray, out float time)
        {
            float dot = Normal.Dot(ray.Direction);
            if (Math.Abs(dot) < 1e-6f)
            {
                time = 0;
                return false;
            }
            float depth = -Dot(ray.Origin) / dot;
            time = depth;
            return depth >= 0.0f;
        }

        public void SnapToPlane(ref Vector offset)
        {
            if (Math.Abs(Normal.Z) > 1e-6f)
            {
                offset.Z = 0;
                offset.Z = -((offset.Y * Normal.Y) + (offset.X * Normal.X) + Distance) / Normal.Z;
            }
        }

        public bool Pack(byte[] data, int offset)
        {
            if (data == null || data.Length < offset + 16)
                return false;
            Buffer.BlockCopy(BitConverter.GetBytes(Normal.X), 0, data, offset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Normal.Y), 0, data, offset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Normal.Z), 0, data, offset + 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Distance), 0, data, offset + 12, 4);
            return true;
        }
        public bool UnPack(byte[] data, int size)
        {
            if (data == null || size < 16)
                return false;
            Normal = new Vector(
                BitConverter.ToSingle(data, 0),
                BitConverter.ToSingle(data, 4),
                BitConverter.ToSingle(data, 8));
            Distance = BitConverter.ToSingle(data, 12);
            return true;
        }
    }

    public enum Sidedness
    {
        Positive,
        Negative,
        InPlane
    }

    public struct Ray
    {
        public Vector Origin;
        public Vector Direction;
        public Ray(Vector origin, Vector direction)
        {
            Origin = origin;
            Direction = direction;
        }
    }
} 