using CsharpSemanticMerger.Core.GitParsing;
using CsharpSemanticMerger.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CsharpSemanticMerger.Tests.GitParsingTests
{
    public sealed class GitParsingServiceTests
    {

        [Fact]
        public void Parse_SingleConflictInsideMethodBody_ProducesUnchangedAndConflictSegmentsWithLineNumbers()
        {
            string source = string.Join("\n", new[]
            {
                "public class Calculator",
                "{",
                "    public int Add(int a, int b)",
                "    {",
                "<<<<<<< HEAD",
                "        return a + b;",
                "=======",
                "        return checked(a + b);",
                ">>>>>>> feature/checked-math",
                "    }",
                "}"
            });

            List<FileSegment> segments = GitParsingService.ParseGitConflict(source);

            Assert.Equal(3, segments.Count);

            var before = Assert.IsType<UnchangedSegment>(segments[0]);
            Assert.Equal(0, before.StartLine);
            Assert.Equal(3, before.EndLine);
            Assert.Equal(2, before.Lines.Count);

            var conflict = Assert.IsType<ConflictSegment>(segments[1]);
            Assert.Equal(4, conflict.StartLine);
            Assert.Equal(8, conflict.EndLine);
            Assert.Equal(new[] { "        return a + b;" }, conflict.HeadLines);
            Assert.Equal(new[] { "        return checked(a + b);" }, conflict.IncomingLines);
            Assert.Empty(conflict.BaseLines);

            var after = Assert.IsType<UnchangedSegment>(segments[2]);
            Assert.Equal(9, after.StartLine);
            Assert.Equal(10, after.EndLine);
            Assert.Equal(new[] { "    }", "}" }, after.Lines);
        }

        [Fact]
        public void Parse_MultipleConflictsInSameClass_ParsesAllConflictsInOrder()
        {
            string source = string.Join("\n", new[]
            {
                "public class Service",
                "{",
                "    private readonly string _name =",
                "<<<<<<< HEAD",
                "        \"primary\";",
                "=======",
                "        \"secondary\";",
                ">>>>>>> incoming",
                "    public void Run()",
                "    {",
                "<<<<<<< HEAD",
                "        Console.WriteLine(\"A\");",
                "=======",
                "        Console.WriteLine(\"B\");",
                ">>>>>>> incoming",
                "    }",
                "}"
            });

            List<FileSegment> segments = GitParsingService.ParseGitConflict(source);
            List<ConflictSegment> conflicts = segments.OfType<ConflictSegment>().ToList();

            Assert.Equal(2, conflicts.Count);
            Assert.Equal(new[] { "        \"primary\";" }, conflicts[0].HeadLines);
            Assert.Equal(new[] { "        \"secondary\";" }, conflicts[0].IncomingLines);
            Assert.Equal(new[] { "        Console.WriteLine(\"A\");" }, conflicts[1].HeadLines);
            Assert.Equal(new[] { "        Console.WriteLine(\"B\");" }, conflicts[1].IncomingLines);
        }

        [Fact]
        public void Parse_Diff3ConflictFormat_CapturesHeadBaseAndIncomingLines()
        {
            string source = string.Join("\n", new[]
            {
                "public class Config",
                "{",
                "<<<<<<< HEAD",
                "    public int Timeout { get; } = 30;",
                "||||||| merged common ancestors",
                "    public int Timeout { get; } = 10;",
                "=======",
                "    public int Timeout { get; } = 60;",
                ">>>>>>> feature/timeout",
                "}"
            });

            List<FileSegment> segments = GitParsingService.ParseGitConflict(source);
            ConflictSegment conflict = Assert.Single(segments.OfType<ConflictSegment>());

            Assert.Equal(new[] { "    public int Timeout { get; } = 30;" }, conflict.HeadLines);
            Assert.Equal(new[] { "    public int Timeout { get; } = 10;" }, conflict.BaseLines);
            Assert.Equal(new[] { "    public int Timeout { get; } = 60;" }, conflict.IncomingLines);
        }

        [Fact]
        public void Parse_EmptyHeadSide_AllowsIncomingAdditionsWithEmptyHeadLines()
        {
            string source = string.Join("\n", new[]
            {
                "public class FeatureFlags",
                "{",
                "<<<<<<< HEAD",
                "=======",
                "    public bool EnableNewFlow => true;",
                ">>>>>>> feature/new-flow",
                "}"
            });

            List<FileSegment> segments = GitParsingService.ParseGitConflict(source);
            ConflictSegment conflict = Assert.Single(segments.OfType<ConflictSegment>());

            Assert.Empty(conflict.HeadLines);
            Assert.Equal(new[] { "    public bool EnableNewFlow => true;" }, conflict.IncomingLines);
        }

        [Fact]
        public void Parse_EmptyIncomingSide_AllowsHeadAdditionsWithEmptyIncomingLines()
        {
            string source = string.Join("\n", new[]
            {
                "public class FeatureFlags",
                "{",
                "<<<<<<< HEAD",
                "    public bool EnableLegacyFlow => true;",
                "=======",
                ">>>>>>> feature/remove-legacy",
                "}"
            });

            List<FileSegment> segments = GitParsingService.ParseGitConflict(source);
            ConflictSegment conflict = Assert.Single(segments.OfType<ConflictSegment>());

            Assert.Equal(new[] { "    public bool EnableLegacyFlow => true;" }, conflict.HeadLines);
            Assert.Empty(conflict.IncomingLines);
        }

        [Fact]
        public void Parse_FakeMarkersInsideStringLiteralsAndComments_DoesNotSplitConflictPrematurely()
        {
            string source = string.Join("\n", new[]
            {
                "public class MarkerStrings",
                "{",
                "    public string GetText()",
                "    {",
                "<<<<<<< HEAD",
                "        var fakeSep = \"=======\";",
                "        // ======= fake separator inside comment",
                "        return fakeSep;",
                "=======",
                "        var fakeEnd = \">>>>>>> not a real end marker\";",
                "        /* <<<<<<< also not a real start */",
                "        return fakeEnd;",
                ">>>>>>> incoming-branch",
                "    }",
                "}"
            });

            List<FileSegment> segments = GitParsingService.ParseGitConflict(source);
            ConflictSegment conflict = Assert.Single(segments.OfType<ConflictSegment>());

            Assert.Equal(3, conflict.HeadLines.Count);
            Assert.Contains(conflict.HeadLines, line => line.Contains("var fakeSep = \"=======\";"));
            Assert.Equal(3, conflict.IncomingLines.Count);
            Assert.Contains(conflict.IncomingLines, line => line.Contains("var fakeEnd = \">>>>>>> not a real end marker\";"));
        }

        [Theory]
        [InlineData("public class Broken\n{\n<<<<<<< HEAD\n    int x = 1;\n=======\n    int x = 2;\n}")]
        [InlineData("public class Broken\n{\n<<<<<<< HEAD\n    int x = 1;\n>>>>>>> incoming\n}")]
        public void Parse_MalformedConflictMarkers_ThrowsExceptionWithLineContext(string invalidSource)
        {
            Exception ex = Assert.ThrowsAny<Exception>(() => GitParsingService.ParseGitConflict(invalidSource));

            Assert.False(string.IsNullOrWhiteSpace(ex.Message));
            Assert.True(
                ex.Message.Contains("marker", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase));
        }
    }
}
