using System;
using System.Numerics;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Frame class ported from GDLE Frame.h
    /// Represents position and orientation in 3D space
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// Origin (position) vector
        /// </summary>
        public Vector Origin { get; set; }

        /// <summary>
        /// Orientation quaternion
        /// </summary>
        public Quaternion Angles { get; set; }

        /// <summary>
        /// Rotation matrix (3x3)
        /// </summary>
        public float[,] Matrix { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Frame()
        {
            Origin = new Vector(0, 0, 0);
            Angles = new Quaternion(1, 0, 0, 0);
            Matrix = new float[3, 3];
            UpdateMatrix();
        }

        /// <summary>
        /// Constructor with origin and angles
        /// </summary>
        public Frame(Vector origin, Quaternion angles)
        {
            Origin = origin;
            Angles = angles;
            Matrix = new float[3, 3];
            UpdateMatrix();
        }

        /// <summary>
        /// Constructor with origin and euler angles
        /// </summary>
        public Frame(Vector origin, Vector eulerAngles)
        {
            Origin = origin;
            Angles = new Quaternion();
            Angles.FromEulerAngles(eulerAngles);
            Matrix = new float[3, 3];
            UpdateMatrix();
        }

        /// <summary>
        /// Constructor from Vector3 and System.Numerics.Quaternion
        /// </summary>
        public Frame(Vector3 origin, System.Numerics.Quaternion angles)
        {
            Origin = Vector.FromVector3(origin);
            Angles = new Quaternion(angles.W, angles.X, angles.Y, angles.Z);
            Matrix = new float[3, 3];
            UpdateMatrix();
        }

        /// <summary>
        /// Update the rotation matrix from quaternion
        /// </summary>
        public void UpdateMatrix()
        {
            // Convert quaternion to rotation matrix
            float w = Angles.W;
            float x = Angles.X;
            float y = Angles.Y;
            float z = Angles.Z;

            Matrix[0, 0] = 1 - 2 * y * y - 2 * z * z;
            Matrix[0, 1] = 2 * x * y - 2 * w * z;
            Matrix[0, 2] = 2 * x * z + 2 * w * y;

            Matrix[1, 0] = 2 * x * y + 2 * w * z;
            Matrix[1, 1] = 1 - 2 * x * x - 2 * z * z;
            Matrix[1, 2] = 2 * y * z - 2 * w * x;

            Matrix[2, 0] = 2 * x * z - 2 * w * y;
            Matrix[2, 1] = 2 * y * z + 2 * w * x;
            Matrix[2, 2] = 1 - 2 * x * x - 2 * y * y;
        }

        /// <summary>
        /// Update quaternion from rotation matrix
        /// </summary>
        public void UpdateQuaternion()
        {
            float trace = Matrix[0, 0] + Matrix[1, 1] + Matrix[2, 2];
            
            if (trace > 0)
            {
                float s = (float)Math.Sqrt(trace + 1.0f) * 2;
                float w = 0.25f * s;
                float x = (Matrix[2, 1] - Matrix[1, 2]) / s;
                float y = (Matrix[0, 2] - Matrix[2, 0]) / s;
                float z = (Matrix[1, 0] - Matrix[0, 1]) / s;
                Angles = new Quaternion(w, x, y, z);
            }
            else if (Matrix[0, 0] > Matrix[1, 1] && Matrix[0, 0] > Matrix[2, 2])
            {
                float s = (float)Math.Sqrt(1.0f + Matrix[0, 0] - Matrix[1, 1] - Matrix[2, 2]) * 2;
                float w = (Matrix[2, 1] - Matrix[1, 2]) / s;
                float x = 0.25f * s;
                float y = (Matrix[0, 1] + Matrix[1, 0]) / s;
                float z = (Matrix[0, 2] + Matrix[2, 0]) / s;
                Angles = new Quaternion(w, x, y, z);
            }
            else if (Matrix[1, 1] > Matrix[2, 2])
            {
                float s = (float)Math.Sqrt(1.0f + Matrix[1, 1] - Matrix[0, 0] - Matrix[2, 2]) * 2;
                float w = (Matrix[0, 2] - Matrix[2, 0]) / s;
                float x = (Matrix[0, 1] + Matrix[1, 0]) / s;
                float y = 0.25f * s;
                float z = (Matrix[1, 2] + Matrix[2, 1]) / s;
                Angles = new Quaternion(w, x, y, z);
            }
            else
            {
                float s = (float)Math.Sqrt(1.0f + Matrix[2, 2] - Matrix[0, 0] - Matrix[1, 1]) * 2;
                float w = (Matrix[1, 0] - Matrix[0, 1]) / s;
                float x = (Matrix[0, 2] + Matrix[2, 0]) / s;
                float y = (Matrix[1, 2] + Matrix[2, 1]) / s;
                float z = 0.25f * s;
                Angles = new Quaternion(w, x, y, z);
            }
        }

        /// <summary>
        /// Check if frame is valid
        /// </summary>
        public bool IsValid()
        {
            return Origin.IsValid() && Angles.IsValid();
        }

        /// <summary>
        /// Check if frame is valid except for heading
        /// </summary>
        public bool IsValidExceptForHeading()
        {
            return Origin.IsValid() && Angles.IsValid();
        }

        /// <summary>
        /// Check if origin vectors are equal
        /// </summary>
        public bool IsVectorEqual(Frame other)
        {
            return Origin.IsEqual(other.Origin);
        }

        /// <summary>
        /// Check if quaternions are equal
        /// </summary>
        public bool IsQuaternionEqual(Frame other)
        {
            return Angles.IsEqual(other.Angles);
        }

        /// <summary>
        /// Cache the frame (update matrix)
        /// </summary>
        public void Cache()
        {
            UpdateMatrix();
        }

        /// <summary>
        /// Cache quaternion (update from matrix)
        /// </summary>
        public void CacheQuaternion()
        {
            UpdateQuaternion();
        }

        /// <summary>
        /// Combine two frames
        /// </summary>
        public void Combine(Frame a, Frame b)
        {
            // Transform b's origin by a's orientation and add a's origin
            Origin = a.Origin + a.TransformVector(b.Origin);
            
            // Combine rotations
            Angles = a.Angles * b.Angles;
            UpdateMatrix();
        }

        /// <summary>
        /// Combine two frames with scale
        /// </summary>
        public void Combine(Frame a, Frame b, Vector scale)
        {
            // Transform b's origin by a's orientation, scale, and add a's origin
            Origin = a.Origin + a.TransformVector(b.Origin * scale);
            
            // Combine rotations
            Angles = a.Angles * b.Angles;
            UpdateMatrix();
        }

        /// <summary>
        /// Rotate by euler angles
        /// </summary>
        public void Rotate(Vector angles)
        {
            Quaternion rotation = new Quaternion();
            rotation.FromEulerAngles(angles);
            Angles = Angles * rotation;
            UpdateMatrix();
        }

        /// <summary>
        /// Get euler angles from quaternion
        /// </summary>
        public Vector GetEulerAngles()
        {
            return Angles.ToEulerAngles();
        }

        /// <summary>
        /// Set rotation by quaternion
        /// </summary>
        public void SetRotate(Quaternion angles)
        {
            Angles = angles;
            UpdateMatrix();
        }

        /// <summary>
        /// Set rotation by euler angles
        /// </summary>
        public void EulerSetRotate(Vector angles, int order = 0)
        {
            Angles = new Quaternion();
            Angles.FromEulerAngles(angles, order);
            UpdateMatrix();
        }

        /// <summary>
        /// Transform global point to local coordinates
        /// </summary>
        public Vector GlobalToLocal(Vector point)
        {
            Vector localPoint = point - Origin;
            return TransformVectorInverse(localPoint);
        }

        /// <summary>
        /// Transform global vector to local coordinates
        /// </summary>
        public Vector GlobalToLocalVec(Vector vector)
        {
            return TransformVectorInverse(vector);
        }

        /// <summary>
        /// Transform local point to global coordinates
        /// </summary>
        public Vector LocalToGlobal(Vector point)
        {
            Vector globalPoint = TransformVector(point);
            return globalPoint + Origin;
        }

        /// <summary>
        /// Transform local vector to global coordinates
        /// </summary>
        public Vector LocalToGlobalVec(Vector vector)
        {
            return TransformVector(vector);
        }

        /// <summary>
        /// Transform point from one local frame to another
        /// </summary>
        public Vector LocalToLocal(Frame frame, Vector point)
        {
            Vector globalPoint = LocalToGlobal(point);
            return frame.GlobalToLocal(globalPoint);
        }

        /// <summary>
        /// Transform vector using rotation matrix
        /// </summary>
        public Vector TransformVector(Vector vector)
        {
            return new Vector(
                Matrix[0, 0] * vector.X + Matrix[0, 1] * vector.Y + Matrix[0, 2] * vector.Z,
                Matrix[1, 0] * vector.X + Matrix[1, 1] * vector.Y + Matrix[1, 2] * vector.Z,
                Matrix[2, 0] * vector.X + Matrix[2, 1] * vector.Y + Matrix[2, 2] * vector.Z
            );
        }

        /// <summary>
        /// Transform vector using inverse rotation matrix
        /// </summary>
        public Vector TransformVectorInverse(Vector vector)
        {
            return new Vector(
                Matrix[0, 0] * vector.X + Matrix[1, 0] * vector.Y + Matrix[2, 0] * vector.Z,
                Matrix[0, 1] * vector.X + Matrix[1, 1] * vector.Y + Matrix[2, 1] * vector.Z,
                Matrix[0, 2] * vector.X + Matrix[1, 2] * vector.Y + Matrix[2, 2] * vector.Z
            );
        }

        /// <summary>
        /// Subtract two frames
        /// </summary>
        public void Subtract1(Frame a, Frame b)
        {
            Origin = a.Origin - b.Origin;
            Angles = a.Angles * b.Angles.Inverse();
            UpdateMatrix();
        }

        /// <summary>
        /// Subtract two frames (alternative method)
        /// </summary>
        public void Subtract2(Frame f1, Frame f2)
        {
            Subtract1(f1, f2);
        }

        /// <summary>
        /// Rotate around axis to target vector
        /// </summary>
        public void RotateAroundAxisToVector(int axis, Vector target)
        {
            // TODO: Implement axis rotation to target
        }

        /// <summary>
        /// Set heading in degrees
        /// </summary>
        public void SetHeading(float degreeHeading)
        {
            float radians = degreeHeading * (float)Math.PI / 180.0f;
            Vector eulerAngles = new Vector(0, 0, radians);
            EulerSetRotate(eulerAngles);
        }

        /// <summary>
        /// Get vector heading
        /// </summary>
        public Vector GetVectorHeading()
        {
            return TransformVector(new Vector(0, 1, 0));
        }

        /// <summary>
        /// Set vector heading
        /// </summary>
        public void SetVectorHeading(Vector heading)
        {
            // TODO: Implement vector heading setting
        }

        /// <summary>
        /// Get heading in degrees
        /// </summary>
        public float GetHeading()
        {
            Vector eulerAngles = GetEulerAngles();
            return eulerAngles.Z * 180.0f / (float)Math.PI;
        }

        /// <summary>
        /// Interpolate rotation between two frames
        /// </summary>
        public void InterpolateRotation(Frame from, Frame to, float t)
        {
            Angles = Quaternion.Slerp(from.Angles, to.Angles, t);
            UpdateMatrix();
        }

        /// <summary>
        /// Convert to Vector3 origin
        /// </summary>
        public Vector3 ToVector3Origin()
        {
            return Origin.ToVector3();
        }

        /// <summary>
        /// Convert to System.Numerics.Quaternion
        /// </summary>
        public System.Numerics.Quaternion ToQuaternion()
        {
            return new System.Numerics.Quaternion(Angles.X, Angles.Y, Angles.Z, Angles.W);
        }

        /// <summary>
        /// Create from Vector3 and System.Numerics.Quaternion
        /// </summary>
        public static Frame FromVector3AndQuaternion(Vector3 origin, System.Numerics.Quaternion quaternion)
        {
            return new Frame(origin, quaternion);
        }

        /// <summary>
        /// Identity frame
        /// </summary>
        public static Frame Identity => new Frame(Vector.Zero, Quaternion.Identity);

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"Frame(Origin={Origin}, Angles={Angles})";
        }

        /// <summary>
        /// Equals override
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Frame other)
                return IsVectorEqual(other) && IsQuaternionEqual(other);
            return false;
        }

        /// <summary>
        /// GetHashCode override
        /// </summary>
        public override int GetHashCode()
        {
            return Origin.GetHashCode() ^ Angles.GetHashCode();
        }
    }
} 