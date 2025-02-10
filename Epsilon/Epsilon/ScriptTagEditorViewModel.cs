using Epsilon.TagEditing.Messages;
using Epsilon.Dialogs;
using Shared;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using TagTool.Cache;
using TagTool.Scripting;
using TagTool.Scripting.Compiler;
using TagTool.Tags.Definitions;

namespace Epsilon
{

	class ScriptTagEditorViewModel : TagEditorPlugin
    {

		private string _scriptSourceCode;

		public ScriptTagEditorViewModel(TagEditorContext context) : base(context) {
            TagEditorContext = context;
		}

		public string ScriptSourceCode
        {
            get => _scriptSourceCode;
            set => SetAndNotify(ref _scriptSourceCode, value);
        }

        public override void OnMessage(object sender, object message)
        {
            if(message is DefinitionDataChangedEvent e)
            {
                Definition = e?.NewData as Scenario;
                Dispatcher.CurrentDispatcher.InvokeAsync(DecompileAsync);
            }
        }

        public async Task LoadAsync() { await DecompileAsync(); }

        private async Task DecompileAsync()
        {
            try
            {
				if (TagEditorContext?.CacheEditor?.CacheFile?.Cache == null) { 
                    throw new InvalidOperationException("Tried to decompile scenario scripts using a null game cache."); 
                }
				ScriptDecompiler decompiler = new ScriptDecompiler(TagEditorContext.CacheEditor.CacheFile.Cache as GameCache, Definition as Scenario);
                ScriptSourceCode = await Task.Run(() =>
                {
                    using (StringWriter writer = new StringWriter())
                    {
                        decompiler.DecompileScripts(writer);
                        return writer.ToString();
                    }
                });
            }
            catch (Exception ex)
            {
                ScriptSourceCode = ex.ToString();
                //MessageBox.Show($"An exception occured while decompiling\n{ex}");
            }
        }

        public async void SaveAndCompile()
        {
            try
            {
                using (IProgressReporter progress = Shell.CreateProgressScope())
                {
                    progress.Report("Compiling script...");
					GameCache cache = Epsilon.CacheFile.Cache;
                    await CompileSourceCode(cache, Definition as Scenario, ScriptSourceCode);
                    progress.Report("Script Compiled and Tag Changes Saved", true, 1);
                    await Task.Delay(TimeSpan.FromSeconds(2.5)); // lol, wow
                }
            }
            catch (Exception ex)
            {
				AlertDialogViewModel error = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    Message = $"An exception occured while attempting to compile script\n{ex}"
                };
                Shell.ShowDialog(error);
            }
        }

        private async Task CompileSourceCode(GameCache cache, Scenario definition, string sourceCode)
        {
			FileInfo file = new FileInfo("tmp.txt");
            try
            {
                ScriptCompiler scriptCompiler = new ScriptCompiler(cache, definition);

                using (StreamWriter fs = new StreamWriter(file.OpenWrite()))
                    await fs.WriteAsync(sourceCode);

                await Task.Run(() => scriptCompiler.CompileFile(file));

                PostMessage(
                    this, 
                    new DefinitionDataChangedEvent(definition) {
						// This will equate to "Save" being clicked on the Definition tab
						DefinitionEditorSaveRequested = true 
                    }
                );

            }
            finally
            {
                if (file.Exists)
                    file.Delete();
            }
        }

        protected override void OnClose() { base.OnClose(); Definition = null; }
    }
}
