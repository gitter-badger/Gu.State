﻿namespace Gu.State
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    public static partial class DiffBy
    {
        /// <summary>
        /// Compares x and y for equality using property values.
        /// If a type implements IList the items of the list are compared
        /// </summary>
        /// <typeparam name="T">The type to compare</typeparam>
        /// <param name="x">The first instance</param>
        /// <param name="y">The second instance</param>
        /// <param name="referenceHandling">
        /// If Structural is used a deep equals is performed.
        /// Default value is Throw
        /// </param>
        /// <param name="bindingFlags">The binding flags to use when getting properties</param>
        /// <returns>Diff.Empty if <paramref name="x"/> and <paramref name="y"/> are equal</returns>
        public static Diff PropertyValues<T>(
            T x,
            T y,
            ReferenceHandling referenceHandling = ReferenceHandling.Throw,
            BindingFlags bindingFlags = Constants.DefaultPropertyBindingFlags)
        {
            var settings = PropertiesSettings.GetOrCreate(bindingFlags, referenceHandling);
            return PropertyValues(x, y, settings);
        }

        /// <summary>
        /// Compares x and y for equality using property values and returns the difference.
        /// If a type implements IList the items of the list are compared
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="x"/> and <paramref name="y"/></typeparam>
        /// <param name="x">The first instance</param>
        /// <param name="y">The second instance</param>
        /// <param name="settings">Specifies how equality is performed.</param>
        /// <returns>Diff.Empty if <paramref name="x"/> and <paramref name="y"/> are equal</returns>
        public static ValueDiff PropertyValues<T>(T x, T y, PropertiesSettings settings)
        {
            Ensure.NotNull(x, nameof(x));
            Ensure.NotNull(y, nameof(y));
            Ensure.NotNull(settings, nameof(settings));
            EqualBy.Verify.CanEqualByPropertyValues(x, y, settings, typeof(DiffBy).Name, nameof(PropertyValues));
            return PropertyValuesDiffs.Get(x, y, settings) ?? new EmptyDiff(x, y);
        }

        private static class PropertyValuesDiffs
        {
            internal static ValueDiff Get<T>(T x, T y, PropertiesSettings settings)
            {
                Debug.Assert(x != null, "x == null");
                Debug.Assert(y != null, "y == null");
                Debug.Assert(settings != null, "settings == null");

                ValueDiff diff;
                if (TryGetValueDiff(x, y, settings, out diff))
                {
                    return diff;
                }

                EqualBy.Verify.CanEqualByPropertyValues(x, y, settings, typeof(DiffBy).Name, nameof(PropertyValues));
                var builder = new DiffBuilderRoot(settings.ReferenceHandling);
                AddSubDiffs(x, y, settings, builder);
                return builder.IsEmpty
                           ? null
                           : new ValueDiff(x, y, builder.Diffs);
            }

            private static void AddSubDiffs<T>(
                T x,
                T y,
                PropertiesSettings settings,
                DiffBuilder builder)
            {
                SubBuilder subBuilder;
                if (!builder.TryAdd(x, y, out subBuilder))
                {
                    return;
                }

                //var diffs = Enumerable.Diffs(x, y, settings, builder, ItemPropertiesDiff);
                var propertyInfos = x.GetType().GetProperties(settings.BindingFlags);
                foreach (var propertyInfo in propertyInfos)
                {
                    if (settings.IsIgnoringProperty(propertyInfo))
                    {
                        continue;
                    }

                    var getterAndSetter = settings.GetOrCreateGetterAndSetter(propertyInfo);
                    if (settings.IsEquatable(getterAndSetter.ValueType))
                    {
                        if (!getterAndSetter.ValueEquals(x, y))
                        {
                            subBuilder.Add(new PropertyDiff(propertyInfo, getterAndSetter.GetValue(x), getterAndSetter.GetValue(y)));
                        }

                        continue;
                    }

                    var xv = getterAndSetter.GetValue(x);
                    var yv = getterAndSetter.GetValue(y);
                    PropertyValueDiff(xv, yv, propertyInfo, settings, builder);
                }
            }

            //private static void ItemPropertiesDiff(
            //    object x,
            //    object y,
            //    PropertiesSettings settings,
            //    DiffBuilder referencePairs)
            //{
            //    ValueDiff diff;
            //    if (TryGetValueDiff(x, y, settings, out diff))
            //    {
            //        return diff;
            //    }

            //    if (settings.ReferenceHandling == ReferenceHandling.References)
            //    {
            //        return ReferenceEquals(x, y)
            //                   ? null
            //                   : new ValueDiff(x, y);
            //    }

            //    EqualBy.Verify.CanEqualByPropertyValues(x, y, settings, typeof(DiffBy).Name, nameof(PropertyValues));
            //    AddSubDiffs(x, y, settings, referencePairs);
            //}

            private static void PropertyValueDiff(
                object xValue,
                object yValue,
                PropertyInfo propertyInfo,
                PropertiesSettings settings,
                DiffBuilder builder)
            {
                ValueDiff diff;
                if (TryGetValueDiff(xValue, yValue, settings, out diff))
                {
                    return diff == null
                               ? null
                               : new PropertyDiff(propertyInfo, diff);
                }

                switch (settings.ReferenceHandling)
                {
                    case ReferenceHandling.References:
                        return ReferenceEquals(xValue, yValue) ? null : new PropertyDiff(propertyInfo, xValue, yValue);
                    case ReferenceHandling.Structural:
                    case ReferenceHandling.StructuralWithReferenceLoops:
                        EqualBy.Verify.CanEqualByPropertyValues(xValue, yValue, settings, typeof(DiffBy).Name, nameof(PropertyValues));
                        var subBuilder = AddSubDiffs(xValue, yValue, settings, builder);
                        return subBuilder.IsEmpty
                                   ? null
                                   : new PropertyDiff(propertyInfo, new ValueDiff(xValue, yValue, subBuilder.Diffs));

                    case ReferenceHandling.Throw:
                        throw Throw.ShouldNeverGetHereException();
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(settings.ReferenceHandling),
                            settings.ReferenceHandling,
                            null);
                }
            }
        }
    }
}
