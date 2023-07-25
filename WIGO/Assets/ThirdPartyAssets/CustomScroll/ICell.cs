using System;
using System.Threading.Tasks;

/// <summary>
/// Interface for creating a Cell.
/// Prototype Cell must have a monobeviour inheriting from ICell
/// </summary>
namespace WIGO.RecyclableScroll
{
    public interface ICell<TData> where TData : ContactInfo
    {
        void InitCell(Action<ICell<TData>> onClick);
        Task ConfigureCell(TData contactInfo, int cellIndex);
        int GetIndex();
        TData GetInfo();
    }
}
