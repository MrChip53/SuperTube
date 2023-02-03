using SuperTube.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperTube.ViewModels
{
    internal class SearchPageViewModel : INotifyPropertyChanged
    {
        string _searchTerm = null;
        bool _isSearching = false;
        List<SearchResult> _searchResults = new List<SearchResult>();
        public event PropertyChangedEventHandler PropertyChanged;

        public IAsyncCommand<string> PerformSearch => new AsyncCommand<string>(ExecuteSearchAsync, CanExecuteSearch);

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (_isSearching != value)
                {
                    _isSearching = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<SearchResult> SearchResults
        {
            get => _searchResults;
            set
            {
                if (value.Except(_searchResults).ToList().Count > 0)
                {
                    _searchResults = value;
                    OnPropertyChanged();
                }
            }
        }

        private async Task ExecuteSearchAsync(string query)
        {
            // TODO move somewhere else
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");

            SearchTerm = null;
            IsSearching = true;

            using (var response = await client.GetAsync("https://9gag.com/v1/search-posts?query=" + query))
            {
                var json = await response.Content.ReadAsStringAsync();

                JsonObject jsonObj = JsonNode.Parse(json).AsObject();

                var postArray = jsonObj["data"]["posts"].AsArray();

                var allResults = new List<SearchResult>();

                foreach (var item in postArray)
                {
                    var result = new SearchResult
                    {
                        Id = (string)item["id"],
                        ImageUrl = (string)item["images"]["imageFbThumbnail"]["url"],
                        Title = (string)item["title"],
                        Description = (string)item["description"]
                    };

                    allResults.Add(result);
                }

                SearchResults = allResults;

                SearchTerm = string.Join(", ", allResults.Select(x => x.Title));
            }

            IsSearching = false;
        }

        private bool CanExecuteSearch(string query) => !IsSearching;

        public SearchPageViewModel() 
        { 
        }

        ~SearchPageViewModel()
        {
            _searchTerm = null;
            _isSearching = false;
            _searchResults = new List<SearchResult>();
        }

        public void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
