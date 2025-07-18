using System;
using System.Numerics;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// 2D Vector for GDLE physics compatibility
    /// </summary>
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator *(Vector2 a, float scalar)
        {
            return new Vector2(a.X * scalar, a.Y * scalar);
        }

        public static Vector2 operator /(Vector2 a, float scalar)
        {
            return new Vector2(a.X / scalar, a.Y / scalar);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        public float Length()
        {
            return (float)Math.Sqrt(LengthSquared());
        }

        public static implicit operator Vector2(System.Numerics.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static implicit operator System.Numerics.Vector2(Vector2 v)
        {
            return new System.Numerics.Vector2(v.X, v.Y);
        }
    }

    /// <summary>
    /// Extension methods for Vector3 compatibility
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Get normalized vector (returns new vector)
        /// </summary>
        public static Vector3 Normalized(this Vector3 vector)
        {
            var length = vector.Length();
            if (length < PhysicsGlobals.Epsilon)
                return Vector3.Zero;
            return vector / length;
        }
    }

    /// <summary>
    /// 3D Vector for GDLE physics compatibility
    /// </summary>
    public struct Vector
    {
        public float X;
        public float Y;
        public float Z;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Vector()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        /// <summary>
        /// Constructor with scalar value
        /// </summary>
        public Vector(float scalar)
        {
            X = scalar;
            Y = scalar;
            Z = scalar;
        }

        /// <summary>
        /// Constructor with x, y, z components
        /// </summary>
        public Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Constructor from Vector3
        /// </summary>
        public Vector(Vector3 vector3)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
        }

        /// <summary>
        /// Convert to Vector3
        /// </summary>
        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        /// <summary>
        /// Convert from Vector3
        /// </summary>
        public static Vector FromVector3(Vector3 vector3)
        {
            return new Vector(vector3.X, vector3.Y, vector3.Z);
        }

        /// <summary>
        /// Scalar multiplication
        /// </summary>
        public static Vector operator *(Vector vector, float scalar)
        {
            return new Vector(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
        }

        /// <summary>
        /// Scalar multiplication (reverse order)
        /// </summary>
        public static Vector operator *(float scalar, Vector vector)
        {
            return vector * scalar;
        }

        /// <summary>
        /// Vector multiplication (component-wise)
        /// </summary>
        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        /// <summary>
        /// Scalar division
        /// </summary>
        public static Vector operator /(Vector vector, float scalar)
        {
            return new Vector(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
        }

        /// <summary>
        /// Vector subtraction
        /// </summary>
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Vector addition
        /// </summary>
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Implicit conversion from Vector3 to Vector
        /// </summary>
        public static implicit operator Vector(Vector3 vector3)
        {
            return new Vector(vector3.X, vector3.Y, vector3.Z);
        }

        /// <summary>
        /// Implicit conversion from Vector to Vector3
        /// </summary>
        public static implicit operator Vector3(Vector vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }



        /// <summary>
        /// Scalar multiplication assignment
        /// </summary>
        public Vector MultiplyAssign(float scalar)
        {
            X *= scalar;
            Y *= scalar;
            Z *= scalar;
            return this;
        }

        /// <summary>
        /// Vector multiplication assignment (component-wise)
        /// </summary>
        public Vector MultiplyAssign(Vector other)
        {
            X *= other.X;
            Y *= other.Y;
            Z *= other.Z;
            return this;
        }

        /// <summary>
        /// Scalar division assignment
        /// </summary>
        public Vector DivideAssign(float scalar)
        {
            X /= scalar;
            Y /= scalar;
            Z /= scalar;
            return this;
        }

        /// <summary>
        /// Vector division assignment (component-wise)
        /// </summary>
        public Vector DivideAssign(Vector other)
        {
            X /= other.X;
            Y /= other.Y;
            Z /= other.Z;
            return this;
        }

        /// <summary>
        /// Vector subtraction assignment
        /// </summary>
        public Vector SubtractAssign(Vector other)
        {
            X -= other.X;
            Y -= other.Y;
            Z -= other.Z;
            return this;
        }

        /// <summary>
        /// Vector addition assignment
        /// </summary>
        public Vector AddAssign(Vector other)
        {
            X += other.X;
            Y += other.Y;
            Z += other.Z;
            return this;
        }

        /// <summary>
        /// Get magnitude (length) of vector
        /// </summary>
        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Get magnitude squared
        /// </summary>
        public float MagSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// Get sum of squares (alias for MagSquared)
        /// </summary>
        public float SumOfSquare()
        {
            return MagSquared();
        }

        /// <summary>
        /// Get length squared (alias for MagSquared)
        /// </summary>
        public float LengthSquared()
        {
            return MagSquared();
        }

        /// <summary>
        /// Get length (alias for Magnitude)
        /// </summary>
        public float Length()
        {
            return Magnitude();
        }

        /// <summary>
        /// Dot product with another vector
        /// </summary>
        public float DotProduct(Vector other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>
        /// Dot product with another vector (alias for DotProduct)
        /// </summary>
        public float Dot(Vector other)
        {
            return DotProduct(other);
        }

        /// <summary>
        /// Normalize the vector (modifies this vector)
        /// </summary>
        public Vector Normalize()
        {
            float mag = Magnitude();
            if (mag > 0)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
            }
            return this;
        }

        /// <summary>
        /// Get normalized vector (returns new vector)
        /// </summary>
        public Vector Normalized()
        {
            Vector result = this;
            return result.Normalize();
        }

        /// <summary>
        /// Cross product with another vector
        /// </summary>
        public Vector Cross(Vector other)
        {
            return new Vector(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X
            );
        }

        /// <summary>
        /// Check if vector is approximately zero
        /// </summary>
        public bool IsZero()
        {
            return Math.Abs(X) < PhysicsGlobals.Epsilon &&
                   Math.Abs(Y) < PhysicsGlobals.Epsilon &&
                   Math.Abs(Z) < PhysicsGlobals.Epsilon;
        }

        /// <summary>
        /// Check if vector is approximately equal to another
        /// </summary>
        public bool IsEqual(Vector other)
        {
            return Math.Abs(X - other.X) < PhysicsGlobals.Epsilon &&
                   Math.Abs(Y - other.Y) < PhysicsGlobals.Epsilon &&
                   Math.Abs(Z - other.Z) < PhysicsGlobals.Epsilon;
        }

        /// <summary>
        /// Get heading angle in radians
        /// </summary>
        public float GetHeading()
        {
            return (float)Math.Atan2(X, Y);
        }

        /// <summary>
        /// Get heading angle in degrees
        /// </summary>
        public float GetHeadingDegrees()
        {
            return GetHeading() * 180.0f / (float)Math.PI;
        }

        /// <summary>
        /// Set heading angle in radians
        /// </summary>
        public void SetHeading(float heading)
        {
            float magnitude = Magnitude();
            X = magnitude * (float)Math.Sin(heading);
            Y = magnitude * (float)Math.Cos(heading);
        }

        /// <summary>
        /// Set heading angle in degrees
        /// </summary>
        public void SetHeadingDegrees(float headingDegrees)
        {
            SetHeading(headingDegrees * (float)Math.PI / 180.0f);
        }

        /// <summary>
        /// Distance to another vector
        /// </summary>
        public float Distance(Vector other)
        {
            Vector diff = this - other;
            return diff.Magnitude();
        }

        /// <summary>
        /// Distance squared to another vector
        /// </summary>
        public float DistanceSquared(Vector other)
        {
            Vector diff = this - other;
            return diff.MagSquared();
        }

        /// <summary>
        /// Linear interpolation between two vectors
        /// </summary>
        public static Vector Lerp(Vector a, Vector b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// Spherical linear interpolation between two vectors
        /// </summary>
        public static Vector Slerp(Vector a, Vector b, float t)
        {
            float dot = a.DotProduct(b);
            dot = Math.Max(-1.0f, Math.Min(1.0f, dot));
            float theta = (float)Math.Acos(dot) * t;
            
            Vector relative = (b - a * dot).Normalized();
            return a * (float)Math.Cos(theta) + relative * (float)Math.Sin(theta);
        }

        /// <summary>
        /// Check if vector is valid
        /// </summary>
        public bool IsValid()
        {
            return !float.IsNaN(X) && !float.IsNaN(Y) && !float.IsNaN(Z) &&
                   !float.IsInfinity(X) && !float.IsInfinity(Y) && !float.IsInfinity(Z);
        }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"Vector({X:F3}, {Y:F3}, {Z:F3})";
        }

        /// <summary>
        /// Equals override
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Vector other)
                return IsEqual(other);
            return false;
        }

        /// <summary>
        /// GetHashCode override
        /// </summary>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Vector a, Vector b)
        {
            return a.IsEqual(b);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Vector a, Vector b)
        {
            return !a.IsEqual(b);
        }

        /// <summary>
        /// Zero vector
        /// </summary>
        public static Vector Zero => new Vector(0, 0, 0);

        /// <summary>
        /// Unit X vector
        /// </summary>
        public static Vector UnitX => new Vector(1, 0, 0);

        /// <summary>
        /// Unit Y vector
        /// </summary>
        public static Vector UnitY => new Vector(0, 1, 0);

        /// <summary>
        /// Unit Z vector
        /// </summary>
        public static Vector UnitZ => new Vector(0, 0, 1);

        /// <summary>
        /// One vector
        /// </summary>
        public static Vector One => new Vector(1, 1, 1);

        // Additional GDLE physics methods

        /// <summary>
        /// Calculate the dot product of two vectors
        /// </summary>
        public static float Dot(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>
        /// Calculate the cross product of two vectors
        /// </summary>
        public static Vector Cross(Vector a, Vector b)
        {
            return new Vector(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        // Operator overloads for Vector3 and Vector
        public static Vector operator -(Vector a, Vector3 b) => a - (Vector)b;
        public static Vector operator -(Vector3 a, Vector b) => (Vector)a - b;
        public static Vector operator +(Vector a, Vector3 b) => a + (Vector)b;
        public static Vector operator +(Vector3 a, Vector b) => (Vector)a + b;

        // Static constants (removed duplicates)
        public static readonly Vector Up = new Vector(0, 0, 1);
        public static readonly Vector Down = new Vector(0, 0, -1);
        public static readonly Vector Left = new Vector(-1, 0, 0);
        public static readonly Vector Right = new Vector(1, 0, 0);
        public static readonly Vector Forward = new Vector(0, 1, 0);
        public static readonly Vector Backward = new Vector(0, -1, 0);
    }
} 