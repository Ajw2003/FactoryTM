namespace EventSystems
{
    public interface IEvent //the syntax of ISomeFunctionality is reserved for Interfaces, which are promises to have some sort of functionality. Interfaces are not classes. 
    { 
    }

    public interface IEvent<T> : IEvent
    {
        T Value { get; }
    }
}