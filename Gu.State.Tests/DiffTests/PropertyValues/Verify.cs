﻿namespace Gu.State.Tests.DiffTests.PropertyValues
{
    using System;

    using Gu.State.Tests.EqualByTests;

    public class Verify : VerifyTests
    {
        public override void VerifyMethod<T>(ReferenceHandling referenceHandling = ReferenceHandling.Throw, string excludedMembers = null, Type excludedType = null)
        {
            var builder = PropertiesSettings.Build();
            if (excludedMembers != null)
            {
                builder.IgnoreProperty<T>(excludedMembers);
            }

            if (excludedType != null)
            {
                builder.IgnoreType(excludedType);
            }

            var settings = builder.CreateSettings(referenceHandling);
            EqualBy.VerifyCanEqualByPropertyValues<T>(settings);
        }
    }
}