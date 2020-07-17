using Stylet;
using System;
using System.Runtime.CompilerServices;

namespace EpsilonLib.Options
{
    public abstract class OptionPageBase : Screen, IOptionsPage
    {
        private bool _isLoading;

        public OptionPageBase(string category, string displayName)
        {
            Category = category;
            DisplayName = displayName;
        }

        public string Category { get; }

        public bool IsDirty { get; set; }

        protected override void OnViewLoaded()
        {
            _isLoading = true;
            Load();
            _isLoading = false;
        }

        protected virtual void SetOptionAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (SetAndNotify(ref field, value, propertyName))
                IsDirty = !_isLoading;
        }

        public abstract void Apply();

        public abstract void Load();
    }
}
