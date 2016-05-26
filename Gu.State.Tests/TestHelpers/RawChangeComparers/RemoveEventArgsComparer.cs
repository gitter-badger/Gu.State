namespace Gu.State.Tests
{
    using NUnit.Framework;

    public class RemoveEventArgsComparer : EventArgsComparer<RemoveEventArgs>
    {
        public static readonly RemoveEventArgsComparer Default = new RemoveEventArgsComparer();

        public override bool Equals(RemoveEventArgs expected, RemoveEventArgs actual)
        {
            if (expected.Index != actual.Index)
            {
                throw new AssertionException($"Expected index to be {expected.Index} but was {actual.Index}");
            }

            return true;
        }
    }
}