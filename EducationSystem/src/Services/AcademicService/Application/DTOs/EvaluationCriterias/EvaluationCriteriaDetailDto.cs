namespace AcademicService.Application.DTOs.EvaluationCriterias;

public sealed class EvaluationCriteriaDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Type { get; init; }
    public decimal Score { get; init; }
    public Guid? ParentId { get; init; }
    public Guid? QuestionId { get; init; }
}
