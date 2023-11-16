using System;
using System.Threading.Tasks;
using Contracts;

namespace SDK
{
    public interface IImageInterpreter
    {
        Task<ImageQueryResult> InterpretImage(ImageQuery query);
    }
}