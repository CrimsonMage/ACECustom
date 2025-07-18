namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Port of GDLE/PhatSDK ClipPlane struct for BSP logic.
    /// </summary>
    public struct ClipPlane
    {
        public Plane? Plane;
        public Sidedness Side;

        public ClipPlane(Plane? plane, Sidedness side)
        {
            Plane = plane;
            Side = side;
        }
    }
} 