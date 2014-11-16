using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CustomChrome
{
    public class VisualStudioButtonCollection : Collection<VisualStudioButton>
    {
        private readonly VisualStudioFormChrome _chrome;

        public VisualStudioButtonCollection(VisualStudioFormChrome chrome)
        {
            if (chrome == null)
                throw new ArgumentNullException("chrome");

            _chrome = chrome;
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.Chrome = null;
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, VisualStudioButton item)
        {
            if (item.Chrome != null)
                throw new InvalidOperationException("Button cannot be added to two collections");

            item.Chrome = _chrome;

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].Chrome = null;

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, VisualStudioButton item)
        {
            if (item.Chrome != null)
                throw new InvalidOperationException("Button cannot be added to two collections");

            this[index].Chrome = null;
            item.Chrome = _chrome;

            base.SetItem(index, item);
        }
    }
}
