﻿namespace Gu.State
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Tracks changes in a graph.
    /// Listens to nested Property and collection changes.
    /// </summary>
    public sealed class ChangeTracker : IChangeTracker
    {
        private static readonly PropertyChangedEventArgs ChangesEventArgs = new PropertyChangedEventArgs(nameof(Changes));
        private readonly IRefCounted<ChangeNode> node;
        private bool disposed;

        public ChangeTracker(INotifyPropertyChanged source, PropertiesSettings settings)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(settings, nameof(settings));
            Track.Verify.IsTrackableType(source.GetType(), settings);
            this.Settings = settings;
            this.node = ChangeNode.GetOrCreate(this, source, settings);
            this.node.Tracker.Changed += this.OnNodeChange;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public event EventHandler Changed;

        public PropertiesSettings Settings { get; }

        /// <inheritdoc/>
        public int Changes => this.node.Tracker.Changes;

        /// <summary>
        /// Dispose(true); //I am calling you from Dispose, it's safe
        /// GC.SuppressFinalize(this); //Hey, GC: don't bother calling finalize later
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.node.Tracker.Changed -= this.OnNodeChange;
            this.node.RemoveOwner(this);
        }

        private void OnNodeChange(object sender, EventArgs e)
        {
            this.Changed?.Invoke(this, e);
            this.PropertyChanged?.Invoke(this, ChangesEventArgs);
        }
    }
}