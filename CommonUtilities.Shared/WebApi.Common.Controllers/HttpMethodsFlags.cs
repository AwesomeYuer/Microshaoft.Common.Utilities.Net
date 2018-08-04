namespace Microshaoft.Web
{
    using System;
    [Flags]
    public enum HttpMethodsFlags : byte
    {
        None                = 0b0000_0000
        , Get               = 0b0000_0001    //[HttpGet]
        , Head              = 0b0000_0010    //[HttpHead]
        , Options           = 0b0000_0100    //[HttpOptions]
        , Trace             = 0b0000_1000
        , ReadOperations    = 0b0000_1111

        , Delete            = 0b0001_0000    //[HttpDelete]   
        , Patch             = 0b0010_0000    //[HttpPatch]
        , Post              = 0b0100_0000    //[HttpPost]
        , Put               = 0b1000_0000    //[HttpPut]
        , WriteOperations   = 0b1111_0000

        , All               = 0b1111_1111
    }
}
