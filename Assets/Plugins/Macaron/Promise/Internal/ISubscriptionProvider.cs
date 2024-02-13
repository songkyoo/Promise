namespace Macaron.Internal
{
    internal interface ISubscriptionProvider<TPublisher, TSubscriber>
    {
        Subscription<TPublisher, TSubscriber> Create(Promise<TPublisher> publisher, Promise<TSubscriber> subscriber);
    }
}
