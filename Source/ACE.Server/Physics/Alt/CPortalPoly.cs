using System;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Port of GDLE/PhatSDK CPortalPoly class for BSP portal logic.
    /// </summary>
    public class CPortalPoly
    {
        // Members
        public int PortalIndex;
        public IntPtr Portal; // CPolygon* (opaque for now)

        // TODO: Implement methods as needed
        public CPortalPoly()
        {
            PortalIndex = 0;
            Portal = IntPtr.Zero;
        }
        ~CPortalPoly() { /* No-op destructor for C# GC */ }
        public bool UnPack(byte[] data, int size)
        {
            // 1:1 port from GDLE/PhatSDK CPortalPoly::UnPack
            if (data == null || size < 4)
                return false;
            using (var br = new System.IO.BinaryReader(new System.IO.MemoryStream(data, 0, size)))
            {
                short index = br.ReadInt16();
                short what = br.ReadInt16();
                PortalIndex = what;
                // In C++: portal = &BSPNODE::pack_poly[Index];
                // In C#: store the index as IntPtr for now (opaque)
                Portal = (IntPtr)index;
            }
            return true;
        }
        // No base.UnPack() to call, but method is explicit for structure
    }
} 