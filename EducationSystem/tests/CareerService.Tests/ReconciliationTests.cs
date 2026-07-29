using System.Text.Json;
using CareerService.Application;

namespace CareerService.Tests;

public sealed class ReconciliationTests
{
    [Fact]
    public void Reconcile_WhenSubjectIsSelected_RemovesProviderSubjectsOutsideThatScope()
    {
        const string program =
            """{"curriculum":{"draftId":"curriculum-1"},"plos":[],"warnings":[]}""";
        const string outcomes =
            """
            {
              "subjects": [
                {
                  "draftId": "subject-extra",
                  "subject": { "code": "MH 25", "name": "Document label" },
                  "clos": []
                },
                {
                  "draftId": "subject-selected",
                  "subject": { "code": "502CNLT25", "name": "An toàn và bảo mật thông tin" },
                  "clos": [
                    {
                      "draftId": "clo-selected-1",
                      "cloCode": "CLO1",
                      "description": "Mô tả CLO",
                      "sourceReferences": []
                    }
                  ]
                }
              ],
              "warnings": []
            }
            """;
        const string mappings =
            """
            {
              "mappings": [
                {
                  "draftId": "mapping-selected",
                  "cloDraftId": "clo-selected-1",
                  "ploDraftId": "plo-1",
                  "progressionLevel": "E"
                },
                {
                  "draftId": "mapping-extra",
                  "cloDraftId": "clo-extra-1",
                  "ploDraftId": "plo-1",
                  "progressionLevel": "E"
                }
              ],
              "warnings": []
            }
            """;

        var result = new OutcomeReconciliationService().Reconcile(
            42, program, outcomes, mappings, "502CNLT25");

        var subject = Assert.Single(result.Subjects);
        Assert.Equal("502CNLT25", subject.Subject.Code);
        Assert.Equal("clo-selected-1", Assert.Single(subject.Clos).DraftId);
        Assert.Equal("mapping-selected", Assert.Single(result.Mappings).DraftId);
    }

    [Fact]
    public void Reconcile_AssignsIds_NormalizesValues_AndCollapsesExactDuplicates()
    {
        var program = """
          {"curriculum":{"draftId":"","sourceReferences":[],"confidence":0.9},
           "plos":[
             {"draftId":"","ploCode":" plo1 ","description":"Use knowledge","sourceReferences":[],"confidence":0.9},
             {"draftId":"","ploCode":"PLO1","description":"Use  knowledge","sourceReferences":[],"confidence":0.8}
           ],"warnings":[]}
          """;
        var outcomes = """
          {"subjects":[{"draftId":"","subject":{"code":" cs101 ","name":" Programming "},"sourceReferences":[],"confidence":0.9,
            "clos":[{"draftId":"","cloCode":" clo1 ","description":"Build software","sourceReferences":[],"confidence":0.9}]}],"warnings":[]}
          """;
        var mappings = """
          {"mappings":[{"draftId":"","cloDraftId":"clo-001-001","ploDraftId":"plo-001","progressionLevel":" r ","sourceReferences":[],"confidence":0.9}],"warnings":[]}
          """;

        var result = new OutcomeReconciliationService().Reconcile(42, program, outcomes, mappings);

        Assert.Equal("PLO1", result.Plos[0].PloCode);
        Assert.False(result.Plos[0].Removed);
        Assert.True(result.Plos[1].Removed);
        Assert.Equal("CS101", result.Subjects[0].Subject.Code);
        Assert.Equal("CLO1", result.Subjects[0].Clos[0].CloCode);
        Assert.Equal("R", result.Mappings[0].ProgressionLevel);
        Assert.Equal(42, result.ImportBatchId);
    }

    [Fact]
    public void Reconcile_ReportsConflictingMappingLevelsAsBlocking()
    {
        var mappings = JsonSerializer.Serialize(new
        {
            mappings = new[]
            {
                Mapping("m1", "E"),
                Mapping("m2", "D"),
            },
            warnings = Array.Empty<object>(),
        });
        var result = new OutcomeReconciliationService().Reconcile(
            1, """{"curriculum":{},"plos":[],"warnings":[]}""",
            """{"subjects":[],"warnings":[]}""", mappings);

        Assert.Contains(result.Warnings,
            warning => warning.Code == "MAPPING_LEVEL_CONFLICT" && warning.Severity == "BlockingError");
    }

    private static object Mapping(string id, string level) => new
    {
        draftId = id,
        cloDraftId = "clo-1",
        ploDraftId = "plo-1",
        progressionLevel = level,
        sourceReferences = Array.Empty<object>(),
        confidence = .9,
    };
}
