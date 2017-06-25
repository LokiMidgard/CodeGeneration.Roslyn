﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CodeGeneration.Roslyn.Generate
{
    class Program
    {
        static int Main(string[] args)
        {
            IReadOnlyList<string> compile = Array.Empty<string>();
            IReadOnlyList<string> refs = Array.Empty<string>();
            IReadOnlyList<string> generatorSearchPaths = Array.Empty<string>();
            string generatedCompileItemFile = null;
            string outputDirectory = null;
            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOptionList("r|reference", ref refs, "Paths to assemblies being referenced");
                syntax.DefineOptionList("generatorSearchPath", ref generatorSearchPaths, "Paths to folders that may contain generator assemblies");
                syntax.DefineOption("out", ref outputDirectory, true, "The directory to write generated source files to");
                syntax.DefineOption("generatedFilesList", ref generatedCompileItemFile, "The path to the file to create with a list of generated source files");
                syntax.DefineParameterList("compile", ref compile, "Source files included in compilation");
            });

            if (!compile.Any())
            {
                Console.Error.WriteLine("No source files are specified.");
                return 1;
            }

            if (outputDirectory == null)
            {
                Console.Error.WriteLine("The output directory must be specified.");
                return 2;
            }

            var generator = new CompilationGenerator
            {
                Compile = compile,
                ReferencePath = refs,
                GeneratorAssemblySearchPaths = generatorSearchPaths,
                IntermediateOutputDirectory = outputDirectory,
            };
            generator.Generate();

            if (generatedCompileItemFile != null)
            {
                File.WriteAllLines(generatedCompileItemFile, generator.GeneratedFiles);
            }

            foreach (var file in generator.GeneratedFiles)
            {
                Console.WriteLine(file);
            }

            return 0;
        }
    }
}