namespace MGSurvey.Infrastructure.Database
{
    public class TableOptions
    {
        public TableOptions(string name)
        {
            Name = name;
        }

        public TableOptions(string name, string schema)
        {
            Name = name;
            Schema = schema;
        }
        public string Name { get; set; }
        public string Schema { get; set; }
    }
}
