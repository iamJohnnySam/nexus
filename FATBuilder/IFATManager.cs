using DatabaseManagers;

namespace FATBuilder
{
    public interface IFATManager
    {
        SQLiteManager db { get; }
    }
}