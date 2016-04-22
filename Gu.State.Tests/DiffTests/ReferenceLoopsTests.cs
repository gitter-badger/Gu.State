namespace Gu.State.Tests.DiffTests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using static DiffTypes;

    public abstract class ReferenceLoopsTests
    {
        public abstract Diff DiffMethod<T>(T x, T y, ReferenceHandling referenceHandling = ReferenceHandling.Throw, string excludedMembers = null, Type excludedType = null) where T : class;

        [Test]
        public void ParentChildCreateWhenParentDirtyLoopDelete()
        {
            var x = new Parent("p1", new Child("c"));
            Assert.AreSame(x, x.Child.Parent);
            Assert.AreSame(x.Child, x.Child.Parent.Child);

            var y = new Parent("p2", new Child("c"));
            Assert.AreSame(y, y.Child.Parent);
            Assert.AreSame(y.Child, y.Child.Parent.Child);

            var expected = "Parent Name x: p1 y: p2 Child Parent ...";
            var result = DiffBy.PropertyValues(x, y, ReferenceHandling.StructuralWithReferenceLoops);
            var actual = result.ToString("", " ");
            Assert.AreEqual(expected, actual);
            Assert.AreSame(result, result.Diffs.Single(d => d.X == x.Child).Diffs.Last(d => d.X == x));
            Assert.Fail("Delete");
        }

        [Test]
        public void ParentChildCreateWhenParentDirtyLoop()
        {
            var x = new Parent("p1", new Child("c"));
            Assert.AreSame(x, x.Child.Parent);
            Assert.AreSame(x.Child, x.Child.Parent.Child);

            var y = new Parent("p2", new Child("c"));
            Assert.AreSame(y, y.Child.Parent);
            Assert.AreSame(y.Child, y.Child.Parent.Child);

            var expected = this is FieldValues.ReferenceLoops
                               ? "Parent <Name>k__BackingField x: p1 y: p2 <Child>k__BackingField <Parent>k__BackingField ..."
                               : "Parent Name x: p1 y: p2 Child Parent ...";
            var result = this.DiffMethod(x, y, ReferenceHandling.StructuralWithReferenceLoops);
            var actual = result.ToString("", " ");
            Assert.AreEqual(expected, actual);
            Assert.AreSame(result, result.Diffs.Single(d => d.X == x.Child).Diffs.Last(d => d.X == x));
        }

        [TestCase("p", "c", "Empty")]
        [TestCase("", "c", "Parent <member2> x: p y: ")]
        [TestCase("p", "", "Parent <member1> <member2> x: c y: ")]
        public void ParentChild(string p, string c, string expected)
        {
            expected = expected?.Replace("<member1>", this is FieldValues.ReferenceLoops ? "<Child>k__BackingField" : "Child")
                                .Replace("<member2>", this is FieldValues.ReferenceLoops ? "<Name>k__BackingField" : "Name");
            var x = new Parent("p", new Child("c"));
            var y = new Parent(p, new Child(c));
            var result = this.DiffMethod(x, y, ReferenceHandling.StructuralWithReferenceLoops);
            Assert.AreEqual(expected, result.ToString("", " "));

            result = this.DiffMethod(x, y, ReferenceHandling.StructuralWithReferenceLoops);
            Assert.AreEqual(expected, result.ToString("", " "));
        }

        [Test]
        public void ParentChildWhenTargetChildIsNull()
        {
            var x = new Parent("p", new Child("c"));
            var y = new Parent("p", null);
            var result = this.DiffMethod(x, y, ReferenceHandling.StructuralWithReferenceLoops);
            var expected = this is FieldValues.ReferenceLoops
                               ? "Parent <Child>k__BackingField x: Gu.State.Tests.DiffTests.DiffTypes+Child y: null"
                               : "Parent Child x: Gu.State.Tests.DiffTests.DiffTypes+Child y: null";
            Assert.AreEqual(expected, result.ToString("", " "));

            //result = this.EqualMethod(y, x, ReferenceHandling.Structural);
            //Assert.AreEqual(false, result);

            result = this.DiffMethod(x, y, ReferenceHandling.References);
            Assert.AreEqual(expected, result.ToString("", " "));
        }

        [Test]
        public void ParentChildWhenSourceChildIsNull()
        {
            var x = new Parent("p", null);
            var y = new Parent("p", new Child("c"));
            var result = this.DiffMethod(x, y, ReferenceHandling.StructuralWithReferenceLoops);
            var expected = this is FieldValues.ReferenceLoops
                               ? "Parent <Child>k__BackingField x: null y: Gu.State.Tests.DiffTests.DiffTypes+Child"
                               : "Parent Child x: null y: Gu.State.Tests.DiffTests.DiffTypes+Child";
            Assert.AreEqual(expected, result.ToString("", " "));

            //result = this.EqualMethod(y, x, ReferenceHandling.Structural);
            //Assert.AreEqual(false, result);

            result = this.DiffMethod(x, y, ReferenceHandling.References);
            Assert.AreEqual(expected, result.ToString("", " "));
        }
    }
}