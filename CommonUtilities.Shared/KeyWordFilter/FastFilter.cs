
namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    /// <summary>
    /// 使用Hash优化算法
    /// </summary>
    public class FastFilter : IWordFilter
    {
        const int CharCount = char.MaxValue + 1;
        private int maxWordLength = 0;
        private int minWordLength = int.MaxValue;
        private HashStringSet m_hashSet = new HashStringSet();
#if !FIXED
        private ushort[] m_fastCheck = new ushort[CharCount];
        private ushort[] m_startLength = new ushort[CharCount];
        private ushort[] m_endLength = new ushort[CharCount];
#else
		//使用指针的方式,但实际测试下来没有使用数组快
		//大对象,必须大于85000字节,以保证永不被压缩
		unsafe struct FixedBuffer
		{
			public fixed ushort FastCheck[CharCount * 3];
		}
		private ushort* m_fastCheck;
		private ushort* m_startLength;
		private ushort* m_endLength;
		private FixedBuffer m_buffer = new FixedBuffer();
		unsafe public FastFilter()
		{
			fixed (ushort* ptr = m_buffer.FastCheck)
			{
				m_fastCheck = ptr;
				m_startLength = ptr + CharCount;
				m_endLength = ptr + CharCount * 2;
			}
		}
#endif
        public void AddKey(string word)
        {
            const int maxLen = sizeof(ushort) * 8;
            if (word.Length > maxLen)
            {
                throw new Exception("参数最大" + maxLen + "个字符");
            }
            maxWordLength = Math.Max(maxWordLength, word.Length);
            minWordLength = Math.Min(minWordLength, word.Length);
            //字符出现的位置(1-16),
            for (int i = 0; i < word.Length; i++)
            {
                m_fastCheck[word[i]] |= (ushort)(1 << i);
            }
            ushort mask = (ushort)(1 << (word.Length - 1));
            //以x开始的字符的长度
            m_startLength[word[0]] |= mask;
            //以y结束的字符的长度
            m_endLength[word[word.Length - 1]] |= mask;
            m_hashSet.Add(word);
        }
        public bool HasBadWord(string text)
        {
            for (int index = 0; index < text.Length; index++)
            {
                int count = 0;
                int maxIndex = Math.Min(maxWordLength + index, text.Length);
                char begin = text[index];
                for (int j = index; j < maxIndex; j++)
                {
                    char current = text[j];
                    ushort mask = (ushort)(1 << count);
                    //先判断字符出现的位置是否匹配
                    if ((m_fastCheck[current] & mask) == 0)
                    {
                        if (count > 1)
                        {
                            index += (count - 1);
                        }
                        break;
                    }
                    ++count;
                    //再判断尾字符和首字符的长度是否有匹配.
                    if ((m_endLength[current] & mask) != 0 && (m_startLength[begin] & mask) != 0)
                    {
                        //进行hash比较
                        if (m_hashSet.Contains(text, index, count))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public string FindOne(string text)
        {
            for (int index = 0; index < text.Length; index++)
            {
                int count = 0;
                int maxIndex = Math.Min(maxWordLength + index, text.Length);
                char begin = text[index];
                for (int j = index; j < maxIndex; j++)
                {
                    char current = text[j];
                    ushort mask = (ushort)(1 << count);
                    //先判断字符出现的位置是否匹配
                    if ((m_fastCheck[current] & mask) == 0)
                    {
                        if (count > 1)
                        {
                            index += (count - 1);
                        }
                        break;
                    }
                    ++count;
                    //再判断尾字符和首字符的长度是否有匹配.
                    if ((m_endLength[current] & mask) != 0 && (m_startLength[begin] & mask) != 0)
                    {
                        //进行hash比较
                        if (m_hashSet.Contains(text, index, count))
                        {
                            return text.Substring(index, count);
                        }
                    }
                }
            }
            return string.Empty;
        }
        public IEnumerable<string> FindAll(string text)
        {
            //List<string> result = new List<string>();
            for (int index = 0; index < text.Length; index++)
            {
                int count = 0;
                int maxIndex = Math.Min(maxWordLength + index, text.Length);
                char begin = text[index];
                for (int j = index; j < maxIndex; j++)
                {
                    char current = text[j];
                    ushort mask = (ushort)(1 << count);
                    if ((m_fastCheck[current] & mask) == 0)
                    {
                        if (count > 1)
                        {
                            index += (count - 1);
                        }
                        break;
                    }
                    ++count;
                    if ((m_endLength[current] & mask) != 0 && (m_startLength[begin] & mask) != 0)
                    {
                        if (m_hashSet.Contains(text, index, count))
                        {
                            //result.Add(text.Substring(index, count));
                            yield return text.Substring(index, count);
                            index += (count - 1);
                            break;
                        }
                    }
                }
            }
            //return result;
        }
        public string Replace(string text, char maskChar = '*')
        {
            char[] chars = null;
            for (int index = 0; index < text.Length; index++)
            {
                int count = 0;
                int maxIndex = Math.Min(maxWordLength + index, text.Length);
                char begin = text[index];
                for (int j = index; j < maxIndex; j++)
                {
                    char current = text[j];
                    ushort mask = (ushort)(1 << count);
                    if ((m_fastCheck[current] & mask) == 0)
                    {
                        if (count > 1)
                        {
                            index += (count - 1);
                        }
                        break;
                    }
                    ++count;
                    if ((m_endLength[current] & mask) != 0 && (m_startLength[begin] & mask) != 0)
                    {
                        if (m_hashSet.Contains(text, index, count))
                        {
                            if (chars == null) chars = text.ToArray();
                            for (int i = index; i < index + count; i++)
                            {
                                chars[i] = maskChar;
                            }
                            index += (count - 1);
                            break;
                        }
                    }
                }
            }
            return chars == null ? text : new string(chars);
        }
    }
}
