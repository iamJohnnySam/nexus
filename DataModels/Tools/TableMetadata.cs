namespace DataModels.Tools;

public class TableMetadata
{
    public string TableName { get; }
    public string SortColumn { get; }
    public bool SortDescending { get; }
    public Dictionary<string, EDataType> Columns { get; }

    public TableMetadata(string tableName, Dictionary<string, EDataType> columns, string defaultSortColumn, bool sortDescending = false)
    {
        TableName = tableName;
        SortColumn = defaultSortColumn;
        SortDescending = sortDescending;
        Columns = columns;

        if (columns.Values.Count(v => v == EDataType.Key) != 1)
            throw new ArgumentException("There must be exactly one primary key column.");
    }
}
