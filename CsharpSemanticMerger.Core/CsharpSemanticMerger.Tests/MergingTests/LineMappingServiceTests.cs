using CsharpSemanticMerger.Core.Mapping;
using CsharpSemanticMerger.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CsharpSemanticMerger.Tests.MergingTests
{
    public sealed class LineMappingServiceTests
    {
        private readonly LineMappingService mapper = new();
        [Fact]
        public void BuildHeadAndIncoming_WithDiff3Conflict_MapsVirtualLinesToExactOriginalLines()
        {
            // Original line numbers:
            // 0: namespace Demo;
            // 1: class Sample {
            // 2: <<<<<<< HEAD
            // 3:     int Value() => 1;
            // 4: ||||||| base
            // 5:     int Value() => 0;
            // 6: =======
            // 7:     int Value() => 2;
            // 8: >>>>>>> incoming
            // 9: }
            var segments = new List<FileSegment>
            {
                new UnchangedSegment(
                    new[] { "namespace Demo;", "class Sample {" },
                    startLine: 0,
                    endLine: 1),
                new ConflictSegment(
                    headLabel: "HEAD",
                    headLines: new[] { "    int Value() => 1;" },
                    baseLabel: "base",
                    baseLines: new[] { "    int Value() => 0;" },
                    incomingLabel: "incoming",
                    incomingLines: new[] { "    int Value() => 2;" },
                    startLine: 2,
                    endLine: 8),
                new UnchangedSegment(
                    new[] { "}" },
                    startLine: 9,
                    endLine: 9)
            };

            ReconstructedSide head = mapper.BuildHead(segments);
            ReconstructedSide incoming = mapper.BuildIncoming(segments);

            Assert.Equal(0, head.LineMapping.ToOriginalLine(0));
            Assert.Equal(1, head.LineMapping.ToOriginalLine(1));
            Assert.Equal(3, head.LineMapping.ToOriginalLine(2));
            Assert.Equal(9, head.LineMapping.ToOriginalLine(3));

            Assert.Equal(0, incoming.LineMapping.ToOriginalLine(0));
            Assert.Equal(1, incoming.LineMapping.ToOriginalLine(1));
            Assert.Equal(7, incoming.LineMapping.ToOriginalLine(2));
            Assert.Equal(9, incoming.LineMapping.ToOriginalLine(3));
        }

        [Fact]
        public void BuildHeadAndIncoming_MultipleConflictsWithVaryingSizes_MapsVirtualLinesAcrossAllBlocks()
        {
            var segments = new List<FileSegment>
            {
                new UnchangedSegment(
                    new[] { "class Multi {" },
                    startLine: 0,
                    endLine: 0),
                new ConflictSegment(
                    headLabel: "HEAD",
                    headLines: new[] { "    int a = 1;", "    int b = 2;" },
                    baseLabel: "base",
                    baseLines: new[] { "    int a = 0;" },
                    incomingLabel: "incoming",
                    incomingLines: Array.Empty<string>(),
                    startLine: 1,
                    endLine: 7),
                new UnchangedSegment(
                    new[] { "    int middle = 10;" },
                    startLine: 8,
                    endLine: 8),
                new ConflictSegment(
                    headLabel: "HEAD",
                    headLines: new[] { "    int c = 3;" },
                    baseLabel: null,
                    baseLines: Array.Empty<string>(),
                    incomingLabel: "incoming",
                    incomingLines: new[] { "    int c = 4;", "    int d = 5;" },
                    startLine: 9,
                    endLine: 14),
                new UnchangedSegment(
                    new[] { "}" },
                    startLine: 15,
                    endLine: 15)
            };

            ReconstructedSide head = mapper.BuildHead(segments);
            ReconstructedSide incoming = mapper.BuildIncoming(segments);

            Assert.Equal(0, head.LineMapping.ToOriginalLine(0));
            Assert.Equal(2, head.LineMapping.ToOriginalLine(1));
            Assert.Equal(3, head.LineMapping.ToOriginalLine(2));
            Assert.Equal(8, head.LineMapping.ToOriginalLine(3));
            Assert.Equal(10, head.LineMapping.ToOriginalLine(4));
            Assert.Equal(15, head.LineMapping.ToOriginalLine(5));

            Assert.Equal(0, incoming.LineMapping.ToOriginalLine(0));
            Assert.Equal(8, incoming.LineMapping.ToOriginalLine(1));
            Assert.Equal(12, incoming.LineMapping.ToOriginalLine(2));
            Assert.Equal(13, incoming.LineMapping.ToOriginalLine(3));
            Assert.Equal(15, incoming.LineMapping.ToOriginalLine(4));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void ToOriginalLine_OutOfRangeVirtualLine_ThrowsArgumentOutOfRangeException(int invalidVirtualLine)
        {
            var segments = new List<FileSegment>
            {
                new UnchangedSegment(new[] { "class Sample {" }, 0, 0),
                new ConflictSegment("HEAD", new[] { "    int x = 1;" }, null, Array.Empty<string>(), "incoming", new[] { "    int x = 2;" }, 1, 5),
                new UnchangedSegment(new[] { "}" }, 6, 6)
            };

            ReconstructedSide head = mapper.BuildHead(segments);

            Assert.Throws<ArgumentOutOfRangeException>(() => head.LineMapping.ToOriginalLine(invalidVirtualLine));
        }
    }
}
