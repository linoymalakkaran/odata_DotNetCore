namespace MGSurvey.Domain.Entities
{
    public class ValidationSchema: BaseEntity<string>
    {
        public string FormId { get; set; }
        public virtual Form Form { get; set; }

    }
}
