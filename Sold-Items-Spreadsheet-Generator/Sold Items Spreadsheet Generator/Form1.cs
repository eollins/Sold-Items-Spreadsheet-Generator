using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
                AddedLog.Items.Clear();
                AddedLog.Items.Add("Opened file");

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
                string data = await new StreamReader(filePath).ReadToEndAsync();
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
                AddedLog.Items.Clear();
                bool toBreak = false;
                foreach (string group in itemsByBrandAndYear.Keys)
                {
                    if (toBreak)
                        break;

                    List<SearchResult> groupResults = new List<SearchResult>();
                    progressBar2.Maximum = itemsByBrandAndYear.Keys.Count;
                    int numberOfPages = 2;
                    int count32 = 0;
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

                        XmlDocument doc = new XmlDocument();
                        try
                        {
                            WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&keywords=" + (year + " " + set + " PSA") + "&categoryId=" + 213 + "&sortOrder=PricePlusShippingLowest&paginationInput.entriesPerPage=200&paginationInput.pageNumber=" + i);
                            WebResponse response = await request.GetResponseAsync();
                            string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                            doc.LoadXml(xml);
                        }
                        catch
                        {
                            AddedLog.Items.Add("Request failed. Retrying.");
                            try
                            {
                                WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&keywords=" + (year + " " + set + " PSA") + "&categoryId=" + 213 + "&sortOrder=PricePlusShippingLowest&paginationInput.entriesPerPage=200&paginationInput.pageNumber=" + i);
                                WebResponse response = await request.GetResponseAsync();
                                string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                                doc.LoadXml(xml);
                            }
                            catch
                            {
                                AddedLog.Items.Add("Skipped call due to internal server error.");
                                continue;
                            }
                        }

                        numberOfPages = int.Parse(((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("paginationOutput")[0]).GetElementsByTagName("totalPages")[0].InnerText);

                        XmlNodeList nodes = ((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("searchResult")[0]).GetElementsByTagName("item");

                        foreach (XmlElement ele in nodes)
                        {
                            if (toBreak)
                                break;

                            SearchResult result = new SearchResult("", 0, "", "");

                            DateTime s = DateTime.Now;
                            bool found = false;
                            string player = "";
                            string title = ele.GetElementsByTagName("title")[0].InnerText.ToLower();
                            foreach (string name in names)
                            {
                                if (title.Contains(name.ToLower()))
                                {
                                    found = true;
                                    player = name;
                                    break;
                                }
                            }
                            //MessageBox.Show((DateTime.Now - s).TotalSeconds.ToString());

                            count32++;
                            //MessageBox.Show((start - DateTime.Now).TotalSeconds.ToString());
                            if (found)
                            {
                                result.Player = player;
                                result.SoldPrice = decimal.Parse(((XmlElement)doc.GetElementsByTagName("sellingStatus")[0]).GetElementsByTagName("currentPrice")[0].InnerText);
                                result.Year = year;
                                result.Set = set;
                                total++;

                                groupResults.Add(result);
                                AddedLog.Items.Add("Added " + player + ", " + total + ", " + count32);
                                label1.Text = total + " items added out of " + count32 + " results";
                                AddedLog.SelectedIndex = AddedLog.Items.Count - 1;

                                if (total > 100)
                                {
                                    toBreak = true;
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Skipped over " + ele.GetElementsByTagName("title")[0].InnerText.ToLower() + ", " + count32);
                                continue;
                            }
                        }
                    }

                    progressBar2.Maximum = itemsByBrandAndYear.Keys.Count;
                    progressBar2.Value++;

                    itemsFound.Add(group, groupResults);
                }

                //XmlDocument m = new XmlDocument();
                //m.Load(@"SampleData.xml");
                //XmlNodeList nodes2 = ((XmlElement)m.GetElementsByTagName("ListingData")[0]).GetElementsByTagName("Listing");
                //int total = 0;
                //foreach (XmlElement ex in nodes2)
                //{
                //    SearchResult result = new SearchResult("", 0, "", "");
                //    result.Year = ex.GetElementsByTagName("Year")[0].InnerText;
                //    result.Set = ex.GetElementsByTagName("Set")[0].InnerText;
                //    result.Player = ex.GetElementsByTagName("Player")[0].InnerText;
                //    result.SoldPrice = decimal.Parse(ex.GetElementsByTagName("SoldPrice")[0].InnerText);

                //    if (!itemsFound.ContainsKey(result.Year + result.Set))
                //    {
                //        itemsFound.Add(result.Year + result.Set, new List<SearchResult>());
                //    }

                //    itemsFound[result.Year + result.Set].Add(result);
                //    total++;
                //}

                XmlDocument doc2 = new XmlDocument();
                doc2.AppendChild(doc2.CreateElement("ListingData"));
                int count2 = 0;

                progressBar3.Maximum = total;
                foreach (string group in itemsFound.Keys)
                {
                    foreach (SearchResult result in itemsFound[group])
                    {
                        XmlElement el = doc2.CreateElement("Listing");
                        el.AppendChild(doc2.CreateElement("Year"));
                        el.AppendChild(doc2.CreateElement("Player"));
                        el.AppendChild(doc2.CreateElement("Set"));
                        el.AppendChild(doc2.CreateElement("SoldPrice"));
                        el.GetElementsByTagName("Year")[0].InnerText = result.Year;
                        el.GetElementsByTagName("Player")[0].InnerText = result.Player;
                        el.GetElementsByTagName("Set")[0].InnerText = result.Set;
                        el.GetElementsByTagName("SoldPrice")[0].InnerText = result.SoldPrice.ToString();
                        doc2.GetElementsByTagName("ListingData")[0].AppendChild(el);
                        count2++;
                        progressBar3.Value++;
                        label1.Text = count2 + " items compiled into data sheet";
                        
                        Debug.WriteLine(new ASCIIEncoding().GetByteCount(doc2.InnerXml) + ", " + group);
                    }
                }

                if (saveXML.ShowDialog() == DialogResult.OK)
                {
                    File.Create(saveXML.FileName).Close();
                    doc2.Save(saveXML.FileName);
                    MessageBox.Show("Saved.");
                }

                Application.Exit();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}