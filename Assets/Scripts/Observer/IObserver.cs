public interface IObserver<in TEvent>
{
    void OnNotify(TEvent evt, object data = null);
}