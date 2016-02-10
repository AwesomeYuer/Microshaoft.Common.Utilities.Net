namespace Microshaoft.MEF.CompositionContainers
{
    using Microsoft.Boc;
    using Microsoft.Boc.MEF.Contracts;
    using Microsoft.Boc.Share;
    using System;
    using System.ComponentModel.Composition;

    public class MessagesCompositionContainer
                    : IMefPartsCompositionContainer
                            <
                                IMessage
                                , string
                                , string
                                , string
                            >
    {
        [ImportMany(typeof(IMessage))]
        public IMessage[] Parts
        {
            get;
            private set;
        }
        public void ImportManyExports(string path)
        {
            MEFHelper
                .ImportManyExportsComposeParts<MessagesCompositionContainer>
                    (
                        path
                        , this
                    );
            JsonsMessagesProcessorsCacheManager
                .LoadFromImportManyExportsMefParts(Parts);
        }

        public void ChainedInvokeAllPartsProcessAction(params string[] parameters)
        {
            throw new NotImplementedException();
        }

        public string ChainedInvokeAllPartsProcessFunc(params string[] parameters)
        {
            throw new NotImplementedException();
        }

        public string InvokeOnePartProcessFunc(string partKey, params string[] parameters)
        {
            throw new NotImplementedException();
        }

        public void InvokeOnePartProcessAction(string partKey, params string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
