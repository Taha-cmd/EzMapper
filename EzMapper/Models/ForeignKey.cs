using EzMapper.Attributes;

namespace EzMapper.Models
{
    class ForeignKey
    {
        public ForeignKey() { }

        public ForeignKey(string fieldName, string targetTable, string targetField, DeleteAction action)
        {
            FieldName = fieldName;
            TargetTable = targetTable;
            TargetField = targetField;
            Action = action;
        }
        public string FieldName { get; set; }
        public string TargetTable { get; set; }
        public string TargetField { get; set; }
        public DeleteAction Action { get; set; }

    }
}
