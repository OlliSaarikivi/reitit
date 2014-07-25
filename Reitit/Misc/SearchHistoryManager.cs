using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    public class SearchHistoryManager
    {
        public const int SearchHistorySize = 500;

        public List<string> SearchHistory { get; private set; }

        public SearchHistoryManager(List<string> history)
        {
            SearchHistory = history;
        }

        public void Add(string search)
        {
            SearchHistory.Remove(search);
            SearchHistory.Insert(0, search);
            while (SearchHistory.Count > SearchHistorySize)
            {
                SearchHistory.RemoveAt(SearchHistory.Count - 1);
            }
        }

        public string[] SearchesWithPrefix(string prefix)
        {
            return (from search in SearchHistory
                    where search.StartsWith(prefix)
                    select search).ToArray();
        }
    }
}
