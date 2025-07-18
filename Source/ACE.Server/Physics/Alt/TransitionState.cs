namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Transition states for physics movement
    /// </summary>
    public enum TransitionState
    {
        OK_TS = 0,
        Collided_TS = 1,
        Slid_TS = 2,
        Adjusted_TS = 3,
        Contact_TS = 4,
        Invalid_TS = 5,
        COLLIDED_TS = 1,
        ADJUSTED_TS = 3
    }
} 