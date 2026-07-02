using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace FinalLabSystem.Tests.Validation;

public class NatighFieldScanTests
{
    private static string FindSolutionDirectory(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return startDir;
    }

    private static List<(string FilePath, int LineNumber, string LineContent)> ScanForNatigh(
        string solutionDir,
        string searchPattern)
    {
        var results = new List<(string FilePath, int LineNumber, string LineContent)>();

        var files = Directory.GetFiles(solutionDir, searchPattern, SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.Combine("bin", "")) &&
                        !f.Contains(Path.Combine("obj", "")) &&
                        !f.Contains(Path.Combine(".git", "")) &&
                        !f.Contains(Path.Combine("node_modules", "")))
            .ToList();

        foreach (var file in files)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(file);
            }
            catch (Exception)
            {
                continue;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("Natigh", StringComparison.Ordinal))
                {
                    var relativePath = Path.GetRelativePath(solutionDir, file);
                    results.Add((relativePath, i + 1, lines[i]));
                }
            }
        }

        return results;
    }

    [Fact]
    public void NoCsFile_Contains_Natigh_AsFieldNameOrIdentifier()
    {
        var solutionDir = FindSolutionDirectory(AppContext.BaseDirectory);
        var productionDir = Path.Combine(solutionDir, "FinalLabSystem");
        var matches = ScanForNatigh(productionDir, "*.cs");

        if (matches.Count > 0)
        {
            var details = string.Join(Environment.NewLine,
                matches.Select(m => $"  {m.FilePath}:{m.LineNumber} -> {m.LineContent.Trim()}"));

            Assert.Fail(
                $"Found \"Natigh\" in {matches.Count} .cs file(s) under \"{productionDir}\":{Environment.NewLine}{details}{Environment.NewLine}" +
                $"This is a regression — Natigh-related fields/properties/columns are permanently excluded from project scope.");
        }
    }

    [Fact]
    public void NoXamlFile_Contains_Natigh_AsFieldNameOrIdentifier()
    {
        var solutionDir = FindSolutionDirectory(AppContext.BaseDirectory);
        var productionDir = Path.Combine(solutionDir, "FinalLabSystem");
        var matches = ScanForNatigh(productionDir, "*.xaml");

        if (matches.Count > 0)
        {
            var details = string.Join(Environment.NewLine,
                matches.Select(m => $"  {m.FilePath}:{m.LineNumber} -> {m.LineContent.Trim()}"));

            Assert.Fail(
                $"Found \"Natigh\" in {matches.Count} .xaml file(s) under \"{productionDir}\":{Environment.NewLine}{details}{Environment.NewLine}" +
                $"This is a regression — Natigh-related fields/properties/columns are permanently excluded from project scope.");
        }
    }
}
