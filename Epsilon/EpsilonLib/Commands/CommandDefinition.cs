using System;
using System.Globalization;
using System.Windows.Input;

namespace EpsilonLib.Commands
{
    public abstract class CommandDefinition
    {
        public virtual bool VisibleByDefault { get; } = true; 
        public abstract string Name { get; }
        public abstract string DisplayText { get; }
        public virtual KeyShortcut KeyShortcut { get; } = KeyShortcut.None;
    }

    public class KeyShortcut
    {
        public KeyGesture KeyGesture { get; }

        private KeyShortcut()
        {

        }

        public KeyShortcut(ModifierKeys modifierKeys, Key key)
        {
            KeyGesture = new KeyGesture(key, modifierKeys);
        }

        public static readonly KeyShortcut None = new KeyShortcut();

        public override string ToString()
        {
            if (KeyGesture == null)
                return "None";

            string cultureDisplay = KeyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);

            string replace = KeyGesture.Key switch
            {
                Key.OemQuestion => "/",
                _ => null
            };

            if (replace != null)
                return cultureDisplay.Replace(KeyGesture.Key.ToString(), replace);

            return cultureDisplay;
        }

        public static bool TryGetNumberKey(int index, out Key key)
        {
            if (Enum.TryParse($"D{index}", out key))
                return true;

            return false;
        }

        public static bool TryGetNumpadKey(int index, out Key key)
        {
            if (Enum.TryParse($"NumPad{index}", out key))
                return true;

            return false;
        }
    }
}