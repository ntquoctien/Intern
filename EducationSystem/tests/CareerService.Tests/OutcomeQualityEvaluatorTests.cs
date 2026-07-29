using CareerService.Application;

namespace CareerService.Tests;

public sealed class OutcomeQualityEvaluatorTests
{
    [Fact]
    public void Evaluate_CalculatesEvidenceScopeCountsAndOverlap()
    {
        var reference = new SourceReference(
            "paragraph-1", "Paragraph", 1, [], 1, null, null, null,
            "CLO1 Build a secure REST API");
        var document = new OutcomeDraftDocument
        {
            Plos = [],
            Subjects =
            [
                new SubjectDraft
                {
                    DraftId = "subject-1",
                    Subject = new SubjectReference { Code = "CS101", Name = "Programming" },
                    SourceReferences = [reference],
                    Clos =
                    [
                        new CloDraft
                        {
                            DraftId = "clo-1",
                            CloCode = "CLO1",
                            Description = "Build a secure REST API",
                            SourceReferences = [reference]
                        }
                    ]
                }
            ],
            Warnings =
            [
                new OutcomeWarning(
                    "OUT_OF_SCOPE_SUBJECT_REMOVED", "Warning", "subject-extra",
                    "subject.code", "Removed")
            ]
        };
        var validation = new[]
        {
            new OutcomeWarning(
                "CLO_WITHOUT_PLO", "Warning", "clo-1", null, "No mapping")
        };
        var blocks = new[]
        {
            new DocumentBlockData(
                "paragraph-1", "Paragraph", 1, null,
                """{"text":"CLO1 Build a secure REST API"}""")
        };

        var report = new OutcomeQualityEvaluator().Evaluate(
            document, blocks, validation, "CS101");

        Assert.Equal(1, report.ScopeCompliance);
        Assert.Equal(1, report.SourceReferenceCoverage);
        Assert.Equal(1, report.CloCountSource);
        Assert.Equal(1, report.CloCountReviewed);
        Assert.Equal(1, report.ExactCloCodeMatchRate);
        Assert.Equal(1, report.ExactPloCodeMatchRate);
        Assert.Equal(1, report.TextOverlapAverage);
        Assert.Equal(1, report.HallucinatedSubjectCount);
        Assert.Equal(1, report.RemovedOutOfScopeSubjectCount);
        Assert.Empty(report.BlockingErrors);
        Assert.Contains("CLO_WITHOUT_PLO", report.Warnings);
    }

    [Fact]
    public void Evaluate_DoesNotHideBlockingValidation()
    {
        var report = new OutcomeQualityEvaluator().Evaluate(
            new OutcomeDraftDocument(),
            [],
            [new OutcomeWarning(
                "SOURCE_REFERENCE_REQUIRED", "BlockingError", "clo-1",
                "sourceReferences", "Missing")],
            "CS101");

        Assert.Contains("SOURCE_REFERENCE_REQUIRED", report.BlockingErrors);
        Assert.Equal(0, report.ScopeCompliance);
    }
}
