using System;
using System.Collections.Generic;
using CsharpSemanticMerger.Core.Models;
using CsharpSemanticMerger.Core.Reconstruction;
using Xunit;

namespace CsharpSemanticMerger.Tests.ReconstructionTests
{
    public sealed class SourceReconstructionServiceTests
    {
        private readonly SourceReconstructionService reconstructor = new();

        [Fact]
        public void Reconstruct_ConflictWithoutMarkersInOutput_PreservesIndentationAndSeparatesSides()
        {
            var segments = new List<FileSegment>
            {
                new UnchangedSegment(
                    new[]
                    {
                        "public class Greeter",
                        "{",
                        "    // Leading comment trivia",
                        "    public string SayHi()",
                        "    {"
                    },
                    startLine: 0,
                    endLine: 4),
                new ConflictSegment(
                    headLabel: "HEAD",
                    headLines: new[] { "        return   \"Hi from HEAD\";" },
                    baseLabel: null,
                    baseLines: Array.Empty<string>(),
                    incomingLabel: "feature/greeting",
                    incomingLines: new[] { "        return   \"Hi from Incoming\";" },
                    startLine: 5,
                    endLine: 9),
                new UnchangedSegment(
                    new[] { "    }", "}" },
                    startLine: 10,
                    endLine: 11)
            };

            ReconstructedSources result = reconstructor.Reconstruct(segments);

            Assert.DoesNotContain("<<<<<<<", result.HeadVersion);
            Assert.DoesNotContain("=======", result.HeadVersion);
            Assert.DoesNotContain(">>>>>>>", result.HeadVersion);
            Assert.DoesNotContain("<<<<<<<", result.IncomingVersion);
            Assert.DoesNotContain("=======", result.IncomingVersion);
            Assert.DoesNotContain(">>>>>>>", result.IncomingVersion);

            Assert.Contains("    // Leading comment trivia", result.HeadVersion);
            Assert.Contains("        return   \"Hi from HEAD\";", result.HeadVersion);
            Assert.DoesNotContain("Hi from Incoming", result.HeadVersion);

            Assert.Contains("    // Leading comment trivia", result.IncomingVersion);
            Assert.Contains("        return   \"Hi from Incoming\";", result.IncomingVersion);
            Assert.DoesNotContain("Hi from HEAD", result.IncomingVersion);
        }

        [Fact]
        public void Reconstruct_FileWithoutConflictMarkers_ReturnsIdenticalHeadAndIncomingVersions()
        {
            var cleanLines = new[]
            {
                "public class CleanClass",
                "{",
                "    public int Value => 42;",
                "}"
            };

            var segments = new List<FileSegment>
            {
                new UnchangedSegment(cleanLines, startLine: 0, endLine: 3)
            };

            ReconstructedSources result = reconstructor.Reconstruct(segments);

            Assert.Equal(result.HeadVersion, result.IncomingVersion);
            Assert.Equal(string.Join("\n", cleanLines), result.HeadVersion);
        }

        [Fact]
        public void Reconstruct_EmptyHeadSideWithDiff3Base_OmitsMarkersAndBaseLinesCleanly()
        {
            var segments = new List<FileSegment>
            {
                new UnchangedSegment(
                    new[] { "public class Builder", "{" },
                    startLine: 0,
                    endLine: 1),
                new ConflictSegment(
                    headLabel: "HEAD",
                    headLines: Array.Empty<string>(),
                    baseLabel: "base",
                    baseLines: new[] { "    public void OldMethod() { }" },
                    incomingLabel: "feature/new-method",
                    incomingLines: new[] { "    public void NewMethod() { }" },
                    startLine: 2,
                    endLine: 7),
                new UnchangedSegment(
                    new[] { "}" },
                    startLine: 8,
                    endLine: 8)
            };

            ReconstructedSources result = reconstructor.Reconstruct(segments);

            Assert.DoesNotContain("OldMethod", result.HeadVersion);
            Assert.DoesNotContain("NewMethod", result.HeadVersion);
            Assert.DoesNotContain("OldMethod", result.IncomingVersion);
            Assert.Contains("    public void NewMethod() { }", result.IncomingVersion);
        }

        [Fact]
        public void Reconstruct_EmptyIncomingSide_PreservesHeadLinesAndLeavesIncomingEmptyInBlock()
        {
            var segments = new List<FileSegment>
            {
                new UnchangedSegment(
                    new[] { "public class Settings", "{" },
                    startLine: 0,
                    endLine: 1),
                new ConflictSegment(
                    headLabel: "HEAD",
                    headLines: new[] { "    public string Mode => \"Strict\";" },
                    baseLabel: null,
                    baseLines: Array.Empty<string>(),
                    incomingLabel: "feature/relaxed",
                    incomingLines: Array.Empty<string>(),
                    startLine: 2,
                    endLine: 5),
                new UnchangedSegment(
                    new[] { "}" },
                    startLine: 6,
                    endLine: 6)
            };

            ReconstructedSources result = reconstructor.Reconstruct(segments);

            Assert.Contains("    public string Mode => \"Strict\";", result.HeadVersion);
            Assert.DoesNotContain("Mode", result.IncomingVersion);
            Assert.DoesNotContain("=======", result.IncomingVersion);
        }
    }
}