using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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

            if (fileType == ".xlsx")
            {
                
            }
            else if (fileType == ".csv")
            {
                string data =  new StreamReader(filePath).ReadToEnd();
                string[] lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                progressBar1.Maximum = lines.Length;

                int count = 0;
                foreach (string item in lines)
                {
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }
                    
                    string[] components = item.Split(',');

                    SpreadsheetItem spreadsheetItem = new SpreadsheetItem("", "", "", "", "", 0, "", "", "", "", "");
                    try
                    {
                        string four = "";
                        string twelve = "";
                        if (components.Length == 13)
                        {
                            spreadsheetItem = new SpreadsheetItem(components[0], components[1], components[2], components[3], components[4] + " " + components[5], int.Parse(components[6]), components[7], components[8], components[9], components[10], components[11] + " " + components[12]);
                        }
                        else
                        {
                            spreadsheetItem = new SpreadsheetItem(components[0], components[1], components[2], components[3], components[4], int.Parse(components[5]), components[6], components[7], components[8], components[9], components[10]);
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    int category = 0;
                    switch (spreadsheetItem.Sport)
                    {
                        case "Baseball":
                            category = 213;
                            break;
                        case "Basketball":
                            category = 214;
                            break;
                        case "Football":
                            category = 215;
                            break;
                        case "Hockey":
                            category = 216;
                            break;
                        case "Boxing":
                            category = 37795;
                            break;
                        default:
                            category = -1;
                            break;
                    }

                    string keywords = spreadsheetItem.Player + " " + spreadsheetItem.Year + " " + spreadsheetItem.Set;
                    string link = "http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&keywords=" + keywords + "&categoryId=" + category + "&listingType=Auction&paginationInput.entriesPerPage=200";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
                    Log.Items.Add("Created web request for " + keywords);

                    WebResponse response = null;
                    if (category != -1)
                    {
                        try
                        {
                            response = await request.GetResponseAsync();
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("Internal Server Error"))
                            {
                                try
                                {
                                    Log.Items.Add("Error. Retrying in 5 seconds");
                                    Thread.Sleep(5000);
                                    response = await request.GetResponseAsync();
                                }
                                catch
                                {
                                    Log.Items.Add("Error. Retrying in 10 seconds");
                                    bool finished = false;
                                    DateTime start = DateTime.Now;
                                    while (!finished)
                                    {
                                        Thread.Sleep(10000);
                                        try
                                        {
                                            response = await request.GetResponseAsync();
                                            finished = true;
                                        }
                                        catch
                                        {
                                            if ((DateTime.Now - start).TotalMinutes >= 5)
                                            {
                                                MessageBox.Show("Calls could not be made.");
                                                Application.Exit();
                                            }
                                        }
                                    }
                                }

                                Log.Items.Add("Web request for " + keywords + " failed");
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                    bool completed = true;

                    Log.Items.Add("Requested items for " + keywords);

                    if (completed)
                    {
                        XmlDocument doc = new XmlDocument();
                        string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                        doc.LoadXml(xml);

                        XmlNodeList nodes = ((XmlElement)((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("searchResult")[0]).GetElementsByTagName("item");
                        int amount = int.Parse(((XmlElement)doc.GetElementsByTagName("findCompletedItemsResponse")[0]).GetElementsByTagName("searchResult")[0].Attributes[0].Value);

                        Log.Items.Add("Found " + amount + " results for " + keywords);

                        if (amount == 0)
                            continue;

                        List<XmlElement> itemsToCheck = new List<XmlElement>();
                        if (graded)
                        {
                            foreach (XmlElement el in nodes)
                            {
                                try
                                {
                                    WebRequest getSingleItems = WebRequest.Create("http://open.api.ebay.com/shopping?callname=GetSingleItem&responseencoding=XML&appid=GregoryM-mailer-PRD-a45ed6035-97c14545&siteid=0&version=967&ItemID=" + el.GetElementsByTagName("itemId")[0].InnerText + "&IncludeSelector=ItemSpecifics");
                                    WebResponse gsiResp = await getSingleItems.GetResponseAsync();
                                    string responseXml = await new StreamReader(gsiResp.GetResponseStream()).ReadToEndAsync();
                                    XmlDocument r = new XmlDocument();
                                    r.LoadXml(responseXml);
                                    XmlNodeList nodes2 = ((XmlElement)((XmlElement)((XmlElement)r.GetElementsByTagName("GetSingleItemResponse")[0]).GetElementsByTagName("Item")[0]).GetElementsByTagName("ItemSpecifics")[0]).GetElementsByTagName("NameValueList");
                                    foreach (XmlElement ele in nodes2)
                                    {
                                        if (ele.GetElementsByTagName("Name")[0].InnerText == "Grade")
                                        {
                                            itemsToCheck.Add(el);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                            }
                        }
                        else
                        {
                            foreach (XmlElement el in nodes)
                            {
                                itemsToCheck.Add(el);
                            }
                        }

                        List<decimal> prices = new List<decimal>();
                        foreach (XmlElement el in itemsToCheck)
                        {
                            decimal price = decimal.Parse(((XmlElement)doc.GetElementsByTagName("sellingStatus")[0]).GetElementsByTagName("currentPrice")[0].InnerText);
                            prices.Add(price);
                        }

                        decimal high = 0;
                        foreach (decimal price in prices)
                        {
                            if (price > high)
                            {
                                high = price;
                            }
                        }

                        decimal low = 99999999;
                        foreach (decimal price in prices)
                        {
                            if (price < low)
                            {
                                low = price;
                            }
                        }

                        decimal avg = 0;
                        foreach (decimal price in prices)
                        {
                            avg += price;
                        }
                        avg = avg / prices.Count;

                        spreadsheetItem.HighPrice = high.ToString("C0");
                        spreadsheetItem.LowPrice = low.ToString("C0");
                        spreadsheetItem.AvgPrice = avg.ToString("C0");

                        items.Add(spreadsheetItem);
                        progressBar1.Value++;
                        label1.Text = progressBar1.Value + " items added";
                        Log.SelectedIndex = Log.Items.Count - 1;
                    }
                }
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".csv")
                {
                    string data = "Year,Set,CardNumber,Quality,Player,Quantity,HighPrice,AvgPrice,LowPrice,Sport,Index\n";
                    foreach (SpreadsheetItem item in items)
                    {
                        StringBuilder b = new StringBuilder(item.Player);
                        b.Replace(',', ' ');
                        item.Player = b.ToString();

                        data += item.Year + "," + item.CardNumber + "," + item.Quality + "," + item.Player + "," + item.Quantity + "," + item.HighPrice + "," + item.AvgPrice + "," + item.LowPrice + "," + item.Sport + "," + item.Index + "\n";
                    }

                    File.WriteAllText(saveFileDialog1.FileName, data);
                    Begin.Enabled = true;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            selectType.SelectedIndex = 0;
        }
    }
}