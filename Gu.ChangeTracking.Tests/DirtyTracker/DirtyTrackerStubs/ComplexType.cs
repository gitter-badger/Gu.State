﻿namespace Gu.ChangeTracking.Tests.DirtyTrackerStubs
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    public class ComplexType : INotifyPropertyChanged
    {
        public static readonly TestComparer Comparer = new TestComparer();
        private string name;
        private int value;

        public ComplexType()
        {
        }

        public ComplexType(string name, int value)
        {
            this.Name = name;
            this.Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == this.name)
                {
                    return;
                }
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (value == this.value)
                {
                    return;
                }
                this.value = value;
                this.OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


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
}