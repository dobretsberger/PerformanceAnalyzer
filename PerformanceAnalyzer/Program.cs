using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Workspaces.MSBuild;
using Microsoft.Build.Locator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;

class LoopAnalyzer : CSharpSyntaxWalker
{
    private int currentDepth = 0;
    public List<(int Depth, int LineNumber)> NestedLoops { get; } = new List<(int Depth, int LineNumber)>();

    public override void VisitForStatement(ForStatementSyntax node)
    {
        currentDepth++;
        if (currentDepth > 2)
        {
            NestedLoops.Add((currentDepth, node.GetLocation().GetLineSpan().StartLinePosition.Line + 1));
        }
        base.VisitForStatement(node);
        currentDepth--;
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        currentDepth++;
        if (currentDepth > 2)
        {
            NestedLoops.Add((currentDepth, node.GetLocation().GetLineSpan().StartLinePosition.Line + 1));
        }
        base.VisitWhileStatement(node);
        currentDepth--;
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        currentDepth++;
        if (currentDepth > 2)
        {
            NestedLoops.Add((currentDepth, node.GetLocation().GetLineSpan().StartLinePosition.Line + 1));
        }
        base.VisitForEachStatement(node);
        currentDepth--;
    }

    public static async Task AnalyzeSolutionAsync(string solutionPath)
    {
        MSBuildLocator.RegisterDefaults();
        using var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath);

        foreach (var project in solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                var root = await document.GetSyntaxRootAsync();
                var analyzer = new LoopAnalyzer();
                analyzer.Visit(root);

                if (analyzer.NestedLoops.Any())
                {
                    Console.WriteLine($"File: {document.FilePath}");
                    foreach (var (depth, line) in analyzer.NestedLoops)
                    {
                        Console.WriteLine($"  Nested Loop Depth: {depth}, Line: {line}");
                    }
                }
            }
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        string solutionPath = "C:\\Git\\WTG\\Cargowise\\Dev\\Winzor\\Winzor.sln";
        await LoopAnalyzer.AnalyzeSolutionAsync(solutionPath);
    }
}
