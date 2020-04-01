using System;
using System.Collections.Generic;

namespace AcadBillOfQuantities.Infrastructure.Interfaces
{
    public interface IAcadCommand
    {
        void Execute();
    }

    public interface IAcadCommand<T> where T : class
    {
        void Execute(T obj);
    }
    public interface IAcadCommand<TResult, TInput>
    {
        TResult Execute(TInput arg);
    }
}
