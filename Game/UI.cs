using Nez.UI;
using System;

namespace Game
{
    class ConfigurableButton : Button
    {
        public event Action<ConfigurableButton> OnFocusedEvent;

        public ConfigurableButton(ButtonStyle style)
            : base(style)
        {
        }

        protected override void OnFocused()
        {
            base.OnFocused();
            OnFocusedEvent?.Invoke(this);
        }
    }

    class ConfigurableTextButton : TextButton
    {
        public event Action<ConfigurableTextButton> OnFocusedEvent;

        public ConfigurableTextButton(string text, TextButtonStyle style)
            : base(text, style)
        {
        }

        protected override void OnFocused()
        {
            base.OnFocused();
            OnFocusedEvent?.Invoke(this);
        }
    }
}
