using System.Reflection;
using System.Runtime.Loader;
using System;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace WpfApp20
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            string[] searchPaths = 
            [
                AppContext.BaseDirectory, 
                Path.Combine(AppContext.BaseDirectory, "Tools")
            ];

            AssemblyLoadContext.Default.Resolving += (AssemblyLoadContext ctx, AssemblyName name) =>
            {
                foreach (string path in searchPaths)
                {
                    try
                    {
                        string assemblyPath = Path.Combine(path, $"{name.Name}.dll");
                        if(File.Exists(assemblyPath))
                            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    }
                    catch (Exception)
                    {
                        // swallow
                    }
                }

                return null;
            };

            AssemblyLoadContext.Default.ResolvingUnmanagedDll += (Assembly assembly, string dllName) =>
            {
                foreach (string path in searchPaths)
                {
                    try
                    {
                        string dllPath = Path.Combine(path, dllName);
                        if(File.Exists(dllPath))
                            return NativeLibrary.Load(dllPath);
                    }
                    catch (Exception)
                    {
                        // swallow
                    }
                }

                return IntPtr.Zero;
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
