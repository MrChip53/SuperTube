using CommunityToolkit.Mvvm.Input;
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
        List<SearchResult> _searchResults = new List<SearchResult>();
        public event PropertyChangedEventHandler PropertyChanged;

        public IAsyncRelayCommand PerformSearch { get; }

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

        private async Task ExecuteSearchAsync()
        {
            // TODO move somewhere else
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");

            using (var response = await client.GetAsync("https://9gag.com/v1/search-posts?query=" + SearchTerm))
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
        }

        public SearchPageViewModel() 
        {
            PerformSearch = new AsyncRelayCommand(ExecuteSearchAsync);
        }

        ~SearchPageViewModel()
        {
            _searchTerm = null;
            _searchResults = new List<SearchResult>();
        }

        public void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
