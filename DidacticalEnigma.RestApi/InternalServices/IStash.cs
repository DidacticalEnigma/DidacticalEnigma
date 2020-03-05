using Optional;

namespace DidacticalEnigma.RestApi.InternalServices
{
    public interface IStash<T>
    {
        Option<T> Get(string identifier);
        void Delete(string identifier);
        string Put(T value);
        void IssueCleanup();
    }
}