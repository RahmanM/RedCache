namespace RedCache.Core
{

    public enum KeyEvent
    {
        Delete = 0,
        RenameFrom = 1,
        RenameTo = 2,
        ExpirationSet = 3,
        Expire = 4,
        SortStore = 5,
        Set = 6,
        RangeSet = 7,
        Increment = 8,
        IncrementByFloat = 9,
        Append = 10,
        PushLeft = 11,
        PopLeft = 12,
        PushRight = 13,
        PopRight = 14,
        ListInsert = 15,
        ListSet = 16,
        ListRemove = 17,
        ListTrim = 18,
        HashSet = 19,
        HashIncrement = 20,
        HashIncrementByFloat = 21,
        HashDelete = 22,
        SetAdd = 23,
        SetRemove = 24,
        SetPop = 25,
        SetIntersectStore = 26,
        SetUnionStore = 27,
        SetDiffStore = 28,
        SortedIncrementScore = 29,
        SortedAddScore = 30,
        SortedRemoveScore = 31,
        SortedRemoveByScore = 32,
        SortedRemoveByRank = 33,
        SortedIntersectStore = 34,
        SortedUnionStore = 35,
        Evicted = 36
    }

}
