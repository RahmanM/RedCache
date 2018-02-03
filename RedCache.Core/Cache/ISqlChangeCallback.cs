namespace RedCache.Core
{

    public interface ISqlChangeCallback
    {
        void SqlChangedCallback(string table, string key);
    }

}
