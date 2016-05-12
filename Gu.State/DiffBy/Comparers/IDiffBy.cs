namespace Gu.State
{
    using System;

    internal interface IDiffBy
    {
        void AddDiffs<TSettings>(
            DiffBuilder collectionBuilder,
            object x,
            object y,
            TSettings settings,
            Action<DiffBuilder, object, object, object, TSettings> itemDiff)
            where TSettings : IMemberSettings;
    }
}