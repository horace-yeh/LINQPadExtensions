<Query Kind="Program" />

void Main()
{
	// Write code to test your extensions here. Press F5 to compile and run.
}

public static class MyExtensions
{
	// Write custom extension methods here. They will be available to all queries.


	//void Main()
	//{
	//	// 資料表名稱
	//	var className = "DatabaseLog";
	//	var tableName = "DatabaseLog";
	//	// 這邊修改為您要執行的 SQL Command
	//	var sqlCommand = $@"SELECT * FROM {tableName}";
	//	// 在 DumpClass 方法裡放 SQL Command 和 Class 名稱
	//	this.Connection.DumpClass(sqlCommand.ToString(), className, tableName).Dump();
	//}


	public static string DumpClass(this IDbConnection connection, string sql, string className = "Info",string tableName = "Info")
	{
		var descDict = connection.DumpClassDesc(tableName);
		if (connection.State != ConnectionState.Open)
		{
			connection.Open();
		}

		var cmd = connection.CreateCommand();
		cmd.CommandText = sql;
		var reader = cmd.ExecuteReader();

		var builder = new StringBuilder();
		do
		{
			if (reader.FieldCount <= 1) continue;

			if (descDict.TryGetValue(tableName, out _))
			{
				builder.AppendLine("/// <summary>");
				builder.AppendLine($"/// {descDict[tableName]}");
				builder.AppendLine("/// </summary>");
			}

			builder.AppendFormat("public class {0}{1}", className, Environment.NewLine);
			builder.AppendLine("{");
			var schema = reader.GetSchemaTable();

			foreach (DataRow row in schema.Rows)
			{
				var type = (Type)row["DataType"];
				var name = TypeAliases.ContainsKey(type) ? TypeAliases[type] : type.Name;
				var isNullable = (bool)row["AllowDBNull"] && NullableTypes.Contains(type);
				var collumnName = (string)row["ColumnName"];

				if (descDict.TryGetValue(collumnName, out _))
				{
					builder.AppendLine("\t/// <summary>");
					builder.AppendLine($"\t/// {descDict[collumnName]}");
					builder.AppendLine("\t/// </summary>");
				}
				builder.AppendLine(string.Format("\tpublic {0}{1} {2} {{ get; set; }}", name, isNullable ? "?" : string.Empty, collumnName));
				//builder.AppendLine();
			}

			builder.AppendLine("}");
			builder.AppendLine();
		} while (reader.NextResult());
		//關閉連線
		connection.Close();
		return builder.ToString();
	}

	private static Dictionary<string, string> DumpClassDesc(this IDbConnection connection, string tableName = "Info")
	{
		if (connection.State != ConnectionState.Open)
		{
			connection.Open();
		}
		var sign = ".";
		var defaultDbo = "dbo";
		tableName = tableName.Replace("[","").Replace("]","");
		if (tableName.Contains(sign)) 
		{
			var temp = tableName.Split(sign);
			defaultDbo = temp.First();
            tableName = temp.Last();
		}

		var sql = $@"SELECT objname AS [ColunmName], value AS [Desc]
                     FROM fn_listextendedproperty (NULL, 'schema', '{defaultDbo}', 'table', N'{tableName}', NULL, default)
                     WHERE name = 'MS_Description'
                     AND objtype = 'TABLE'
                     UNION
                     SELECT objname AS [ColunmName], value AS [Desc]
                     FROM fn_listextendedproperty (NULL, 'schema', '{defaultDbo}', 'table', N'{tableName}', 'column', default)
                     WHERE name = 'MS_Description' AND objtype = 'COLUMN'";
		var dict = new Dictionary<string, string>();
		var cmd = connection.CreateCommand();
		cmd.CommandText = sql;
		try
		{
			var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				var val = reader[1].ToString().Replace("\r\n", " ");
				dict.Add($"{reader[0]}", $"{val}");
			}
			reader.Close();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		connection.Close();
		return dict;
	}

	private static readonly Dictionary<Type, string> TypeAliases = new Dictionary<Type, string> {
		{ typeof(int), "int" },
		{ typeof(short), "short" },
		{ typeof(byte), "byte" },
		{ typeof(byte[]), "byte[]" },
		{ typeof(long), "long" },
		{ typeof(double), "double" },
		{ typeof(decimal), "decimal" },
		{ typeof(float), "float" },
		{ typeof(bool), "bool" },
		{ typeof(string), "string" }
	};

	private static readonly HashSet<Type> NullableTypes = new HashSet<Type> {
		typeof(int),
		typeof(short),
		typeof(long),
		typeof(double),
		typeof(decimal),
		typeof(float),
		typeof(bool),
		typeof(DateTime)
	};

}

// You can also define namespaces, non-static classes, enums, etc.

#region Advanced - How to multi-target

// The NETx symbol is active when a query runs under .NET x or later.
// (LINQPad also recognizes NETx_0_OR_GREATER in case you enjoy typing.)

#if NET8
// Code that requires .NET 8 or later
#endif

#if NET7
// Code that requires .NET 7 or later
#endif

#if NET6
// Code that requires .NET 6 or later
#endif

#if NETCORE
// Code that requires .NET Core or later
#else
// Code that runs under .NET Framework (LINQPad 5)
#endif

#endregion