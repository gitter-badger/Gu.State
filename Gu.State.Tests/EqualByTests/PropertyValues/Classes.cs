﻿namespace Gu.State.Tests.EqualByTests.PropertyValues
{
    using System;
    using System.Collections.Generic;

    public class Classes : ClassesTests
    {
        public override bool EqualMethod<T>(T x, T y, ReferenceHandling referenceHandling = ReferenceHandling.Structural, string excludedMembers = null, Type excludedType = null)
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
            return EqualBy.PropertyValues(x, y, settings);
        }

        public new static IReadOnlyList<EqualByTestsShared.EqualsData> EqualsSource => EqualByTestsShared.EqualsSource;
    }
}
