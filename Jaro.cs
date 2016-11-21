using System;
using System.Collections.Generic;

namespace JaroDistance
{

    public struct JaroCalculator
    {

        public const int MaxWordSize = 64;
        private const ulong HighBit = 0x8000000000000000;

        private char[] ss;
        private char[] tt;
        private int ssp;
        private int ttp;
        private Mask64Bit[] matrix;

        private static KeyValueCollection<char, char> mapping = null;

        public static JaroCalculator New()
        {
            JaroCalculator c = new JaroCalculator();
            c.ss = new char[JaroCalculator.MaxWordSize];
            c.tt = new char[JaroCalculator.MaxWordSize];
            c.ssp = 0;
            c.ttp = 0;
            c.matrix = new Mask64Bit[JaroCalculator.MaxWordSize];
            return c;
        }

        public static string[] SplitAndNormalize(string s)
        {
            if (s == null)
            {
                JaroCalculator.mapping = new KeyValueCollection<char, char>(128, 32, JaroCalculator.Compare);
                JaroCalculator.AddMapping(
                    @"харос",
                    @"xapoc");
                return null;
            }
            char[] buf = new char[JaroCalculator.MaxWordSize];
            int p = 0;
            List<string> l = new List<string>();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (Char.IsLetter(c))
                {
                    if (p >= JaroCalculator.MaxWordSize) continue;
                    c = Char.ToLower(c);
                    if (JaroCalculator.mapping.ContainsKey(c))
                        c = JaroCalculator.mapping[c];
                    buf[p++] = c;
                    continue;
                }
                switch (c)
                {
                    case '\'':
                    case '`':
                        if (p < JaroCalculator.MaxWordSize)
                            buf[p++] = '\'';
                        break;

                    case '-':
                        if (p < JaroCalculator.MaxWordSize)
                            buf[p++] = '-';
                        break;

                    default:
                        if (p > 0)
                        {
                            l.Add(new String(buf, 0, p));
                            p = 0;
                        }
                        break;
                }
            }
            if (p > 0) l.Add(new String(buf, 0, p));
            buf = null;
            if (l.Count == 0) return null;
            return l.ToArray();
        }

        private static void AddMapping(string from, string to)
        {
            if (from.Length != to.Length) return;
            for (int i = 0; i < from.Length; i++)
                JaroCalculator.mapping[Char.ToLower(from[i])] = Char.ToLower(to[i]);
        }

        private static int Compare(char x, char y)
        {
            return ((ushort)x).CompareTo((ushort)y);
        }

        public float Calculate(string s, string t)
        {
            if ((s == null) || (t == null))
                return 0f;

            int sl = s.Length;
            int tl = t.Length;
            if ((sl == 0) || (tl == 0))
                return 0f;
            int H = (((tl > sl) ? tl : sl) >> 1) - 1;

            if (sl > JaroCalculator.MaxWordSize) sl = JaroCalculator.MaxWordSize;
            if (tl > JaroCalculator.MaxWordSize) tl = JaroCalculator.MaxWordSize;

            this.ssp = 0;
            this.ttp = 0;
            for (int i = 0; i < JaroCalculator.MaxWordSize; i++)
                this.matrix[i].Mask = 0;

            int l = (H << 1) + 1;
            for (int j = 0; j < tl; j++)
            {
                int i = j - H;
                for (int k = 0; k < l; k++)
                {
                    if ((i < 0) || (i >= sl))
                    {
                        i++;
                        continue;
                    }
                    if (s[i] == t[j])
                    {
                        this.matrix[j].Mask |= JaroCalculator.HighBit >> i;
                    }
                    i++;
                }
            }

            l = (H << 1);
            Mask64Bit m = new Mask64Bit(JaroCalculator.HighBit);
            for (int i = 0; i < H; i++)
            {
                m.Mask = (m.Mask >> 1) | JaroCalculator.HighBit;
                l--;
            }

            for (int j = 0; j < tl; j++)
            {
                if ((this.matrix[j].Mask & m.Mask) != 0)
                    this.tt[this.ttp++] = t[j];
                m.Mask = m.Mask >> 1;
                if (l > 0)
                {
                    m.Mask |= JaroCalculator.HighBit;
                    l--;
                }
            }

            if (this.ttp == 0) return 0f;

            l = (H << 1) + 1;
            m.Mask = JaroCalculator.HighBit;
            for (int i = 0; i < sl; i++)
            {
                int j = i - H;
                for (int k = 0; k < l; k++)
                {
                    if ((j < 0) || (j >= tl))
                    {
                        j++;
                        continue;
                    }
                    if ((this.matrix[j].Mask & m.Mask) != 0)
                    {
                        this.ss[this.ssp++] = s[i];
                        break;
                    }
                    j++;
                }
                m.Mask = m.Mask >> 1;
            }

            if (this.ssp == 0) return 0f;

            int T = 0;
            l = (this.ssp < this.ttp) ? this.ssp : this.ttp;
            for (int i = 0; i < l; i++)
                if (this.ss[i] != this.tt[i]) T++;

            double d = (((double)this.ssp / (double)sl) + 
                        ((double)this.ttp / (double)tl) + 
                        (((double)this.ssp - ((double)T / 2.0f)) / (double)this.ssp)) / 3.0f;

            return (float)d;
        }

