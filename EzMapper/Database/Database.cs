using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EzMapper.Database
{
    internal  class Database<TConnection, TCommand, TParam> : IDatebase
                        where TConnection : DbConnection
                        where TCommand    : DbCommand
                        where TParam      : DbParameter
                        
    {
        protected readonly string connectionString;

        public Database(string conn)
        {
            connectionString = conn;
        }

        protected TConnection GetConnection(string connectionString)
        {
            TConnection conn = (TConnection)Activator.CreateInstance(typeof(TConnection), connectionString);
            conn.Open();

            return conn;
        }

        protected TCommand CreateCommand(string statement, DbConnection connection, params DbParameter[] parameters)
        {
            TCommand command = (TCommand)Activator.CreateInstance(typeof(TCommand), statement, connection);
            command.Parameters.AddRange(parameters);
            command.Prepare();

            return command;
        }

        protected IEnumerable<TResult> ReadRows<TResult>(DbCommand command, Func<DbDataReader, TResult> rowReader)
        {
            using var reader = command.ExecuteReader();
            var results = new List<TResult>();
            while (reader.Read())
                results.Add(rowReader(reader));

            return results;
        }

        public DbParameter Param<TValue>(string key, TValue value)
        {
            TParam param = Activator.CreateInstance<TParam>();
            param.ParameterName = key;
            param.Value = value;

            return param;
        }

        public int ExecuteNonQuery(string statement, params DbParameter[] parameters)
        {
            using var conn = GetConnection(connectionString);
            using var command = CreateCommand(statement, conn, parameters);

            return command.ExecuteNonQuery();
        }

        public IEnumerable<TResult> ExecuteQuery<TResult>(string statement, Func<DbDataReader, TResult> rowReader, params DbParameter[] parameters)
        {
            using var conn = GetConnection(connectionString);
            using var command = CreateCommand(statement, conn, parameters);

            return ReadRows(command, rowReader);
        }
    }
}
