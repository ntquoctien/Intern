using System.Text.Json;
using CareerService.Application;

namespace CareerService.Tests;

public sealed class OutcomeExtractionContextBuilderTests
{
    [Fact]
    public void SubjectScopedProgramResult_UsesSelectedCurriculumAndContainsNoPlos()
    {
        var selection = new OutcomeExtractionSelection(
            "ATBM", "An toàn thông tin", "ATBM-2025",
            "Chương trình ATBM", "2025", Guid.NewGuid(),
            "502CNLT25", "An toàn và bảo mật thông tin");

        var result = OutcomeImportProcessor.BuildSubjectScopedProgramResult(selection);
        using var document = System.Text.Json.JsonDocument.Parse(result.Json);

        Assert.Equal(0, document.RootElement.GetProperty("plos").GetArrayLength());
        Assert.Equal(
            "ATBM-2025",
            document.RootElement.GetProperty("curriculum")
                .GetProperty("curriculumCode").GetString());
        Assert.Null(result.ProviderRequestId);
    }

    [Fact]
    public void CourseContext_IsBoundedAndRetainsLateExplicitOutcomeEvidence()
    {
        var blocks = Enumerable.Range(0, 500)
            .Select(index => new DocumentBlockData(
                $"paragraph-{index}",
                "Paragraph",
                index,
                null,
                JsonSerializer.Serialize(new
                {
                    text = index == 420
                        ? "CHUẨN ĐẦU RA MÔN HỌC CLO1: Thiết lập tường lửa"
                        : $"Nội dung tham khảo {index} {new string('x', 180)}",
                    paragraphIndex = index,
                    headingPath = Array.Empty<string>()
                })))
            .ToList();
        var selection = new OutcomeExtractionSelection(
            "CNTT", "Công Nghệ Thông Tin", "CNTT", null, "2026",
            Guid.NewGuid(), "502CNLT25", "An toàn và bảo mật thông tin");

        var input = OutcomeExtractionContextBuilder.BuildDocumentContext(
            2, selection, blocks, 12_000);

        Assert.True(input.Length <= 12_000);
        Assert.Contains("paragraph-420", input);
        Assert.DoesNotContain("paragraph-300", input);
        Assert.Contains("502CNLT25", input);
    }

    [Fact]
    public void CourseContext_PrioritizesCompleteFormalObjectiveSection()
    {
        var blocks = new List<DocumentBlockData>
        {
            Block(0, "Tên môn học"),
            Block(1, "II. MỤC TIÊU MÔN HỌC")
        };
        blocks.AddRange(Enumerable.Range(1, 14)
            .Select(index => Block(index + 1, $"+ CLO objective {index}")));
        blocks.Add(Block(16, "III. NỘI DUNG MÔN HỌC"));
        blocks.Add(Block(17, "Mục tiêu bài học không phải CLO"));

        var input = OutcomeExtractionContextBuilder.BuildDocumentContext(
            2,
            new OutcomeExtractionSelection(
                "ATBM", "ATBM", "ATBM-2025", null, "2025",
                Guid.NewGuid(), "502CNLT25", "An toàn và bảo mật"),
            blocks,
            12_000);

        Assert.Contains("CLO objective 1", input);
        Assert.Contains("CLO objective 14", input);
        Assert.DoesNotContain("không phải CLO", input);
        Assert.DoesNotContain("headingPath", input);
        Assert.Equal(
            14,
            OutcomeExtractionContextBuilder.CountFormalCourseObjectives(blocks));
    }

