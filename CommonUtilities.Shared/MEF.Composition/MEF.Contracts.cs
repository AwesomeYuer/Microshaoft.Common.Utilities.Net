namespace Microshaoft.MEF.Contracts
{
    using System;
    public delegate void ExceptionEventHandler<TSender>(TSender sender, Exception exception);
}
namespace Microshaoft.MEF.Contracts
{
    using System;
    public interface IMefChainedProcessorPart<TContainer, TPartKey, TResult, TParams>
    {
        IMefChainedProcessorPart<TContainer, TPartKey, TResult, TParams> Instance
        {
            get;
        }
        int Priority
        {
            get;
        }
        TPartKey Key
        {
            get;
        }
        void OnOnceProcessAction(params TParams[] parameters);
        TResult OnOnceProcessFunc(params TParams[] parameters);
        void OnChainedOnceProcessAction(out ChainedProcessNextStep next, params TParams[] parameters);
        TResult OnChainedOnceProcessFunc(out ChainedProcessNextStep next, params TParams[] parameters);
        void OnChainedOnceAsyncQueueProcessAction(out ChainedProcessNextStep next, params TParams[] parameters);
        bool OnChainedOnceAsyncQueueProcessFunc(out ChainedProcessNextStep next, params TParams[] parameters);
        event ExceptionEventHandler<IMefChainedProcessorPart<TContainer, TPartKey, TResult, TParams>> OnCaughtExceptionInContainer;
        string GetRuntimeTypeFullName();
        Type GetRuntimeType();
    }
    public enum ChainedProcessNextStep
    {
        Continue
        , Break
    }
}
namespace Microshaoft.MEF.Contracts
{
    //using System;
    //using System.Xml;
    public interface IMefPartsCompositionContainer<TPart, TPartKey, TResult, TInvokeParams>
    {
        TPart[] Parts
        {
            get;
        }
        void ImportManyExports(string path);
        void ChainedInvokeAllPartsProcessAction(params TInvokeParams[] parameters);
        TResult ChainedInvokeAllPartsProcessFunc(params TInvokeParams[] parameters);
        TResult InvokeOnePartProcessFunc(TPartKey partKey, params TInvokeParams[] parameters);
        void InvokeOnePartProcessAction(TPartKey partKey, params TInvokeParams[] parameters);
    }
}