using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Windows;

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

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");

            AssemblyLoadContext.Default.Resolving += (AssemblyLoadContext ctx, AssemblyName name) =>
            {
                foreach (string path in searchPaths)
                {
                    string assemblyPath = Path.Combine(path, $"{name.Name}.dll");

                    AssemblyName an;

                    try
                    {
                        Assembly assembly = Assembly.LoadFile(assemblyPath);

                        an = new AssemblyName(assembly.GetName().Name);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    if (AssemblyName.ReferenceMatchesDefinition(name, an)) return ctx.LoadFromAssemblyPath(assemblyPath);
                }

                return null;
            };

            AssemblyLoadContext.Default.ResolvingUnmanagedDll += (Assembly assembly, string dllName) =>
            {
                foreach (string path in searchPaths)
                {
                    string dllPath = Path.Combine(path, dllName);

                    IntPtr handle;

                    try
                    {
                        handle = NativeLibrary.Load(dllPath);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    if (Path.GetFileName(dllPath) == dllName) return handle;
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
