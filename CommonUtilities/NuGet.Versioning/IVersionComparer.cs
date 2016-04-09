using System.Collections.Generic;

namespace Microshaoft.Versioning
{
    /// <summary>
    /// IVersionComparer represents a version comparer capable of sorting and determining the equality of SemanticVersion objects.
    /// </summary>
    public interface IVersionComparer : IEqualityComparer<SemanticVersion>, IComparer<SemanticVersion>
    {

    }
}
