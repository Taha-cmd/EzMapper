using log4net;
using System.Runtime.CompilerServices;

namespace EzMapper
{
    internal class Default
    {
        public const string DbName = "db.sqlite";
        public const string IdProprtyName = "ID";
        public const string OwnerIdPropertyName = "OwnerId";

        public static ILog GetLogger([CallerFilePath] string filename = "") => LogManager.GetLogger(filename);
    }
}
