using System;
using System.Numerics;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Quaternion class for GDLE physics integration
    /// </summary>
    public class Quaternion
    {
        // Components
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        // Constants
        public const float EPSILON = 0.001f;

        /// <summary>
        /// Default constructor (identity quaternion)
        /// </summary>
        public Quaternion()
        {
            W = 1.0f;
            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;
        }

        /// <summary>
        /// Constructor with components
        /// </summary>
        public Quaternion(float w, float x, float y, float z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
            Normalize();
        }

        /// <summary>
        /// Constructor from euler angles (in radians)
        /// </summary>
        public Quaternion(Vector eulerAngles)
        {
            FromEulerAngles(eulerAngles);
        }

        /// <summary>
        /// Constructor from euler angles with order
        /// </summary>
        public Quaternion(Vector eulerAngles, int order)
        {
            FromEulerAngles(eulerAngles, order);
        }

        /// <summary>
        /// Create from euler angles (in radians)
        /// </summary>
        public void FromEulerAngles(Vector eulerAngles)
        {
            FromEulerAngles(eulerAngles, 0); // Default XYZ order
        }

        /// <summary>
        /// Create from euler angles with specified order
        /// </summary>
        public void FromEulerAngles(Vector eulerAngles, int order)
        {
            float cx = (float)Math.Cos(eulerAngles.X * 0.5f);
            float sx = (float)Math.Sin(eulerAngles.X * 0.5f);
            float cy = (float)Math.Cos(eulerAngles.Y * 0.5f);
            float sy = (float)Math.Sin(eulerAngles.Y * 0.5f);
            float cz = (float)Math.Cos(eulerAngles.Z * 0.5f);
            float sz = (float)Math.Sin(eulerAngles.Z * 0.5f);

            // XYZ order (default)
            W = cx * cy * cz + sx * sy * sz;
            X = sx * cy * cz - cx * sy * sz;
            Y = cx * sy * cz + sx * cy * sz;
            Z = cx * cy * sz - sx * sy * cz;

            Normalize();
        }

        /// <summary>
        /// Convert to euler angles
        /// </summary>
        public Vector ToEulerAngles()
        {
            Vector euler = new Vector();

            // Roll (x-axis rotation)
            float sinr_cosp = 2 * (W * X + Y * Z);
            float cosr_cosp = 1 - 2 * (X * X + Y * Y);
            euler.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // Pitch (y-axis rotation)
            float sinp = 2 * (W * Y - Z * X);
            if (Math.Abs(sinp) >= 1)
                euler.Y = (float)Math.CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                euler.Y = (float)Math.Asin(sinp);

            // Yaw (z-axis rotation)
            float siny_cosp = 2 * (W * Z + X * Y);
            float cosy_cosp = 1 - 2 * (Y * Y + Z * Z);
            euler.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return euler;
        }

        /// <summary>
        /// Normalize the quaternion
        /// </summary>
        public void Normalize()
        {
            float length = (float)Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
            if (length > EPSILON)
            {
                W /= length;
                X /= length;
                Y /= length;
                Z /= length;
            }
        }

        /// <summary>
        /// Get normalized quaternion
        /// </summary>
        public Quaternion Normalized()
        {
            Quaternion result = new Quaternion(W, X, Y, Z);
            result.Normalize();
            return result;
        }

        /// <summary>
        /// Get conjugate (inverse rotation)
        /// </summary>
        public Quaternion Conjugate()
        {
            return new Quaternion(W, -X, -Y, -Z);
        }

        /// <summary>
        /// Get inverse
        /// </summary>
        public Quaternion Inverse()
        {
            float lengthSquared = W * W + X * X + Y * Y + Z * Z;
            if (lengthSquared > EPSILON)
            {
                float invLengthSquared = 1.0f / lengthSquared;
                return new Quaternion(W * invLengthSquared, -X * invLengthSquared, -Y * invLengthSquared, -Z * invLengthSquared);
            }
            return new Quaternion(); // Return identity if zero length
        }

        /// <summary>
        /// Spherical linear interpolation
        /// </summary>
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
        {
            float dot = a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z;
            
            // Ensure we take the shortest path
            if (dot < 0)
            {
                b = new Quaternion(-b.W, -b.X, -b.Y, -b.Z);
                dot = -dot;
            }

            float theta = (float)Math.Acos(Math.Max(-1, Math.Min(1, dot)));
            float sinTheta = (float)Math.Sin(theta);

            if (sinTheta > EPSILON)
            {
                float invSinTheta = 1.0f / sinTheta;
                float coeff1 = (float)Math.Sin((1 - t) * theta) * invSinTheta;
                float coeff2 = (float)Math.Sin(t * theta) * invSinTheta;

                return new Quaternion(
                    coeff1 * a.W + coeff2 * b.W,
                    coeff1 * a.X + coeff2 * b.X,
                    coeff1 * a.Y + coeff2 * b.Y,
                    coeff1 * a.Z + coeff2 * b.Z
                );
            }
            else
            {
                // Linear interpolation for very small angles
                return new Quaternion(
                    a.W + t * (b.W - a.W),
                    a.X + t * (b.X - a.X),
                    a.Y + t * (b.Y - a.Y),
                    a.Z + t * (b.Z - a.Z)
                );
            }
        }

        /// <summary>
        /// Check if quaternion is valid
        /// </summary>
        public bool IsValid()
        {
            return !float.IsNaN(W) && !float.IsNaN(X) && !float.IsNaN(Y) && !float.IsNaN(Z) &&
                   !float.IsInfinity(W) && !float.IsInfinity(X) && !float.IsInfinity(Y) && !float.IsInfinity(Z);
        }

        /// <summary>
        /// Check if quaternion is approximately equal to another
        /// </summary>
        public bool IsEqual(Quaternion other)
        {
            return Math.Abs(W - other.W) < EPSILON &&
                   Math.Abs(X - other.X) < EPSILON &&
                   Math.Abs(Y - other.Y) < EPSILON &&
                   Math.Abs(Z - other.Z) < EPSILON;
        }

        /// <summary>
        /// Check if quaternion is approximately zero
        /// </summary>
        public bool IsZero()
        {
            return Math.Abs(W) < EPSILON &&
                   Math.Abs(X) < EPSILON &&
                   Math.Abs(Y) < EPSILON &&
                   Math.Abs(Z) < EPSILON;
        }

        /// <summary>
        /// Quaternion multiplication
        /// </summary>
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W
            );
        }

        /// <summary>
        /// Quaternion equality
        /// </summary>
        public static bool operator ==(Quaternion a, Quaternion b)
        {
            return a?.IsEqual(b) ?? (b == null);
        }

        /// <summary>
        /// Quaternion inequality
        /// </summary>
        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Identity quaternion
        /// </summary>
        public static Quaternion Identity => new Quaternion(1, 0, 0, 0);

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"Quaternion(W={W:F3}, X={X:F3}, Y={Y:F3}, Z={Z:F3})";
        }

        /// <summary>
        /// Equals override
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Quaternion other)
                return IsEqual(other);
            return false;
        }

        /// <summary>
        /// GetHashCode override
        /// </summary>
        public override int GetHashCode()
        {
            return W.GetHashCode() ^ X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
    }
} 