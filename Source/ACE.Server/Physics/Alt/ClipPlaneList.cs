using System.Collections.Generic;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Port of GDLE/PhatSDK ClipPlaneList struct for BSP logic.
    /// </summary>
    public struct ClipPlaneList
    {
        public uint ClipPlaneNum;
        public List<ClipPlane> ClipPlaneListData;
        public int LeafContainsObj;

        public ClipPlaneList(int initialCapacity)
        {
            ClipPlaneNum = 0;
            ClipPlaneListData = new List<ClipPlane>(initialCapacity);
            LeafContainsObj = 0;
        }
    }
} 