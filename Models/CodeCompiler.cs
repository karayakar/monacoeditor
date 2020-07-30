using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using monacoEditorCSharp.DataHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace monacoEditorCSharp.Models
{
    public static class Extension
    {
        public static T[] Concatenate<T>(this T[] first, T[] second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }

            return first.Concat(second).ToArray();
        }
    }
    public static class CSharpScriptCompiler
    {
        private static MetadataReference[] _ref;


        public static object Compile(SourceInfo sourceInfo)
        {
            var usings = new List<string>();
            var allusingsInCode = sourceInfo.SourceCode.Split(new string[] { "using " }, StringSplitOptions.None);
            foreach (var item in allusingsInCode)
            {
                if (!String.IsNullOrWhiteSpace(item))
                {
                    usings.Add(item.Split(';')[0]);
                }
            }
            //var scriptOptions = ScriptOptions.Default.AddImports(usings);
            DownloadNugetPackages.DownloadAllPackages(sourceInfo.Nuget);
            var assemblies = DownloadNugetPackages.LoadPackages(sourceInfo.Nuget);
            ScriptOptions.Default.AddReferences(assemblies);
            var scriptOptions = ScriptOptions.Default.AddReferences(usings).AddReferences(assemblies);

            string result = "";
            try
            {
                result = JsonConvert.SerializeObject(CSharpScript.EvaluateAsync(sourceInfo.SourceCode, scriptOptions).Result);
                var r = JsonConvert.SerializeObject(CSharpScript.RunAsync(sourceInfo.SourceCode, scriptOptions).Result);
                if (String.IsNullOrWhiteSpace(result) || result == "null")
                {
                    result = r;
                }
            }
            catch (Exception ex)
            {

                result = ex.Message;
            }
            return result;


        }

        public static dynamic CompileRos(SourceInfo sourceInfo)
        {
            var usings = new List<string>();
            var allusingsInCode = sourceInfo.SourceCode.Split(new string[] { "using " }, StringSplitOptions.None);
            foreach (var item in allusingsInCode)
            {
                if (!String.IsNullOrWhiteSpace(item))
                {
                    usings.Add(item.Split(';')[0]);
                }
            }

            List<Assembly> assemblies = new List<Assembly>
        {
            Assembly.Load("Microsoft.CodeAnalysis"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
            Assembly.Load("Microsoft.CodeAnalysis.Features"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
        };

            var assembliesList = DownloadNugetPackages.LoadPackages(sourceInfo.Nuget);
            foreach (var item in assembliesList)
            {
                var fname = item.FullName.Split(',')[0];
                if (assemblies.Where(x => x.FullName.Split(',')[0] == fname).FirstOrDefault() == null)
                {
                    try
                    {

                        var asm = item.FullName.Split(',')[0];
                        var loadAssembly = Assembly.Load(asm);
                        assemblies.Add(loadAssembly);
                    }
                    catch (Exception)
                    {
                        //assemblies.Add(item);
                    }
                }
            }

            var assembliest = AppDomain.CurrentDomain.GetAssemblies()
                      .Where(a => !a.IsDynamic && File.Exists(a.Location));
            //MefHostServices.DefaultAssemblies.AddRange (assembliest);

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()?
                    .SelectMany(x => x?.GetTypes())?
                    .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                //.WithAssemblies(assemblies)
                //.WithAssemblies(assembliest)
                .CreateContainer();

            var host = MefHostServices.Create(compositionContext);

            var workspace = new AdhocWorkspace(host);

            var scriptCode = sourceInfo.SourceCode;// "Guid.N";

            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: usings);

            if (_ref == null || _ref.Length == 0)
            {



                //.GroupBy(a => a.GetName().Name)
                //.Select(grp =>
                //     grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version)))
                //.Select(a => MetadataReference.CreateFromFile(a.Location));


                MetadataReference[] _refs2 = assembliest
                 .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                 .ToArray();

                MetadataReference[] _refs = assembliesList
                 .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                 .ToArray();


                _ref = new MetadataReference[0];

                try
                {

                    try
                    {
                        foreach (var item in DependencyContext.Default.CompileLibraries)
                        {
                            try
                            {
                                var arr = item.ResolveReferencePaths().Select(asm => MetadataReference.CreateFromFile(asm))?
                         .ToArray();
                                _ref = _ref.Concatenate(arr);
                            }
                            catch (Exception)
                            {


                            }

                        }

                    }
                    catch (Exception)
                    {

                    }



                }
                catch (Exception)
                {

                }
                _ref = _ref.Concatenate(_refs);
                _ref = _ref.Concatenate(_refs2);

            }
            if (_ref != null && _ref.Length > 0)
            {
                _ref = _ref.Concatenate(new[]
                 {
                  MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                  });
                var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
                    .WithMetadataReferences(_ref)
                   .WithCompilationOptions(compilationOptions);


                var scriptProject = workspace.AddProject(scriptProjectInfo);
                var scriptDocumentInfo = DocumentInfo.Create(
                    DocumentId.CreateNewId(scriptProject.Id), "Script",
                    sourceCodeKind: SourceCodeKind.Script,
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(scriptCode), VersionStamp.Create())));
                var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

                // cursor position is at the end
                var position = sourceInfo.LineNumberOffsetFromTemplate;// scriptCode.Length - 1;

                var completionService = CompletionService.GetService(scriptDocument);

                var results = completionService.GetCompletionsAsync(scriptDocument, position).Result;
                if (results == null && sourceInfo.LineNumberOffsetFromTemplate < sourceInfo.SourceCode.Length)
                {

                    sourceInfo.LineNumberOffsetFromTemplate++;
                    CompileRos(sourceInfo);
                }

                if (sourceInfo.SourceCode[sourceInfo.LineNumberOffsetFromTemplate - 1].ToString() == "(")
                {
                    sourceInfo.LineNumberOffsetFromTemplate--;
                    //CompileRos(sourceInfo);
                    results = completionService.GetCompletionsAsync(scriptDocument, sourceInfo.LineNumberOffsetFromTemplate).Result;


                }
                //Method parameters

                List<string> overloads = GetMethodParams(scriptCode, position);
                if (overloads != null && overloads.Count > 0)
                {

                    ImmutableArray<CompletionItem>.Builder builder = ImmutableArray.CreateBuilder<CompletionItem>();
                    foreach (var item in overloads)
                    {
                        string DisplayText = item;
                        string insertText = item.Split('(')[1].Split(')')[0];
                        CompletionItem ci = CompletionItem.Create(insertText, insertText, insertText);
                        builder.Add(ci);

                    }

                    if (builder.Count > 0)
                    {
                        ImmutableArray<CompletionItem> itemlist = builder.ToImmutable();
                        return CompletionList.Create(new Microsoft.CodeAnalysis.Text.TextSpan(), itemlist);

                    }
                }

                return results;
            }
            return null;
        }

        public static dynamic CompileRos2(SourceInfo sourceInfo)
        {

            List<Assembly> assemblies = new List<Assembly>
        {
            Assembly.Load("Microsoft.CodeAnalysis"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
            Assembly.Load("Microsoft.CodeAnalysis.Features"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
        };

            var assembliesList = DownloadNugetPackages.LoadPackages(sourceInfo.Nuget);
            foreach (var item in assembliesList)
            {
                var fname = item.FullName.Split(',')[0];
                if (assemblies.Where(x => x.FullName.Split(',')[0] == fname).FirstOrDefault() == null)
                {
                    var loadAssembly = Assembly.Load(item.FullName.Split(',')[0]);
                    assemblies.Add(loadAssembly);
                }
            }

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()
                    .SelectMany(x => x.GetTypes())
                    .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                //.WithAssemblies(assemblies)
                .CreateContainer();

            var host = MefHostServices.Create(compositionContext);

            var workspace = new AdhocWorkspace(host);

            var scriptCode = sourceInfo.SourceCode;// "Guid.N";

            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

            var usings = new List<string>();
            var allusingsInCode = sourceInfo.SourceCode.Split(new string[] { "using " }, StringSplitOptions.None);
            foreach (var item in allusingsInCode)
            {
                if (!String.IsNullOrWhiteSpace(item))
                {
                    usings.Add(item.Split(';')[0]);
                }
            }




            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: usings);

            MetadataReference[] _ref =
              DependencyContext.Default.CompileLibraries
              .First(cl => cl.Name == "mssqlrestapi")
              .ResolveReferencePaths()
              .Select(asm => MetadataReference.CreateFromFile(asm))
              .ToArray();

            MetadataReference[] _refs = assembliesList

              .Select(asm => MetadataReference.CreateFromFile(asm.Location))
              .ToArray();

            MetadataReference[] newArray = new MetadataReference[_ref.Length + _refs.Length];
            Array.Copy(_ref, newArray, _ref.Length);
            Array.Copy(_refs, 0, newArray, _ref.Length, _refs.Length);
            //assembliesList
            //IMethodSymbol methodWithGenericTypeArgsSymbol =    simpleClassToAnalyze.GetMembers("MySimpleMethod").FirstOrDefault()   as IMethodSymbol;
            //var genericMethodSignature = methodWithGenericTypeArgsSymbol.Parameters;

            var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
                .WithMetadataReferences(newArray)
               .WithCompilationOptions(compilationOptions);

            // without .net asseblies
            // .WithMetadataReferences(new[]
            // {
            //  MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            //  })

            var scriptProject = workspace.AddProject(scriptProjectInfo);
            var scriptDocumentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(scriptProject.Id), "Script",
                sourceCodeKind: SourceCodeKind.Script,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(scriptCode), VersionStamp.Create())));
            var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

            // cursor position is at the end
            var position = sourceInfo.LineNumberOffsetFromTemplate;// scriptCode.Length - 1;

            var completionService = CompletionService.GetService(scriptDocument);

            var results = completionService.GetCompletionsAsync(scriptDocument, position).Result;
            if (results == null && sourceInfo.LineNumberOffsetFromTemplate < sourceInfo.SourceCode.Length)
            {
                sourceInfo.LineNumberOffsetFromTemplate++;
                CompileRos2(sourceInfo);
            }

            //Method parameters

            List<string> overloads = GetMethodParams(scriptCode, position);
            if (overloads != null && overloads.Count > 0)
            {

                ImmutableArray<CompletionItem>.Builder builder = ImmutableArray.CreateBuilder<CompletionItem>();
                foreach (var item in overloads)
                {
                    string DisplayText = item;
                    string insertText = item.Split('(')[1].Split(')')[0];
                    CompletionItem ci = CompletionItem.Create(insertText, insertText, insertText);
                    builder.Add(ci);

                }

                if (builder.Count > 0)
                {
                    ImmutableArray<CompletionItem> itemlist = builder.ToImmutable();
                    return CompletionList.Create(new Microsoft.CodeAnalysis.Text.TextSpan(), itemlist);

                }
            }

            return results;// JsonConvert.SerializeObject(results);
        }

        public static dynamic FormatCode(SourceInfo sourceInfo)
        {

            var assemblies = new[]
       {
            Assembly.Load("Microsoft.CodeAnalysis"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
            Assembly.Load("Microsoft.CodeAnalysis.Features"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
        };

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()
                    .SelectMany(x => x.GetTypes())
                    .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();

            var host = MefHostServices.Create(compositionContext);

            var workspace = new AdhocWorkspace(host);
            var sourceLanguage = new CSharpLanguage();

            SyntaxTree syntaxTree = sourceLanguage.ParseText(sourceInfo.SourceCode, SourceCodeKind.Script);
            var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
            return Formatter.Format(root, workspace).ToFullString();
        }

        public static List<string> GetMethodParams(string scriptCode, int position)
        {
            //position = position - 2;
            List<string> overloads = new List<string>();
            var meta = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            string[] assemblies = meta.ToString().Split(';', StringSplitOptions.None);
            var sourceLanguage = new CSharpLanguage();
            //SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(scriptCode);
            SyntaxTree syntaxTree = sourceLanguage.ParseText(scriptCode, SourceCodeKind.Script);
            var root = (CompilationUnitSyntax)syntaxTree.GetRoot();

            try
            {
                if (_ref == null)
                {
                    _ref = _ref = new MetadataReference[0];


                    var assembliest = AppDomain.CurrentDomain.GetAssemblies()
                          .Where(a => !a.IsDynamic && File.Exists(a.Location));


                    MetadataReference[] _refs2 = assembliest
                     .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                     .ToArray();




                    try
                    {
                        foreach (var item in DependencyContext.Default.CompileLibraries)
                        {
                            try
                            {
                                var arr = item.ResolveReferencePaths().Select(asm => MetadataReference.CreateFromFile(asm))?
                         .ToArray();
                                _ref = _ref.Concatenate(arr);
                            }
                            catch (Exception)
                            {


                            }

                        }

                    }
                    catch (Exception)
                    {

                    }



                    _ref = _ref.Concatenate(_refs2);
                }

                var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var compilation = CSharpCompilation.Create("MyCompilation",
                    syntaxTrees: new[] { syntaxTree }, references: _ref);

                var model = compilation.GetSemanticModel(syntaxTree);

                var theToken = syntaxTree.GetRoot().FindToken(position);
                var theNode = theToken.Parent;
                while (!theNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression))
                {
                    theNode = theNode.Parent;
                    if (theNode == null) break; // There isn't an InvocationExpression in this branch of the tree
                }

                if (theNode == null)
                {
                    overloads = null;
                }
                else
                {
                    var symbolInfo = model.GetSymbolInfo(theNode);
                    var symbol = symbolInfo.Symbol;
                    var containingType = symbol?.ContainingType;

                    //overloads = containingType?.GetMembers(symbol.Name);
                    if (symbolInfo.CandidateSymbols != null)
                    {
                        if (symbolInfo.CandidateSymbols.Length > 0)
                        {
                            foreach (var parameters in symbolInfo.CandidateSymbols)
                            {
                                var i = parameters.ToMinimalDisplayParts(model, position);
                                if (parameters.Kind == SymbolKind.Method)
                                {
                                    var mp = parameters.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                                    //var smpl = parameters.ToMinimalDisplayString();
                                    overloads.Add(mp);

                                }

                            }

                        }
                    }
                }

                return overloads;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public class CSharpLanguage : ILanguageService
        {
            private static readonly LanguageVersion MaxLanguageVersion = Enum
                .GetValues(typeof(LanguageVersion))
                .Cast<LanguageVersion>()
                .Max();

            public SyntaxTree ParseText(string sourceCode, SourceCodeKind kind)
            {
                var options = new CSharpParseOptions(kind: kind, languageVersion: MaxLanguageVersion);

                // Return a syntax tree of our source code
                return CSharpSyntaxTree.ParseText(sourceCode, options);
            }

            public Compilation CreateLibraryCompilation(string assemblyName, bool enableOptimisations)
            {
                throw new NotImplementedException();
            }
        }
    }
}
