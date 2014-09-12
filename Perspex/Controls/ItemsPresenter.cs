﻿// -----------------------------------------------------------------------
// <copyright file="ItemsControl.cs" company="Steven Kirk">
// Copyright 2014 MIT Licence. See licence.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Perspex.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Perspex.Layout;

    public class ItemsPresenter : Control, IVisual
    {
        public static readonly PerspexProperty<IEnumerable> ItemsProperty =
            ItemsControl.ItemsProperty.AddOwner<ItemsPresenter>();

        public static readonly PerspexProperty<ItemsPanelTemplate> ItemsPanelProperty =
            ItemsControl.ItemsPanelProperty.AddOwner<ItemsPresenter>();

        public static readonly PerspexProperty<DataTemplate> ItemTemplateProperty =
            ItemsControl.ItemTemplateProperty.AddOwner<ItemsPresenter>();

        private Panel panel;

        public ItemsPresenter()
        {
            this.GetObservable(ItemsProperty).Subscribe(this.ItemsChanged);
        }

        public IEnumerable Items
        {
            get { return this.GetValue(ItemsProperty); }
            set { this.SetValue(ItemsProperty, value); }
        }

        public ItemsPanelTemplate ItemsPanel
        {
            get { return this.GetValue(ItemsPanelProperty); }
            set { this.SetValue(ItemsPanelProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        IEnumerable<IVisual> IVisual.ExistingVisualChildren
        {
            get { return Enumerable.Repeat(this.panel, this.panel != null ? 1 : 0); }
        }

        IEnumerable<IVisual> IVisual.VisualChildren
        {
            get { return Enumerable.Repeat(this.GetPanel(), 1); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Panel panel = this.GetPanel();
            panel.Measure(availableSize);
            return panel.DesiredSize.Value;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.GetPanel().Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override DataTemplate FindDataTemplate(object content)
        {
            TabItem tabItem = content as TabItem;

            if (tabItem != null)
            {
                return new DataTemplate(_ => tabItem);
            }
            else
            {
                return this.ItemTemplate ?? base.FindDataTemplate(content);
            }
        }

        private IEnumerable<Control> CreateItems(IEnumerable items)
        {
            if (items != null)
            {
                return items
                    .Cast<object>()
                    .Select(x => this.GetDataTemplate(x).Build(x))
                    .OfType<Control>();
            }
            else
            {
                return Enumerable.Empty<Control>();
            }
        }

        private Panel GetPanel()
        {
            if (this.panel == null && this.ItemsPanel != null)
            {
                this.panel = this.ItemsPanel.Build();
                ((IVisual)this.panel).VisualParent = this;
                this.ItemsChanged(this.Items);
            }

            return this.panel;
        }

        private void ItemsChanged(IEnumerable items)
        {
            if (this.panel != null)
            {
                this.panel.Children = new PerspexList<Control>(this.CreateItems(items));

                if (this.panel.Children.Count > 0)
                {
                    ((TabItem)this.panel.Children[0]).Classes.Add(":selected");
                }
            }
        }
    }
}
