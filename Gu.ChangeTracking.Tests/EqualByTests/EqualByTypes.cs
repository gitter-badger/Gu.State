﻿namespace Gu.ChangeTracking.Tests.EqualByTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    public static class EqualByTypes
    {
        public class WithComplexValue
        {
            private string name;
            private int value;
            private ComplexType complexValue;

            public WithComplexValue()
            {
            }

            public WithComplexValue(string name, int value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name
            {
                get { return this.name; }
                set { this.name = value; }
            }

            public int Value
            {
                get { return this.value; }
                set { this.value = value; }
            }

            public ComplexType ComplexValue
            {
                get { return this.complexValue; }
                set { this.complexValue = value; }
            }
        }

        public class WithSimpleValues
        {
            private int intValue;
            private int? nullableIntValue;
            private string stringValue;
            private StringSplitOptions enumValue;

            public WithSimpleValues(int intValue, int? nullableIntValue, string stringValue, StringSplitOptions enumValue)
            {
                this.IntValue = intValue;
                this.NullableIntValue = nullableIntValue;
                this.StringValue = stringValue;
                this.EnumValue = enumValue;
            }

            public int IntValue
            {
                get { return this.intValue; }
                set { this.intValue = value; }
            }

            public int? NullableIntValue
            {
                get { return this.nullableIntValue; }
                set { this.nullableIntValue = value; }
            }

            public string StringValue
            {
                get { return this.stringValue; }
                set { this.stringValue = value; }
            }

            public StringSplitOptions EnumValue
            {
                get { return this.enumValue; }
                set { this.enumValue = value; }
            }

            public override string ToString()
            {
                return $"({this.IntValue}, {this.NullableIntValue}, {this.StringValue}, {this.EnumValue})";
            }
        }

        public class ComplexType
        {
            public static readonly TestComparer Comparer = new TestComparer();

            public ComplexType()
            {
            }

            public ComplexType(string name, int value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name { get; set; }

            public int Value { get; set; }

            public sealed class TestComparer : IEqualityComparer<ComplexType>, IComparer<ComplexType>, IComparer
            {
                public bool Equals(ComplexType x, ComplexType y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return true;
                    }
                    if (ReferenceEquals(x, null))
                    {
                        return false;
                    }
                    if (ReferenceEquals(y, null))
                    {
                        return false;
                    }
                    if (x.GetType() != y.GetType())
                    {
                        return false;
                    }
                    return string.Equals(x.Name, y.Name) && x.Value == y.Value;
                }

                public int GetHashCode(ComplexType obj)
                {
                    unchecked
                    {
                        return ((obj.Name?.GetHashCode() ?? 0) * 397) ^ obj.Value;
                    }
                }

                public int Compare(ComplexType x, ComplexType y)
                {
                    return this.Equals(x, y)
                               ? 0
                               : -1;
                }

                int IComparer.Compare(object x, object y)
                {
                    return this.Compare((ComplexType)x, (ComplexType)y);
                }
            }
        }

        public sealed class Immutable : IEquatable<Immutable>
        {
            public Immutable(int value)
            {
                this.Value = value;
            }

            public int Value { get; }

            public static bool operator ==(Immutable left, Immutable right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Immutable left, Immutable right)
            {
                return !Equals(left, right);
            }

            public bool Equals(Immutable other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }
                if (ReferenceEquals(this, other))
                {
                    return true;
                }
                return this.Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                return obj is Immutable && this.Equals((Immutable)obj);
            }

            public override int GetHashCode()
            {
                return this.Value;
            }
        }

        public class WithArrayProperty
        {
            public WithArrayProperty()
            {
            }

            public WithArrayProperty(string name, int value)
            {
                this.Name = name;
                this.Value = value;
            }

            public WithArrayProperty(string name, int value, int[] array)
            {
                this.Name = name;
                this.Value = value;
                this.Array = array;
            }

            public string Name { get; set; }

            public int Value { get; set; }

            public int[] Array { get; set; }
        }

        public class WithCalculatedProperty : INotifyPropertyChanged
        {
            private readonly int factor;
            private int value;

            public WithCalculatedProperty(int factor = 1)
            {
                this.factor = factor;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public int Value
            {
                get { return this.value; }
                set
                {
                    if (value == this.value) return;
                    this.value = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.CalculatedValue));
                }
            }

            public int CalculatedValue => this.Value * this.factor;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class WithComplexProperty
        {
            public WithComplexProperty()
            {
            }

            public WithComplexProperty(string name, int value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name { get; set; }

            public int Value { get; set; }

            public ComplexType ComplexType { get; set; }
        }

        public class WithListProperty<T>
        {
            public List<T> Items { get; set; } = new List<T>();
        }

        public class WithProperty<T>
        {
            public WithProperty()
            {
            }

            public WithProperty(T value)
            {
                this.Value = value;
            }

            public T Value { get; set; }
        }

        public class WithReadonlyField
        {
            public readonly int ReadonlyValue;
            public int Value;

            public WithReadonlyField(int readonlyValue, int value)
            {
                this.ReadonlyValue = readonlyValue;
                this.Value = value;
            }
        }

        public class WithReadonlyProperty<T>
        {
            public WithReadonlyProperty(T value)
            {
                this.Value = value;
            }

            public T Value { get; }
        }

        public class WithSimpleFields
        {
            internal static readonly TestComparer Comparer = new TestComparer();

            public WithSimpleFields()
            {
            }

            public WithSimpleFields(int intValue, int? nullableIntValue, string stringValue, StringSplitOptions enumValue)
            {
                this.IntValue = intValue;
                this.NullableIntValue = nullableIntValue;
                this.StringValue = stringValue;
                this.EnumValue = enumValue;
            }

            internal int IntValue;
            internal int? NullableIntValue;
            internal string StringValue;
            internal StringSplitOptions EnumValue;

            internal sealed class TestComparer : IEqualityComparer<WithSimpleFields>, IComparer
            {
                public bool Equals(WithSimpleFields x, WithSimpleFields y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return true;
                    }
                    if (ReferenceEquals(x, null))
                    {
                        return false;
                    }
                    if (ReferenceEquals(y, null))
                    {
                        return false;
                    }
                    if (x.GetType() != y.GetType())
                    {
                        return false;
                    }
                    return x.IntValue == y.IntValue && x.NullableIntValue == y.NullableIntValue && string.Equals(x.StringValue, y.StringValue) && x.EnumValue == y.EnumValue;
                }

                public int GetHashCode(WithSimpleFields obj)
                {
                    unchecked
                    {
                        var hashCode = obj.IntValue;
                        hashCode = (hashCode * 397) ^ obj.NullableIntValue.GetHashCode();
                        hashCode = (hashCode * 397) ^ (obj.StringValue != null
                                                           ? obj.StringValue.GetHashCode()
                                                           : 0);
                        hashCode = (hashCode * 397) ^ (int)obj.EnumValue;
                        return hashCode;
                    }
                }

                int IComparer.Compare(object x, object y)
                {
                    return this.Equals((WithSimpleFields)x, (WithSimpleFields)y)
                               ? 0
                               : -1;
                }
            }
        }

        public class WithSimpleProperties : INotifyPropertyChanged
        {
            internal static readonly TestComparer Comparer = new TestComparer();

            private int intValue;
            private int? nullableIntValue;
            private string stringValue;
            private StringSplitOptions enumValue;

            public WithSimpleProperties()
            {
            }

            public WithSimpleProperties(int intValue, int? nullableIntValue, string stringValue, StringSplitOptions enumValue)
            {
                this.intValue = intValue;
                this.nullableIntValue = nullableIntValue;
                this.stringValue = stringValue;
                this.enumValue = enumValue;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public int IntValue
            {
                get { return this.intValue; }
                set
                {
                    if (value == this.intValue) return;
                    this.intValue = value;
                    this.OnPropertyChanged();
                }
            }

            public int? NullableIntValue
            {
                get { return this.nullableIntValue; }
                set
                {
                    if (value == this.nullableIntValue) return;
                    this.nullableIntValue = value;
                    this.OnPropertyChanged();
                }
            }

            public string StringValue
            {
                get { return this.stringValue; }
                set
                {
                    if (value == this.stringValue) return;
                    this.stringValue = value;
                    this.OnPropertyChanged();
                }
            }

            public StringSplitOptions EnumValue
            {
                get { return this.enumValue; }
                set
                {
                    if (value == this.enumValue) return;
                    this.enumValue = value;
                    this.OnPropertyChanged();
                }
            }
            public void SetFields(int intValue, string stringValue)
            {
                this.intValue = intValue;
                this.stringValue = stringValue;
            }

            [NotifyPropertyChangedInvocator]
            public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            internal sealed class TestComparer : IEqualityComparer<WithSimpleProperties>, IComparer
            {
                public bool Equals(WithSimpleProperties x, WithSimpleProperties y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return true;
                    }
                    if (ReferenceEquals(x, null))
                    {
                        return false;
                    }
                    if (ReferenceEquals(y, null))
                    {
                        return false;
                    }
                    if (x.GetType() != y.GetType())
                    {
                        return false;
                    }
                    return x.IntValue == y.IntValue && x.NullableIntValue == y.NullableIntValue && string.Equals(x.StringValue, y.StringValue) && x.EnumValue == y.EnumValue;
                }

                public int GetHashCode(WithSimpleProperties obj)
                {
                    unchecked
                    {
                        var hashCode = obj.IntValue;
                        hashCode = (hashCode * 397) ^ obj.NullableIntValue.GetHashCode();
                        hashCode = (hashCode * 397) ^ (obj.StringValue?.GetHashCode() ?? 0);
                        hashCode = (hashCode * 397) ^ (int)obj.EnumValue;
                        return hashCode;
                    }
                }

                int IComparer.Compare(object x, object y)
                {
                    return this.Equals((WithSimpleProperties)x, (WithSimpleProperties)y)
                               ? 0
                               : -1;
                }
            }
        }


        public class WithIndexerType
        {
            // ReSharper disable once UnusedParameter.Global
            public int this[int index]
            {
                get { return 0; }
                set { }
            }
        }
    }
}
