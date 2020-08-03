namespace MGSurvey.Business.Models
{
    public class ValidationSchema: BaseModel<string>
    {
        public string FormId { get; set; }
        public virtual Form Form { get; set; }
    }
}
