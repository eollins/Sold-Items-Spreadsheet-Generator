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
        Dictionary<string, List<SpreadsheetItem>> itemsByBrandAndYear = new Dictionary<string, List<SpreadsheetItem>>();
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

                //switch (selectType.SelectedIndex)
                //{
                //    case 0:
                //        RetrieveItems(openSpreadsheet.FileName, Path.GetExtension(openSpreadsheet.FileName), true);
                //        break;
                //    case 1:
                        RetrieveItems(openSpreadsheet.FileName, Path.GetExtension(openSpreadsheet.FileName), false);
                //        break;
                //}
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

                    if (!itemsByBrandAndYear.ContainsKey(item.Year + item.Set))
                    {
                        itemsByBrandAndYear.Add(item.Year + item.Set, new List<SpreadsheetItem>());
                    }

                    itemsByBrandAndYear[(item.Year + item.Set)].Add(item);
                    names.Add(item.Player);

                    progressBar1.Value++;
                    label1.Text = progressBar1.Value + " items grouped";
                }

                int count1 = progressBar1.Value;
                MessageBox.Show("There are " + itemsByBrandAndYear.Keys.Count + " groups.");

                textBox1.Text = "Searching eBay Databases";


                Dictionary<string, List<SearchResult>> itemsFound = new Dictionary<string, List<SearchResult>>();
                int total = 0;
                int callsPlaced = 0;
                foreach (string group in itemsByBrandAndYear.Keys)
                {
                    List<SearchResult> groupResults = new List<SearchResult>();
                    progressBar2.Maximum = itemsByBrandAndYear.Keys.Count;
                    int numberOfPages = 2;
                    for (int i = 1; i < numberOfPages; i++)
                    {
                        DateTime start = DateTime.Now;
                        callsPlaced++;
                        string year;
                        string set;
                        if (group.Contains("-"))
                        {
                            year = group.Substring(0, 7);
                            set = group.Substring(7);
                        }
                        else
                        {
                            year = group.Substring(0, 4);
                            set = group.Substring(4);
                        }

                        WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&keywords=" + (year + " " + set + " PSA") + "&categoryId=" + 213 + "&sortOrder=PricePlusShippingLowest&paginationInput.entriesPerPage=200&paginationInput.pageNumber=" + i);
                        WebResponse response = await request.GetResponseAsync();
                        string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);

                        numberOfPages = int.Parse(((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("paginationOutput")[0]).GetElementsByTagName("totalPages")[0].InnerText);

                        XmlNodeList nodes = ((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("searchResult")[0]).GetElementsByTagName("item");

                        foreach (XmlElement ele in nodes)
                        {
                            SearchResult result = new SearchResult("", 0, "", "");

                            DateTime s = DateTime.Now;
                            bool found = false;
                            string player = "";
                            foreach (string name in names)
                            {
                                if (ele.GetElementsByTagName("title")[0].InnerText.ToLower() == name.ToLower())
                                {
                                    found = true;
                                    player = name;
                                    break;
                                }
                            }
                            //MessageBox.Show((DateTime.Now - s).TotalSeconds.ToString());


                            MessageBox.Show((start - DateTime.Now).TotalSeconds.ToString());
                            if (found)
                            {
                                result.Player = player;
                                result.SoldPrice = decimal.Parse(((XmlElement)doc.GetElementsByTagName("sellingStatus")[0]).GetElementsByTagName("currentPrice")[0].InnerText);
                                result.Year = year;
                                result.Set = set;
                                total++;

                                groupResults.Add(result);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    itemsFound.Add(group, groupResults);
                }

                MessageBox.Show("Searches completed with " + callsPlaced + " calls and " + total + " total items mapped.");
            }
        }

        public async Task<XmlDocument> PlaceFindCompletedItemsCall(int page, string keywords, int categoryID)
        {
            WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&keywords=" + keywords + "&categoryId=" + categoryID + "&sortOrder=PricePlusShippingLowest&paginationInput.entriesPerPage=200");
            WebResponse response = await request.GetResponseAsync();
            string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}