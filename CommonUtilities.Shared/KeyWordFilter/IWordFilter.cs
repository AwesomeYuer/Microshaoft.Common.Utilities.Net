namespace Microshaoft
{

    using System.Collections.Generic;
    public interface IWordFilter
    {
        void AddKey(string key);
        bool HasBadWord(string text);
        string FindOne(string text);
        IEnumerable<string> FindAll(string text);
        string Replace(string text, char mask = '*');
    }
}