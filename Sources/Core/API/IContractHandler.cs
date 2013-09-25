using System;

namespace MessageBus.Core.API
{
    public interface IContractHandler
    {
        Type ContractType { get; }

        IProcessor CreateProcessor(object data);
    }
}