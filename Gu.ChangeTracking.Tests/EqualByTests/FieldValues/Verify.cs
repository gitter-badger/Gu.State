﻿namespace Gu.ChangeTracking.Tests.EqualByTests.FieldValues
{
    using System;
    using Gu.ChangeTracking.Tests.CopyTests;
    using NUnit.Framework;

    public class Verify : VerifyTests
    {
        public override void VerifyMethod<T>()
        {
            Assert.Inconclusive("Implement?");
            //EqualBy.VerifyCanEquateFieldValues<T>();
        }

        public override void VerifyMethod<T>(ReferenceHandling referenceHandling)
        {
            Assert.Inconclusive("Implement?");
            //EqualBy.VerifyCanEquateFieldValues<T>(referenceHandling);
        }
    }
}
