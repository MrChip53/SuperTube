using CommunityToolkit.Mvvm.Input;
using SuperTube.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<SearchResult> SearchResults { get; private set; } = new ObservableCollection<SearchResult>();

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

                SearchResults.Clear();

                foreach (var item in postArray)
                {
                    SearchResults.Add(new SearchResult
                    {
                        Id = (string)item["id"],
                        ImageUrl = (string)item["images"]["imageFbThumbnail"]["url"],
                        Title = (string)item["title"],
                        Description = (string)item["description"]
                    });
                }
            }
        }

        public SearchPageViewModel() 
        {
            PerformSearch = new AsyncRelayCommand(ExecuteSearchAsync);
        }

        ~SearchPageViewModel()
        {
            _searchTerm = null;
        }

        public void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
