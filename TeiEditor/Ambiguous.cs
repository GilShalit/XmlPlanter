using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeiEditor
{
    public class Ambiguous
    {
        private List<KeyValuePair<string, string>> dictionary;
        public Ambiguous()
        {
            dictionary = new List<KeyValuePair<string, string>>();
        }

        public void Add(string key, string value)
        {
            dictionary.Add(new KeyValuePair<string, string>(key, value));
        }

        public bool ContainsKey(string key)
        {
            return dictionary.Where(kvp => kvp.Key == key).Any();
        }

        public List<string> Values(string key)
        {
            return dictionary.Where(kvp => kvp.Key == key).Select(kvp => kvp.Value).ToList();
        }

        public string Value(string key)
        {
            List<string> values = Values(key);
            if (values != null || values.Count == 1) return values[0];
            else return null;
        }

        public bool isAmbiguous(string key)
        {
            List<string> values = Values(key);
            if (values == null || values.Count == 1) return false;
            else return true;
        }

        public void Clear()
        {
            dictionary.Clear();
        }
    }
}
