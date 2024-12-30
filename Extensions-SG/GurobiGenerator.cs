﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Extensions_SG
{
    [Generator]
    public class GurobiGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                if (context.Compilation.GetTypeByMetadataName("Gurobi.GRBModel") is null
                    || context.Compilation.GetTypeByMetadataName("Gurobi.GRBVar") is null
                    || context.Compilation.GetTypeByMetadataName("Gurobi.GRBCallback") is null
                    || context.Compilation.GetTypeByMetadataName("Gurobi.GRBTempConstr") is null
                )
                    return;

                context.AddSource("Extensions-SG.GurobiExtensions.cs", SourceText.From(Assembly.GetExecutingAssembly().GetManifestResourceStream("Extensions-SG.GurobiExtensions.cs"), Encoding.UTF8, canBeEmbedded: true));
            }
            catch (Exception)
            {
                Debugger.Launch();
            }
        }
    }
}
