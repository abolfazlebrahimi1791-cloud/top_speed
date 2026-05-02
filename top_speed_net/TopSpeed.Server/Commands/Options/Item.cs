using System;

namespace TopSpeed.Server.Commands.Options
{
    internal sealed class OptionItem
    {
        private readonly Func<string>? _valueFactory;
        private readonly Action _activate;

        public OptionItem(
            string key,
            string labelMessageId,
            OptionValueType type,
            Action activate,
            Func<string>? valueFactory = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Option key is required.", nameof(key));
            if (string.IsNullOrWhiteSpace(labelMessageId))
                throw new ArgumentException("Option label is required.", nameof(labelMessageId));

            Key = key.Trim();
            LabelMessageId = labelMessageId;
            Type = type;
            _activate = activate ?? throw new ArgumentNullException(nameof(activate));
            _valueFactory = valueFactory;
        }

        public string Key { get; }
        public string LabelMessageId { get; }
        public OptionValueType Type { get; }

        public void Activate()
        {
            _activate();
        }

        public string GetValueOrEmpty()
        {
            if (_valueFactory == null)
                return string.Empty;

            var value = _valueFactory();
            return value ?? string.Empty;
        }
    }
}
