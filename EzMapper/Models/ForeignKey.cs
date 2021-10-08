namespace EzMapper.Models
{
    class ForeignKey
    {
        public ForeignKey() { }

        public ForeignKey(string fieldName, string targetTable, string targetField)
        {
            FieldName = fieldName;
            TargetTable = targetTable;
            TargetField = targetField;
        }
        public string FieldName { get; set; }
        public string TargetTable { get; set; }
        public string TargetField { get; set; }
    }
}
