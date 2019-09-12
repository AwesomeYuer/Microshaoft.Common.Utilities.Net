#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;

    public static class MEFHelper
    {
        public static void ImportManyExportsComposeParts<T>(string path,  T attributedPart)
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path));
            var container = new CompositionContainer(catalog);
            container.ComposeParts(attributedPart);
        }
    }
}

#endif
