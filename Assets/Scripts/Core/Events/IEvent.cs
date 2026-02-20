namespace DarkfallOnline.Events
{
    /// <summary>
    /// Marker interface that every game event struct must implement.
    ///
    /// Prefer structs over classes for events to keep allocations at zero.
    /// Example:
    ///   public struct PlayerDiedEvent : IEvent { public int PlayerId; }
    /// </summary>
    public interface IEvent { }
}
