namespace Microshaoft.Web
{
    using System;
    [Flags]
    public enum HttpMethodsFlags : byte
    {
        HttpNone                = 0b0000_0000
        , HttpGet               = 0b0000_0001    //[HttpGet]
        , HttpHead              = 0b0000_0010    //[HttpHead]
        , HttpOptions           = 0b0000_0100    //[HttpOptions]
        , HttpTrace             = 0b0000_1000
        , HttpReadOperationsMethods    = 0b0000_1111
          
        , HttpDelete            = 0b0001_0000    //[HttpDelete]   
        , HttpPatch             = 0b0010_0000    //[HttpPatch]
        , HttpPost              = 0b0100_0000    //[HttpPost]
        , HttpPut               = 0b1000_0000    //[HttpPut]
        , HttpWriteOperationsMethods   = 0b1111_0000

        , HttpAllMethods               = 0b1111_1111
    }
}
