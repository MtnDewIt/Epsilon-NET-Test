using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BlamScriptEditorPlugin
{
    /// <summary>
    /// Interaction logic for ScriptTagEditorView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ScriptTagEditorView : UserControl
    {
        public ScriptTagEditorView()
        {
            InitializeComponent();
            SearchPanel panel = SearchPanel.Install(ScriptSourceTextBox);

            SolidColorBrush brush = new((Color)ColorConverter.ConvertFromString("#40FF7810"));
            brush.Freeze();
            panel.MarkerBrush = brush;
            panel.MarkerCornerRadius = 0.0f;
        }

        private void ScriptKeyDownHandler(object sender, KeyEventArgs e)
        {
            bool controlModifier = e.KeyboardDevice.Modifiers == ModifierKeys.Control;

            switch (e.Key)
            {
                case Key.Z when controlModifier && sender is ScriptTextEditor:
                    e.Handled = true;
                    break;
                case Key.S when controlModifier:
                    SaveButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    break;
                default:
                    break;
            }
        }

        private void InsertText(string insert)
        {
            var cursorPosition = ScriptSourceTextBox.SelectionStart;
            ScriptSourceTextBox.Text = ScriptSourceTextBox.Text.Insert(ScriptSourceTextBox.SelectionStart, insert);
            ScriptSourceTextBox.SelectionStart = cursorPosition + insert.Length;
        }

        private void ScriptSourceTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }
    }
}