        struct Mask64Bit
        {
            public ulong Mask;

            public Mask64Bit(ulong v)
            {
                this.Mask = v;
            }

            public override string ToString()
            {
                ulong m = JaroCalculator.HighBit;
                string s = String.Empty;
                for (int i = 0; i < JaroCalculator.MaxWordSize; i++)
                {
                    s += ((this.Mask & m) == 0) ? "0" : "1";
                    m = m >> 1;
                }
                return s;
            }
        }
    }

    public sealed class KeyValueCollection<TKey, TValue>
    {
        private Func<TKey, TKey, int> comparer;
        private CollectionItem[] items;
        private int count;
        private int growSize;

        public KeyValueCollection(int size, int growSize, Func<TKey, TKey, int> comparer)
        {
            this.count = 0;
            this.items = new CollectionItem[size];
            this.comparer = comparer;
            this.growSize = growSize;
        }

        public void Clear()
        {
            this.count = 0;
        }

        public int Count
        {
            get { return this.count; }
        }

        public TKey[] Keys()
        {
            TKey[] keys = new TKey[this.count];
            for (int i = 0; i < this.count; i++)
                keys[i] = this.items[i].Key;
            return keys;
        }

        public TKey Key(int i)
        {
            if ((i < 0) || (i >= this.count))
                throw new IndexOutOfRangeException();
            return this.items[i].Key;
        }

        public TValue Value(int i)
        {
            if ((i < 0) || (i >= this.count))
                throw new IndexOutOfRangeException();
            return this.items[i].Value;
        }

        public bool ContainsKey(TKey key)
        {
            int p;
            return this.Find(key, out p);
        }

        public TValue this[TKey key, TValue defaultValue]
        {
            get
            {
                int p;
                if (!this.Find(key, out p))
                    return defaultValue;
                return this.items[p].Value;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int p;
                if (!this.Find(key, out p))
                    throw new ArgumentException(String.Format("Key '{0}' not found", key.ToString()));
                return this.items[p].Value;
            }
            set
            {
                int p;
                if (!this.Find(key, out p))
                {
                    this.count++;
                    if (this.count == this.items.Length)
                        this.Resize();
                    int i = this.count;
                    while (i > p)
                        this.items[i] = this.items[--i];
                    this.items[p].Key = key;
                }
                this.items[p].Value = value;
            }
        }

        private void Resize()
        {
            int l = this.items.Length;
            l = l + ((this.growSize <= 0) ? (l >> 1) : this.growSize);
            CollectionItem[] list = new CollectionItem[l];
            for (int i = 0; i < this.count; i++)
                list[i] = this.items[i];
            this.items = null;
            this.items = list;
            list = null;
        }

        private bool Find(TKey key, out int position)
        {
            position = 0;
            if (this.count == 0)
                return false;
            int min = 0;
            int max = this.count - 1;
            while (true)
            {
                int mid = (min + max) >> 1;
                int r = this.comparer(key, this.items[mid].Key);
                if (r == 0)
                {
                    position = mid;
                    return true;
                }
                if (r > 0)
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
                if (min > max) break;
            }
            position = min;
            return false;
        }

        struct CollectionItem
        {
            public TKey Key;
            public TValue Value;

            public override string ToString()
            {
                return String.Format("{0}: {1}", this.Key.ToString(), this.Value.ToString());
            }
        }
    }
}