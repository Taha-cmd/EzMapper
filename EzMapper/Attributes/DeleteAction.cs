using System;

namespace EzMapper.Attributes
{
    public enum DeleteAction
    {
        SetNull,
        Cascade,
        SetDefault,
        NoAction
    }

    internal static class EnumExtensions
    {
        public static string Value(this DeleteAction deleteAction)
        {
            return deleteAction switch
            {
                DeleteAction.Cascade => "CASCADE",
                DeleteAction.SetDefault => "SET DEFAULT",
                DeleteAction.SetNull => "SET NULL",
                DeleteAction.NoAction => "NO ACTION",
                _ => throw new NotImplementedException()
            };
        }
    }
}
