using System;
using System.Threading.Tasks;
using Contracts.Receipts;

namespace SDK
{
    public interface IReceiptInterpreter
    {
        Task<ReceiptQueryResult> Interpret(ReceiptQuery query);
    }
}