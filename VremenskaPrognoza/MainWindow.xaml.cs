﻿using System;
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
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Shapes;


namespace VremenskaPrognoza
{
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private const string ApiKey = "4705475bbe9048e98ab185757230404";
        private const string AutoCompleteUrl = "http://api.weatherapi.com/v1/search.json?key={0}&q={1}";
        private const string CurrentUrl = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}&aqi=no";

        private const string ForecastUrl =
            "http://api.weatherapi.com/v1/forecast.json?key={0}&q={1}&days={2}&aqi=no&alerts=yes";
        private readonly HttpClient _client;

        private int ChosenDay;

        public MainWindow()
        {
            ChosenDay = 8;

            InitializeComponent();
            _client = new HttpClient();
            //generateGraph();
            
        }

        private async void cityComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = CityComboBox.Text.Trim();
            if (input.Length >= 3)
            {
                string url = string.Format(AutoCompleteUrl, ApiKey, input);
                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    List<City> cities = JsonSerializer.Deserialize<List<City>>(responseBody);
                    CityComboBox.ItemsSource = cities;
                    CityComboBox.DisplayMemberPath = "DisplayName";
                    CityComboBox.IsDropDownOpen = true;
                }
            }
            else
            {
                CityComboBox.ItemsSource = null;
            }
        }
        private async void CityComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CityComboBox.SelectedItem != null)
            {
                string selectedItem = CityComboBox.SelectedItem.ToString();
                string url = string.Format(ForecastUrl, ApiKey, selectedItem, 1);
                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    WeatherData data = JsonSerializer.Deserialize<WeatherData>(responseBody);
                    
                    CityComboBox.Text = selectedItem;
                    
                    CityComboBox.IsDropDownOpen = false;
                    CityComboBox.ItemsSource = null;
                    mapResponseToBoxes(data);
                    ChartValues<double> values = new ChartValues<double> { };
                    for(int i = 0; i < 24; i++)
                    {
                        values.Add(data.forecast.forecastday[0].hour[i].pressure_mb);
                    }
                    generateGraph("Pressure_Mb", values);
                }
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //TO-DO: Uzeti podatke grada, datuma i parametra koji se selektuje, poslati upit i proslediti dalje u ovu metodu
        private async void generateGraph(string title, ChartValues<double> values)
        {
            try
            {
                if (SeriesCollection != null)
                {
                    SeriesCollection.Clear();
                    SeriesCollection.Add(new LineSeries
                    {
                        Title = title,
                        Values = values
                    });
                }
                else
                {
                    SeriesCollection = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = title,
                            Values = values
                        }
                    };
                    Labels = new[] { "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00" };
                }
            }
            catch { }
            DataContext = this;
        }

        private void mapResponseToBoxes(WeatherData data)
        {
            string iconUri = data.current.condition.icon;
            // BitmapImage bitmap = new BitmapImage();
            // bitmap.UriSource = new Uri(iconUri);
            currentWeatherImage.Source = new BitmapImage(new Uri("http:"+iconUri));
            
            BigTemp.Text = data.current.temp_c.ToString() + "° C";
            minMax.Text = data.forecast.forecastday[0].day.mintemp_c.ToString() + "/" +
                          data.forecast.forecastday[0].day.maxtemp_c.ToString()+ "° C";
            // maxTemp.Text = data.forecast.forecastday[0].day.maxtemp_c.ToString() + "° C";
            // minTemp.Text = data.forecast.forecastday[0].day.mintemp_c.ToString() + "° C";
            if (data.forecast.forecastday[0].day.daily_chance_of_rain <
                data.forecast.forecastday[0].day.daily_chance_of_snow)
            {
                chanceOfRainSnow.Text = "Chance of snow: ";
                rainSnow.Text = data.forecast.forecastday[0].day.daily_chance_of_snow.ToString() + "%";
            }
            else
            {
                chanceOfRainSnow.Text = "Chance of rain: ";
                rainSnow.Text = data.forecast.forecastday[0].day.daily_chance_of_rain.ToString() + "%";
            }
            feelsLike.Text = data.current.feelslike_c.ToString() + "° C";
            windDirection.Text = data.current.wind_dir;
            precipitation.Text = data.current.precip_mm.ToString() + "mm";
            humidity.Text = data.current.humidity.ToString() + "%";
            uv.Text = data.current.uv.ToString();
            clouds.Text = data.current.cloud.ToString() + "%";
            windSpeed.Text = data.current.wind_kph.ToString() + " kph";
            visibility.Text = data.current.vis_km.ToString() + "km";
            pressure.Text = data.current.pressure_mb.ToString() +"Mbar";
            // alert.Text = data.alerts.alerts.ToString();

        }
        private class City
        {
            public int Id { get; set; }
            public string name { get; set; }
            public string region { get; set; }
            public string country { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public string url { get; set; }
            public string DisplayName => name;
            public override string ToString()
            {
                return name;
            }
        }
        public class Location
        {
            public string name { get; set; }
            public string region { get; set; }
            public string country { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public string tz_id { get; set; }
            public long localtime_epoch { get; set; }
            public string localtime { get; set; }
        }

        public class Condition
        {
            public string text { get; set; }
            public string icon { get; set; }
            public int code { get; set; }
        }

        public class Current
        {
            public long last_updated_epoch { get; set; }
            public string last_updated { get; set; }
            public double temp_c { get; set; }
            public double temp_f { get; set; }
            public int is_day { get; set; }
            public Condition condition { get; set; }
            public double wind_mph { get; set; }
            public double wind_kph { get; set; }
            public int wind_degree { get; set; }
            public string wind_dir { get; set; }
            public double pressure_mb { get; set; }
            public double pressure_in { get; set; }
            public double precip_mm { get; set; }
            public double precip_in { get; set; }
            public int humidity { get; set; }
            public int cloud { get; set; }
            public double feelslike_c { get; set; }
            public double feelslike_f { get; set; }
            public double vis_km { get; set; }
            public double vis_miles { get; set; }
            public double uv { get; set; }
            public double gust_mph { get; set; }
            public double gust_kph { get; set; }
        }

        public class WeatherData
        {
            public Location location { get; set; }
            public Current current { get; set; }
            public Forecast forecast { get; set; }
            public Alerts alerts { get; set; }
        }

        public class Alerts
        {
            public List<Alert> alerts { get; set; }
        }

        public class Alert
        {
            
        }
        public class Forecast
        {
            public List<ForecastDay> forecastday { get; set; }
        }
        public class ForecastDay
        {
            public string date { get; set; }
            public long date_epoch { get; set; }
            public Day day { get; set; }
            public Astro astro { get; set; }
            public List<Hour> hour { get; set; }
        }
        
        public class Day
        {
            public double maxtemp_c { get; set; }
            public double maxtemp_f { get; set; }
            public double mintemp_c { get; set; }
            public double mintemp_f { get; set; }
            public double avgtemp_c { get; set; }
            public double avgtemp_f { get; set; }
            public double maxwind_mph { get; set; }
            public double maxwind_kph { get; set; }
            public double totalprecip_mm { get; set; }
            public double totalprecip_in { get; set; }
            public double totalsnow_cm { get; set; }
            public double avgvis_km { get; set; }
            public double avgvis_miles { get; set; }
            public double avghumidity { get; set; }
            public int daily_will_it_rain { get; set; }
            public int daily_chance_of_rain { get; set; }
            public int daily_will_it_snow { get; set; }
            public int daily_chance_of_snow { get; set; }
            public Condition condition { get; set; }
            public double uv { get; set; }
            
        }
        public class Hour
        {
            public int time_epoch { get; set; }
            public string time { get; set; }
            public double temp_c { get; set; }
            public double temp_f { get; set; }
            public int is_day { get; set; }
            public Condition condition { get; set; }
            public double wind_mph { get; set; }
            public double wind_kph { get; set; }
            public int wind_degree { get; set; }
            public string wind_dir { get; set; }
            public double pressure_mb { get; set; }
            public double pressure_in { get; set; }
            public double precip_mm { get; set; }
            public double precip_in { get; set; }
            public int humidity { get; set; }
            public int cloud { get; set; }
            public double feelslike_c { get; set; }
            public double feelslike_f { get; set; }
            public double windchill_c { get; set; }
            public double windchill_f { get; set; }
            public double heatindex_c { get; set; }
            public double heatindex_f { get; set; }
            public double dewpoint_c { get; set; }
            public double dewpoint_f { get; set; }
            public int will_it_rain { get; set; }
            public int chance_of_rain { get; set; }
            public int will_it_snow { get; set; }
            public int chance_of_snow { get; set; }
            public double vis_km { get; set; }
            public double vis_miles { get; set; }
            public double gust_mph { get; set; }
            public double gust_kph { get; set; }
            public double uv { get; set; }
        }
        public class Astro
        {
            public string sunrise { get; set; }
            public string sunset { get; set; }
            public string moonrise { get; set; }
            public string moonset { get; set; }
            public string moon_phase { get; set; }
            public string moon_illumination { get; set; }
            public int is_moon_up { get; set; }
            public int is_sun_up { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
