using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TopSpeed.Server.Commands.Options
{
    internal sealed class OptionMenu
    {
        public OptionMenu(string titleMessageId, IReadOnlyList<OptionItem> items)
        {
            if (string.IsNullOrWhiteSpace(titleMessageId))
                throw new ArgumentException("Menu title is required.", nameof(titleMessageId));
            if (items == null || items.Count == 0)
                throw new ArgumentException("Menu must contain at least one item.", nameof(items));

            TitleMessageId = titleMessageId;
            Items = new ReadOnlyCollection<OptionItem>(items as List<OptionItem> ?? new List<OptionItem>(items));
        }

        public string TitleMessageId { get; }
        public IReadOnlyList<OptionItem> Items { get; }
    }
}
