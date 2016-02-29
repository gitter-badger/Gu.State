﻿namespace Gu.ChangeTracking.Tests.FieldValues
{
    using System;

    using Gu.ChangeTracking.Tests.CopyStubs;

    using NUnit.Framework;

    public class Classes : Tests.ClassesTests
    {
        public override void CopyMethod<T>(T source, T target)
        {
            Copy.FieldValues(source, target);
        }

        public override void CopyMethod<T>(T source, T target, ReferenceHandling referenceHandling)
        {
            Copy.FieldValues(source, target, referenceHandling);
        }

        public override void CopyMethod<T>(T source, T target, params string[] excluded)
        {
            Copy.FieldValues(source, target, excluded);
        }

        [Test]
        public void WithReadonlyFieldThrows()
        {
            var x = new WithReadonlyField(1, 2);
            var y = new WithReadonlyField(3, 4);
            var expected = "Copy.FieldValues(x, y) failed.\r\n" +
                           "The readonly field WithReadonlyField.ReadonlyValue differs after copy.\r\n" +
                           " - Source value: 1.\r\n" +
                           " - Target value: 3.\r\n" +
                           "The field is of type int.\r\n" +
                           "Solve the problem by any of:\r\n" +
                           "* Use CopyFieldsSettings and specify how copying is performed:\r\n" +
                           "  - ReferenceHandling.Structural means that a deep copy is performed.\r\n" +
                           "  - ReferenceHandling.References means that references are copied.\r\n" +
                           "  - Exclude the type WithReadonlyField.\r\n" +
                           "  - Exclude the field WithReadonlyField.ReadonlyValue.\r\n";

            var exception = Assert.Throws<InvalidOperationException>(() => Copy.FieldValues(x, y));
            Assert.AreEqual(expected, exception.Message);
        }
    }
}