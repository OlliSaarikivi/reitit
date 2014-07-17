using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    public class SearchHistoryManager
    {
        public const int SearchHistorySize = 100;

        public List<string> SearchHistory { get; private set; }

        public SearchHistoryManager()
        {
            SearchHistory = App.Current.Settings.SearchHistory;
            //for (int i = 0; i < SearchHistorySize; ++i)
            //{
            //    SearchHistory.Add(RandomString(5));
            //}
        }

        Random random = new Random();

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public void Add(string search)
        {
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
