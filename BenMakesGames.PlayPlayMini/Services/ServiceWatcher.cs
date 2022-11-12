using System.Collections.Generic;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed class ServiceWatcher
{
    private List<IServiceLoadContent> ServiceWithLoadContentEvents { get; } = new();
    private List<IServiceInitialize> ServicesWithInitializeEvents { get; } = new();
    private List<IServiceInput> ServicesWithInputEvents { get; } = new();
    private List<IServiceUpdate> ServicesWithUpdateEvents { get; } = new();
    private List<IServiceDraw> ServicesWithDrawEvents { get; } = new();

    public IReadOnlyCollection<IServiceLoadContent> ContentLoadingServices => ServiceWithLoadContentEvents;
    public IReadOnlyCollection<IServiceInitialize> InitializedServices => ServicesWithInitializeEvents;
    public IReadOnlyCollection<IServiceInput> InputServices => ServicesWithInputEvents;
    public IReadOnlyCollection<IServiceUpdate> UpdatedServices => ServicesWithUpdateEvents;
    public IReadOnlyCollection<IServiceDraw> DrawnServices => ServicesWithDrawEvents;

    public void RegisterService(object service)
    {
        if (service is IServiceLoadContent hasLoadContent)
            ServiceWithLoadContentEvents.Add(hasLoadContent);

        if (service is IServiceInitialize hasInitialize)
            ServicesWithInitializeEvents.Add(hasInitialize);

        if (service is IServiceInput hasInput)
            ServicesWithInputEvents.Add(hasInput);

        if (service is IServiceUpdate hasUpdate)
            ServicesWithUpdateEvents.Add(hasUpdate);

        if (service is IServiceDraw hasDraw)
            ServicesWithDrawEvents.Add(hasDraw);
    }

    public void UnregisterService(object service)
    {
        if (service is IServiceLoadContent hasLoadContent)
            ServiceWithLoadContentEvents.Remove(hasLoadContent);

        if (service is IServiceInitialize hasInitialize)
            ServicesWithInitializeEvents.Remove(hasInitialize);

        if (service is IServiceInput hasInput)
            ServicesWithInputEvents.Remove(hasInput);

        if (service is IServiceUpdate hasUpdate)
            ServicesWithUpdateEvents.Remove(hasUpdate);

        if (service is IServiceDraw hasDraw)
            ServicesWithDrawEvents.Remove(hasDraw);
    }
}