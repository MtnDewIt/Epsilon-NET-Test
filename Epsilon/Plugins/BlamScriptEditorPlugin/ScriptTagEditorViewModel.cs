using CacheEditor;
using CacheEditor.TagEditing;
using CacheEditor.TagEditing.Messages;
using Shared;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TagTool.Cache;
using TagTool.Scripting.Compiler;
using TagTool.Tags.Definitions;

namespace BlamScriptEditorPlugin
{
    class ScriptTagEditorViewModel : TagEditorPluginBase
    {
        private readonly IShell _shell;
        private readonly ICacheFile _cacheFile;
        private readonly Scenario _definition;
        public string ScriptSourceCode { get; set; }

        public ScriptTagEditorViewModel(IShell shell, ICacheFile cacheFile, Scenario definition)
        {
            _shell = shell;
            _cacheFile = cacheFile;
            _definition = definition;
            Load(cacheFile, definition);
        }

        protected override void OnMessage(object sender, object message)
        {
            if(message is DefinitionDataChangedEvent e)
            {
                Load(_cacheFile, e.NewData as Scenario);
            }
        }

        private async void Load(ICacheFile cacheFile, Scenario definition)
        {
            var decompiler = new ScriptDecompiler(cacheFile.Cache, definition);
            try
            {
                ScriptSourceCode = await Task.Run(() => decompiler.Decompile());
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
                using (var progress = _shell.CreateProgressScope())
                {
                    progress.Report("Compiling script...");
                    var cache = _cacheFile.Cache;
                    await CompileSourceCode(cache, _definition, ScriptSourceCode);
                    progress.Report("Script Compiled");
                    await Task.Delay(TimeSpan.FromSeconds(2.5));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception occured while attempting to compile script\n{ex}");
            }
        }

        private async Task CompileSourceCode(GameCache cache, Scenario definition, string sourceCode)
        {
            var file = new FileInfo("tmp.txt");
            try
            {
                ScriptCompiler scriptCompiler = new ScriptCompiler(_cacheFile.Cache, _definition);

                using (var fs = new StreamWriter(file.OpenWrite()))
                    await fs.WriteAsync(sourceCode);

                await Task.Run(() => scriptCompiler.CompileFile(file));

                PostMessage(this, new DefinitionDataChangedEvent(definition));
            }
            finally
            {
                if (file.Exists)
                    file.Delete();
            }
        }
    }
}
