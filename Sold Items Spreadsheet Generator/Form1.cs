using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading; 
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Sold_Items_Spreadsheet_Generator
{
    public partial class Form1 : Form
    {
        Dictionary<string, List<SpreadsheetItem>> itemsByYear = new Dictionary<string, List<SpreadsheetItem>>();
        Dictionary<string, List<SpreadsheetItem>> itemsByQuality = new Dictionary<string, List<SpreadsheetItem>>();
        Dictionary<string, List<SpreadsheetItem>> itemsByBoth = new Dictionary<string, List<SpreadsheetItem>>();
        List<string> names = new List<string>();

        List<SpreadsheetItem> items = new List<SpreadsheetItem>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Begin_Click(object sender, EventArgs e)
        {
            DialogResult result = openSpreadsheet.ShowDialog();
            if (result == DialogResult.OK)
            {
                Log.Items.Clear();
                Log.Items.Add("Opened file");

                switch (selectType.SelectedIndex)
                {
                    case 0:
                        RetrieveItems(openSpreadsheet.FileName, Path.GetExtension(openSpreadsheet.FileName), true);
                        break;
                    case 1:
                        RetrieveItems(openSpreadsheet.FileName, Path.GetExtension(openSpreadsheet.FileName), false);
                        break;
                }
            }
            else if (result != DialogResult.Cancel)
            {
                MessageBox.Show("Could not open file.");
            }
        }

        private async void RetrieveItems(string filePath, string fileType, bool graded)
        {
            List<SpreadsheetItem> items = new List<SpreadsheetItem>();
            Begin.Enabled = false;
            progressBar1.Value = 0;
            progressBar2.Value = 0;
            progressBar3.Value = 0;

            textBox1.Text = "Grouping Items";

            if (fileType == ".xlsx")
            {
                
            }
            else if (fileType == ".csv")
            {
                string data =  new StreamReader(filePath).ReadToEnd();
                string[] lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                progressBar1.Maximum = lines.Length;

                int count = 0;
                foreach (string line in lines)
                {
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }

                    string[] components = line.Split(',');

                    if (components.Length != 11)
                    {
                        if (components[4][0] == '"' && components[5][components[5].Length - 1] == '"')
                        {
                            string[] temp = components;
                            components = new string[] { temp[0], temp[1], temp[2], temp[3], temp[4] + temp[5], temp[6], temp[7], temp[8], temp[9], temp[10], temp[11] + temp[12] };
                        }
                    }

                    SpreadsheetItem item = new SpreadsheetItem(components[0], components[1], components[2], components[3], components[4], int.Parse(components[5]), components[6], components[7], components[8], components[9], components[10]);

                    if (!itemsByBoth.ContainsKey(item.Year))
                    {
                        itemsByBoth.Add(item.Year, new List<SpreadsheetItem>());
                    }

                    itemsByBoth[item.Year].Add(item);
                    names.Add(item.Player);

                    progressBar1.Value++;
                    label1.Text = progressBar1.Value + " items grouped";
                }

                int count1 = progressBar1.Value;

                textBox1.Text = "Searching eBay Databases";


                List<SearchResult> itemsFound = new List<SearchResult>();
                List<string> newNames = new List<string>();
                int count2 = 0;
                int total = 0;
                foreach (string year in itemsByBoth.Keys)
                {
                    progressBar2.Maximum = names.Count;

                    bool tobreak = false;

                    int numberOfPages = 2;
                    for (int i = 1; i < numberOfPages; i++)
                    {
                        WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&keywords=" + year + "&categoryId=213&categoryId=214&categoryId=215&categoryId=216&categoryId=37795&itemFilter(0).name=Professional%20Grader&itemFilter(0).value=Professional%20Sports%20(PSA)&listingType=Auction&paginationInput.entriesPerPage=200&paginationInput.pageNumber=" + i);
                        WebResponse response = await request.GetResponseAsync();

                        string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);

                        numberOfPages = int.Parse(((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("paginationOutput")[0]).GetElementsByTagName("totalPages")[0].InnerText);

                        XmlNodeList responseItems = ((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("searchResult")[0]).GetElementsByTagName("item");
                        foreach (XmlElement el in responseItems)
                        {
                            SearchResult result = new SearchResult();
                            result.Year = year;
                            result.SoldPrice = decimal.Parse(((XmlElement)el.GetElementsByTagName("sellingStatus")[0]).GetElementsByTagName("convertedCurrentPrice")[0].InnerText);

                            string title = el.GetElementsByTagName("title")[0].InnerText.ToLower();

                            if (title.Contains("goudey sport kings"))
                            {
                                title.Replace("goudey sport kings", " ");
                            }

                            foreach (string player in names)
                            {
                                if (title.Contains(player.ToLower()))
                                {
                                    result.Player = player;
                                    break;
                                }
                            }

                            progressBar2.Value++;

                            if (result.Player != null)
                            {
                                itemsFound.Add(result);
                                count2++;
                            }

                            total++;
                            newNames.Add(result.Player);
                        }

                        //MessageBox.Show(year + " " + count2 + "/" + total);
                        count = 0;

                        textBox1.Text = itemsFound.Count.ToString();
                        //total = 0;
                        if (itemsFound.Count >= 250)
                        {
                            tobreak = true;
                        }
                    }

                    if (tobreak)
                    {
                        break;
                    }
                }

                List<string> playersAlreadyVerified = new List<string>();
                int numberOfPlayers = 0;
                foreach (SearchResult result in itemsFound)
                {
                    if (!playersAlreadyVerified.Contains(result.Player))
                    {
                        playersAlreadyVerified.Add(result.Player);
                        numberOfPlayers++;
                    }
                }



                MessageBox.Show(numberOfPlayers.ToString() + " players from " + total + " results");






                /*List<SearchResult> itemsFound = new List<SearchResult>();
                foreach (string category in itemsByBoth.Keys)
                {
                    progressBar2.Maximum = names.Count;

                    //Tomorrow:
                    //Find ways to determine quality from listing
                    //Organize accordingly

                    int numberOfPages = 2;
                    for (int i = 1; i < numberOfPages; i++)
                    {
                        WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&keywords=" + category.Split(' ')[0] + "&categoryId=213&categoryId=214&categoryId=215&categoryId=216&categoryId=37795&itemFilter(0).name=Professional%20Grader&itemFilter(0).value=Not%20Professionally%20Graded&listingType=Auction&paginationInput.entriesPerPage=200&paginationInput.pageNumber=" + i);
                        WebResponse response = await request.GetResponseAsync();

                        string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);

                        numberOfPages = int.Parse(((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("paginationOutput")[0]).GetElementsByTagName("totalPages")[0].InnerText);

                        XmlNodeList responseItems = ((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("searchResult")[0]).GetElementsByTagName("item");
                        foreach (XmlElement el in responseItems)
                        {
                            string title = el.GetElementsByTagName("title")[0].InnerText.ToLower();
                            
                            SearchResult result = new SearchResult();
                            if (title.Contains(category.Split(' ')[1]))
                                result.Year = int.Parse(category.Split(' ')[1]);
                            result.SoldPrice = decimal.Parse(((XmlElement)el.GetElementsByTagName("sellingStatus")[0]).GetElementsByTagName("convertedCurrentPrice")[0].InnerText);

                            if (title.Contains("goudey sport kings"))
                            {
                                title.Replace("goudey sport kings", " ");
                            }

                            foreach (string player in names)
                            {
                                if (title.Contains(player))
                                { 
                                    result.Player = player;
                                    break;
                                }
                            }

                            progressBar2.Value++;

                            if (result.Player != null)
                            {
                                itemsFound.Add(result);
                                MessageBox.Show("item found");
                            }
                        }
                    }*/

                    
                        
                        

                    //parse through xml here and assign bits of xml to cards, calculate prices and update the dictionaries. at the end compile into spreadsheet.
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            selectType.SelectedIndex = 0;
        }
    }
}