    [Fact]
    public void SelectedMetadataEvidence_AddsReferencesWithoutReplacingProviderEvidence()
    {
        var blocks = new[]
        {
            Block(0, "CHƯƠNG TRÌNH MÔN HỌC"),
            Block(1, "Tên môn học: An toàn và bảo mật thông tin")
        };
        var document = new OutcomeDraftDocument
        {
            Subjects =
            [
                new SubjectDraft
                {
                    Subject = new SubjectReference
                    {
                        Code = "502CNLT25",
                        Name = "An toàn và bảo mật thông tin"
                    }
                }
            ]
        };
        var selection = new OutcomeExtractionSelection(
            "ATBM", "ATBM", "ATBM-2025", null, "2025",
            Guid.NewGuid(), "502CNLT25", "An toàn và bảo mật thông tin");

        OutcomeImportProcessor.ApplySelectedMetadataEvidence(document, blocks, selection);

        Assert.Equal("paragraph-0", document.Curriculum.SourceReferences.Single().BlockId);
        Assert.Equal("paragraph-1", document.Subjects.Single()
            .SourceReferences.Single().BlockId);
    }

    [Fact]
    public void FormalObjectiveReconciliation_RebuildsIncompleteProviderList()
    {
        var blocks = new List<DocumentBlockData>
        {
            Block(0, "II. MỤC TIÊU MÔN HỌC")
        };
        blocks.AddRange(Enumerable.Range(1, 14)
            .Select(index => Block(index, $"+ Mục tiêu chính thức {index}")));
        blocks.Add(Block(15, "III. NỘI DUNG MÔN HỌC"));
        var document = new OutcomeDraftDocument
        {
            Subjects =
            [
                new SubjectDraft
                {
                    DraftId = "subject-001",
                    Subject = new SubjectReference { Code = "502CNLT25" },
                    Clos =
                    [
                        new CloDraft { DraftId = "old", CloCode = "CLO1" }
                    ]
                }
            ]
        };
        var selection = new OutcomeExtractionSelection(
            "ATBM", "ATBM", "ATBM-2025", null, "2025",
            Guid.NewGuid(), "502CNLT25", "An toàn và bảo mật thông tin");

        var changed = OutcomeImportProcessor.ReconcileFormalCourseObjectives(
            document, blocks, selection);

        Assert.True(changed);
        Assert.Equal(14, document.Subjects.Single().Clos.Count);
        Assert.Equal("Mục tiêu chính thức 14",
            document.Subjects.Single().Clos.Last().Description);
        Assert.Equal("paragraph-14",
            document.Subjects.Single().Clos.Last().SourceReferences.Single().BlockId);
        Assert.Contains(document.Warnings,
            item => item.Code == "CLO_LIST_RECONCILED_FROM_FORMAL_OBJECTIVES");
    }

    [Fact]
    public void MatrixContext_DoesNotRepeatUnrelatedDocumentBlocks()
    {
        var blocks = Enumerable.Range(1, 10)
            .Select(sequence => Block(
                sequence,
                sequence == 8
                    ? "MA TRẬN CLO-PLO: CLO1 PLO2 R"
                    : $"Nội dung chương {sequence}"))
            .ToArray();
        var selection = new OutcomeExtractionSelection(
            "CNTT", "Công Nghệ Thông Tin", "CNTT", null, "2026",
            null, null, null);

        var input = OutcomeExtractionContextBuilder.BuildMatrixContext(
            selection,
            blocks,
            """{"curriculum":{},"plos":[],"warnings":[]}""",
            """{"subjects":[],"warnings":[]}""",
            8_000);

        Assert.DoesNotContain("\"blockId\":\"paragraph-1\",", input);
        Assert.Contains("paragraph-8", input);
        Assert.Contains("paragraph-9", input);
    }

    [Fact]
    public void MatrixEvidence_IsFalseForOrdinaryCourseOutline()
    {
        var blocks = new[]
        {
            Block(1, "MỤC TIÊU MÔN HỌC"),
            Block(2, "Về kiến thức: mô tả cách thức mã hóa thông tin"),
            Block(3, "Về kỹ năng: thiết lập tường lửa bảo vệ mạng")
        };

        Assert.False(OutcomeExtractionContextBuilder.HasExplicitMatrixEvidence(blocks));
    }

    private static DocumentBlockData Block(int sequence, string text) =>
        new(
            $"paragraph-{sequence}",
            "Paragraph",
            sequence,
            null,
            JsonSerializer.Serialize(new
            {
                text,
                paragraphIndex = sequence,
                headingPath = Array.Empty<string>()
            }));
}
