using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    public class NamespacingDictionary<V> : IDictionary<string, V>
    {
        private IDictionary<string, V> _actual;
        private string _space;
        public NamespacingDictionary(IDictionary<string, V> actual, string space)
        {
            _actual = actual;
            _space = space;
        }

        public void Add(string key, V value)
        {
            _actual.Add(_space + key, value);
        }

        public bool ContainsKey(string key)
        {
            return _actual.ContainsKey(_space + key);
        }

        public ICollection<string> Keys
        {
            get
            {
                return (from k in _actual.Keys
                        where k.StartsWith(_space)
                        select k.Substring(_space.Length)).ToArray();
            }
        }

        public bool Remove(string key)
        {
            return _actual.Remove(_space + key);
        }

        public bool TryGetValue(string key, out V value)
        {
            return _actual.TryGetValue(_space + key, out value);
        }

        public ICollection<V> Values
        {
            get
            {
                return (from e in _actual
                        where e.Key.StartsWith(_space)
                        select e.Value).ToArray();
            }
        }

        public V this[string key]
        {
            get
            {
                return _actual[_space + key];
            }
            set
            {
                _actual[_space + key] = value;
            }
        }

        public void Add(KeyValuePair<string, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            foreach (var k in _actual.Keys)
            {
                if (k.StartsWith(_space))
                    _actual.Remove(k);
            }
        }

        public bool Contains(KeyValuePair<string, V> item)
        {
            return _actual.Contains(new KeyValuePair<string, V>(_space + item.Key, item.Value));
        }

        public void CopyTo(KeyValuePair<string, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return Keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return _actual.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, V> item)
        {
            return _actual.Remove(new KeyValuePair<string, V>(_space + item.Key, item.Value));
        }

        public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
        {
            return (from e in _actual
                    where e.Key.StartsWith(_space)
                    select new KeyValuePair<string, V>(e.Key.Substring(_space.Length), e.Value)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class Tombstoner
    {
        private string _id;
        public Tombstoner(string id)
        {
            _id = id;
        }
        public void TombstoneTo(IDictionary<string, object> state)
        {
            var namespacedState = new NamespacingDictionary<object>(state, _id);
            SaveState(namespacedState);
        }
        public void RestoreFrom(IDictionary<string, object> state)
        {
            var namespacedState = new NamespacingDictionary<object>(state, _id);
            RestoreState(namespacedState);
        }
        protected abstract void SaveState(IDictionary<string, object> state);
        protected abstract void RestoreState(IDictionary<string, object> state);
    }
}
