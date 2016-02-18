namespace Gu.ChangeTracking
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// This class tracks property changes in source and keeps target in sync
    /// </summary>
    public class PropertySynchronizer<T> : IDisposable
        where T : class, INotifyPropertyChanged
    {
        private readonly T source;
        private readonly T target;
        private readonly ItemSynchronizerCollection itemSynchronizers = new ItemSynchronizerCollection();
        private readonly Lazy<Dictionary<object, PropertySynchronizer<INotifyPropertyChanged>>> propertySynchronizers =
            new Lazy<Dictionary<object, PropertySynchronizer<INotifyPropertyChanged>>>(
                () => new Dictionary<object, PropertySynchronizer<INotifyPropertyChanged>>());

        public PropertySynchronizer(T source, T target, ReferenceHandling referenceHandling)
            : this(source, target, Constants.DefaultPropertyBindingFlags, referenceHandling)
        {
        }

        public PropertySynchronizer(T source, T target, BindingFlags bindingFlags, ReferenceHandling referenceHandling)
            : this(source, target, new CopyPropertiesSettings(null, bindingFlags, referenceHandling))
        {
        }

        public PropertySynchronizer(T source, T target, params string[] ignoreProperties)
            : this(source, target, Constants.DefaultPropertyBindingFlags, ignoreProperties)
        {
        }

        public PropertySynchronizer(T source, T target, BindingFlags bindingFlags, params string[] ignoreProperties)
            : this(source,
                target,
                new CopyPropertiesSettings(
                    source?.GetType().GetIgnoreProperties(bindingFlags, ignoreProperties),
                    bindingFlags,
                    ReferenceHandling.Throw))
        {
        }

        public PropertySynchronizer(T source, T target, CopyPropertiesSettings settings)
        {
            Ensure.NotSame(source, target, nameof(source), nameof(target));
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(target, nameof(target));
            Ensure.SameType(source, target, nameof(source), nameof(target));
            Copy.VerifyCanCopyPropertyValues<T>(settings);
            this.Settings = settings;
            this.target = target;
            this.source = source;
            var notifyCollectionChanged = source as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
            {
                notifyCollectionChanged.CollectionChanged += this.OnSourceCollectionChanged;
                Copy.PropertyValues(source, target, settings);
                this.ResetItemSynchronizers();
            }
            else
            {
                this.source.PropertyChanged += this.OnSourcePropertyChanged;
                Copy.PropertyValues(source, target, settings);
                foreach (var propertyInfo in this.source.GetType().GetProperties(this.Settings.BindingFlags))
                {
                    if (this.Settings.IsIgnoringProperty(propertyInfo) ||
                        this.Settings.GetSpecialCopyProperty(propertyInfo) != null)
                    {
                        continue;
                    }

                    if (!Copy.IsCopyableType(propertyInfo.PropertyType))
                    {
                        this.UpdateSubPropertySynchronizer(propertyInfo);
                    }
                }
            }
        }

        public CopyPropertiesSettings Settings { get; }

        public void Dispose()
        {
            this.source.PropertyChanged -= this.OnSourcePropertyChanged;
            if (this.propertySynchronizers.IsValueCreated)
            {
                foreach (var propertySynchronizer in this.propertySynchronizers.Value)
                {
                    propertySynchronizer.Value.Dispose();
                }
            }

            var notifyCollectionChanged = this.source as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
            {
                notifyCollectionChanged.CollectionChanged -= this.OnSourceCollectionChanged;
            }

            this.itemSynchronizers?.Dispose();

        }

        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                Copy.PropertyValues(this.source, this.target, this.Settings);
                return;
            }

            var propertyInfo = this.source.GetType()
                                   .GetProperty(e.PropertyName, this.Settings.BindingFlags);
            if (propertyInfo == null)
            {
                return;
            }

            if (this.Settings.IsIgnoringProperty(propertyInfo))
            {
                return;
            }

            var specialCopyProperty = this.Settings.GetSpecialCopyProperty(propertyInfo);
            if (specialCopyProperty != null)
            {
                specialCopyProperty.CopyValue(this.source, this.target);
                return;
            }

            if (!Copy.IsCopyableType(propertyInfo.PropertyType))
            {
                Copy.PropertyValues(this.source, this.target, this.Settings);
                this.UpdateSubPropertySynchronizer(propertyInfo);
            }
            else
            {
                Copy.PropertyValue(this.source, this.target, propertyInfo);
            }
        }

        private void UpdateSubPropertySynchronizer(PropertyInfo propertyInfo)
        {
            var sv = (INotifyPropertyChanged)propertyInfo.GetValue(this.source);
            var tv = (INotifyPropertyChanged)propertyInfo.GetValue(this.target);
            var synchronizers = this.propertySynchronizers.Value;
            PropertySynchronizer<INotifyPropertyChanged> synchronizer;
            if (synchronizers.TryGetValue(propertyInfo, out synchronizer))
            {
                synchronizer?.Dispose();
            }

            synchronizers[propertyInfo] = this.CreateSynchronizer(sv, tv);
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var sv = e.NewItems[i];
                        object tv = null;
                        if (sv != null)
                        {
                            if (Copy.IsCopyableType(sv.GetType()))
                            {
                                tv = sv;
                            }
                            else
                            {
                                tv = Activator.CreateInstance(sv.GetType(), true);
                                Copy.PropertyValues(sv, tv, this.Settings);
                            }
                        }

                        var index = e.NewStartingIndex + i;
                        ((IList)this.target).Insert(index, tv);
                        this.UpdateItemSynchronizer(index, NotifyCollectionChangedAction.Add);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var index = e.OldStartingIndex + i;
                        ((IList)this.target).RemoveAt(index);
                        this.UpdateItemSynchronizer(index, NotifyCollectionChangedAction.Remove);
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    ((IList)this.target)[e.NewStartingIndex] = ((IList)this.source)[e.NewStartingIndex];
                    this.UpdateItemSynchronizer(e.NewStartingIndex, NotifyCollectionChangedAction.Replace);
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.target.GetType().GetMethod("Move", new[] { typeof(int), typeof(int) }).Invoke(this.target, new object[] { e.OldStartingIndex, e.NewStartingIndex });
                    this.UpdateItemSynchronizer(e.OldStartingIndex, NotifyCollectionChangedAction.Move, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Copy.PropertyValues(this.source, this.target, this.Settings);
                    this.ResetItemSynchronizers();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResetItemSynchronizers()
        {
            var sourceList = (IList)this.source;
            for (int i = 0; i < sourceList.Count; i++)
            {
                this.UpdateItemSynchronizer(i, NotifyCollectionChangedAction.Reset);
            }
        }

        private void UpdateItemSynchronizer(int index, NotifyCollectionChangedAction action, int toIndex = -1)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var synchronizer = this.CreateSynchronizer(index);
                        this.itemSynchronizers.Insert(index, synchronizer);
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    this.itemSynchronizers.RemoveAt(index);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        var synchronizer = this.CreateSynchronizer(index);
                        this.itemSynchronizers[index] = synchronizer;
                        break;
                    }

                case NotifyCollectionChangedAction.Move:
                    this.itemSynchronizers.Move(index, toIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        var synchronizer = this.CreateSynchronizer(index);
                        if (this.itemSynchronizers.Count > index)
                        {
                            this.itemSynchronizers[index] = synchronizer;
                        }
                        else
                        {
                            this.itemSynchronizers.Insert(index, synchronizer);
                        }

                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private PropertySynchronizer<INotifyPropertyChanged> CreateSynchronizer(int index)
        {
            return this.CreateSynchronizer(((IList)this.source)[index], ((IList)this.target)[index]);
        }

        private PropertySynchronizer<INotifyPropertyChanged> CreateSynchronizer(object sv, object tv)
        {
            if (sv == null || Copy.IsCopyableType(sv.GetType()))
            {
                return null;
            }

            if (this.Settings.ReferenceHandling == ReferenceHandling.Throw)
            {
                throw new NotSupportedException("Specify how to handle reference types using ReferenceHandling");
            }

            return new PropertySynchronizer<INotifyPropertyChanged>((INotifyPropertyChanged)sv, (INotifyPropertyChanged)tv, this.Settings);
        }
    }
}