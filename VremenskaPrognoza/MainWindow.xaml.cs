using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace VremenskaPrognoza
{
    public partial class MainWindow : Window
    {
        private const string ApiKey = "4705475bbe9048e98ab185757230404";
        private const string AutoCompleteUrl = "http://api.weatherapi.com/v1/search.json?key={0}&q={1}";
        private readonly HttpClient _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new HttpClient();
        }

        private async void cityComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = cityComboBox.Text.Trim();
            if (input.Length >= 3)
            {
                string url = string.Format(AutoCompleteUrl, ApiKey, input);
                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    List<City> cities = JsonSerializer.Deserialize<List<City>>(responseBody);
                    cityComboBox.ItemsSource = cities;
                    cityComboBox.DisplayMemberPath = "DisplayName";
                    cityComboBox.IsDropDownOpen = true;
                }
            }
            else
            {
                cityComboBox.ItemsSource = null;
            }
        }

        private class City
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Region { get; set; }
            public string Country { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
            public string Url { get; set; }
            public string DisplayName => Name;
            public override string ToString()
            {
                return Name;
            }
        }
    }
}
