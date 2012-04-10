using System;
using System.Collections.Generic;
using System.Text;

namespace Gamefloor.Support
{
    /// <summary>
    /// Helper class to aid with the implementation of non-default indexers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Indexer1d<T>
    {
        public Indexer1d(Getter1d<T> getter, Setter1d<T> setter)
        {
            m_getter = getter;
            m_setter = setter;
        }

        private Getter1d<T> m_getter;
        private Setter1d<T> m_setter;

        public T this[int index]
        {
            get
            {
                return m_getter(index);
            }
            set
            {
                m_setter(index, value);
            }
        }
    }

    public delegate T Getter1d<T>(int index);
    public delegate void Setter1d<T>(int index, T value);
}
