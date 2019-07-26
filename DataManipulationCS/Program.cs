using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManipulationCS
{
    public class Program
    {
        /// <summary>
        /// String to hold the data URL
        /// </summary>
        private const string URL = "https://recruitment.highfieldqualifications.com/api/gettest";
        private const string SendURL = "https://recruitment.highfieldqualifications.com/api/SubmitTest";

        /// <summary>
        /// Create HTTP client
        /// </summary>
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Item to hold all retrieved text in full
        /// </summary>
        public class FetchedItem
        {
            public string title { get; set; }
            public string details { get; set; }
            public string requestType { get; set; }
            public string uriToSubmit { get; set; }
            public string objectLayout { get; set; }
            public DataItem[] data { get; set; }
        }

        /// <summary>
        /// Item to hold each data item
        /// </summary>
        public class DataItem
        {
            public int id { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public DateTime dob { get; set; }
            public string favouriteColour { get; set; }
        }

        /// <summary>
        /// Item to hold the colour and count
        /// </summary>
        public class TopColour
        {
            public string Colour { get; set; }
            public int Amount { get; set; }
        }

        /// <summary>
        /// Item to hold all text to return
        /// </summary>
        public class ReturnItem
        {
            public List<int> AgePlus20 { get; set; }
            public Dictionary<string, int> TopColours { get; set; }

            public ReturnItem()
            {
                AgePlus20 = new List<int>();
                TopColours = new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Method to get all data from the URL
        /// </summary>
        /// <param name="path"> URL in its' entirety </param>
        /// <returns> Header and all data items </returns>
        static async Task<FetchedItem> GetProductAsync(string path)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            FetchedItem product = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<FetchedItem>();
            }
            return product;
        }

        static async Task SendResults(ReturnItem itemToSend)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(SendURL, itemToSend);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Assembles object to return to the server
        /// </summary>
        /// <param name="fetchedData"> All data fetched from the server </param>
        /// <returns> ReturnItem containing all data to send </returns>
        public static ReturnItem AssembleOutput(FetchedItem fetchedData)
        {
            ReturnItem dataToSend = new ReturnItem();

            // Get all ages plus 20 //
            foreach (DataItem item in fetchedData.data)
            {
                DateTime today = DateTime.Today;
                int age = today.Year - item.dob.Year;
                int agePlusTwenty = age + 20;
                dataToSend.AgePlus20.Add(agePlusTwenty);
            }

            // Get count of all favourite colours //
            Dictionary<string, int> FavouriteColourCount = new Dictionary<string, int>();
            foreach (DataItem item in fetchedData.data)
            {
                if (FavouriteColourCount.ContainsKey(item.favouriteColour))
                {
                    int tempVal = FavouriteColourCount[item.favouriteColour] + 1;
                    FavouriteColourCount[item.favouriteColour] = tempVal;
                }
                else
                {
                    FavouriteColourCount.Add(item.favouriteColour, 1);
                }
            }

            // Assemble ordered dictionary of all favourite colours //
            foreach (KeyValuePair<string, int> colour in FavouriteColourCount.OrderByDescending(key => key.Value))
            {  
                dataToSend.TopColours[colour.Key] =  colour.Value;
            }  

            return dataToSend;
        }

        /// <summary>
        /// Method to run all sections of web API interaction
        /// </summary>
        public static async Task RunInteraction()
        {
            FetchedItem retrievedData = await GetProductAsync(URL);

            ReturnItem dataToSend = AssembleOutput(retrievedData);

            await SendResults(dataToSend);
        }

        /// <summary>
        /// Main method to execute
        /// </summary>
        static void Main(string[] args)
        {
            RunInteraction().GetAwaiter().GetResult();
        }
    }
}
