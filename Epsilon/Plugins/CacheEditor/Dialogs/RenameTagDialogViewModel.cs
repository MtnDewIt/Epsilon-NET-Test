using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheEditor.ViewModels
{
    class RenameTagDialogViewModel : Screen
    {
        private string _name;
        private string _message;

        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        public string Message
        {
            get => _message;
            set => SetAndNotify(ref _message, value);
        }

        public void Confirm()
        {
            RequestClose(true);
        }

        public void Cancel()
        {
            RequestClose(false);
        }
    }
}
