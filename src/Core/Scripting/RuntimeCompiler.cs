using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

namespace ZargoEngine.Scripting
{
    public static class RuntimeCompiler
    {
        const string scripts = "Scripts/";

        public static void ScanBehaviours()
        {
            string directory = Directory.GetCurrentDirectory() + "../" + AssetManager.AssetsPath;
            string codePath  = directory + scripts + "FirstBehaviour.cs";

            string sourceCode = File.ReadAllText(codePath);

            var assemblyNames = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
                                                    .Select(a => a.Location).ToList();


            var dll = Directory.GetCurrentDirectory() + "\\" +"ImGui.NET.dll";
            
            assemblyNames.Add(dll);
            assemblyNames.ForEach(x => Debug.Log(x));

            CompilerParameters parameters = new CompilerParameters()
            { 
                
            };
            CSharpCodeProvider provider = new CSharpCodeProvider();

            parameters.ReferencedAssemblies.AddRange(assemblyNames.ToArray());
            
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, sourceCode);

            if (results.Errors.HasErrors)
            {
                Debug.Log("compilation failed");
                foreach (var item in results.Errors)
                {
                    Debug.Log(item);
                }
            }
            else
            {
                Debug.Log("compile Sucses");
            }
        }
    }
}
