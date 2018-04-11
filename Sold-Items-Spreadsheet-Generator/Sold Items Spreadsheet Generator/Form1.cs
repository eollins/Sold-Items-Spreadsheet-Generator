using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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
                int listingId = 1;
                DatabaseLog.Items.Clear();
                List<string> groups = new List<string>();

                SqlConnection connection = new SqlConnection(Connection.ConnectionString);
                SqlCommand command = new SqlCommand("usp_clearListings");
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = connection;
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

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
                            WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&outputSelector=SellerInfo&listingType=Auction&keywords=" + (year + " " + set + " PSA") + "&categoryId=" + 213 + "&sortOrder=PricePlusShippingLowest&paginationInput.entriesPerPage=200&paginationInput.pageNumber=" + i);
                            WebResponse response = await request.GetResponseAsync();
                            string xml = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                            doc.LoadXml(xml);
                        }
                        catch
                        {
                            AddedLog.Items.Add("Request failed. Retrying.");
                            try
                            {
                                WebRequest request = WebRequest.Create("http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findCompletedItems&SERVICE-VERSION=1.7.0&SECURITY-APPNAME=GregoryM-mailer-PRD-a45ed6035-97c14545&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&outputSelector=SellerInfo&listingType=Auction&keywords=" + (year + " " + set + " PSA") + "&categoryId=" + 213 + "&sortOrder=PricePlusShippingLowest&paginationInput.entriesPerPage=200&paginationInput.pageNumber=" + i);
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

                            SearchResult result = new SearchResult();

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

                            double grade = 0;
                            string[] words = title.Split(' ');
                            int level = 0;
                            foreach (string str in words)
                            {
                                if (str == "psa")
                                {
                                    level++;
                                    continue;
                                }

                                if (level == 1)
                                {
                                    try
                                    {
                                        grade = double.Parse(str);
                                    }
                                    catch
                                    {
                                        grade = 0;
                                    }

                                    level++;
                                }

                                if (level == 2)
                                {
                                    if (str == "OC" || str == "MC" || str == "ST" || str == "MK")
                                    {
                                        found = false;
                                    }
                                }
                            }

                            int count4 = 0;
                            List<string> numbers = new List<string>();
                            foreach (string str in words)
                            {
                                if (str.Length > 0 && str[0] == '#')
                                {
                                    numbers.Add(str);
                                    count4++;
                                }
                            }

                            string cardnumber = "";
                            if (count4 == 1)
                            {
                                cardnumber = numbers[0];
                            }
                            else if (count4 > 1)
                            {
                                cardnumber = "Mult";
                            }
                            else
                            {
                                cardnumber = "N/F";
                            }

                            //MessageBox.Show((DateTime.Now - s).TotalSeconds.ToString());

                            string imageURL = "";

                            try
                            {
                                imageURL = ele.GetElementsByTagName("galleryPlusURL")[0].InnerText;
                            }
                            catch
                            {
                                imageURL = "Unavailable";
                            }

                            count32++;
                            //MessageBox.Show((start - DateTime.Now).TotalSeconds.ToString());
                            if (found)
                            {
                                result.Player = player;
                                result.Price = (((XmlElement)ele.GetElementsByTagName("sellingStatus")[0]).GetElementsByTagName("currentPrice")[0].InnerText);
                                result.Year = year;
                                result.Brand = set;
                                result.PSAGrade = grade.ToString();
                                result.CardNumber = cardnumber;
                                result.ImageURL = imageURL;

                                try
                                {
                                    result.Seller = ((XmlElement)ele.GetElementsByTagName("sellerInfo")[0]).GetElementsByTagName("sellerUserName")[0].InnerText;
                                }
                                catch (Exception ex)
                                {
                                    result.Seller = "Unavailable";
                                }

                                total++;

                                //year, brand, num, player, grade, seller, price

                                groupResults.Add(result);
                                AddedLog.Items.Add("Added " + player + ", " + total + ", " + count32);
                                label1.Text = total + " items added out of " + count32 + " results";
                                AddedLog.SelectedIndex = AddedLog.Items.Count - 1;

                                if (total > 12500)
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

                    itemsFound.Add(group, groupResults);
                    label1.Text = "Importing group \"" + group + "\" into database";

                    SqlCommand addCommand = new SqlCommand("usp_addListing");
                    addCommand.CommandType = CommandType.StoredProcedure;
                    addCommand.Connection = connection;
                    int succeeded1 = 0;
                    int attempts = 0;

                    foreach (SearchResult result in groupResults)
                    {
                        addCommand.Parameters.Clear();
                        addCommand.Parameters.Add(new SqlParameter("@Year", result.Year));
                        addCommand.Parameters.Add(new SqlParameter("@Brand", result.Brand));
                        addCommand.Parameters.Add(new SqlParameter("@CardNumber", result.CardNumber));
                        addCommand.Parameters.Add(new SqlParameter("@Player", result.Player));
                        addCommand.Parameters.Add(new SqlParameter("@Price", result.Price));
                        addCommand.Parameters.Add(new SqlParameter("@PSAGrade", result.PSAGrade));
                        addCommand.Parameters.Add(new SqlParameter("@Seller", result.Seller));
                        addCommand.Parameters.Add(new SqlParameter("@ListingID", listingId));
                        addCommand.Parameters.Add(new SqlParameter("@ImageUrl", result.ImageURL));
                        listingId++;

                        SqlDataAdapter adapter2 = new SqlDataAdapter(addCommand);
                        DataTable table2 = new DataTable();

                        try
                        {
                            adapter2.Fill(table2);
                        }
                        catch (Exception ex)
                        {
                            attempts++;
                            Debug.WriteLine(succeeded1 + " / " + attempts + " / " + (attempts - succeeded1) + " / " + groupResults.Count);
                            //progressBar1.Value++;
                            continue;
                        }

                        addCommand.Parameters.Clear();

                        attempts++;

                        if (table2.Rows.Count != 0)
                        {
                            succeeded1++;
                        }

                        //DatabaseLog.Items[DatabaseLog.Items.Count - 1] = "Imported " + attempts + "/" + succeeded1 + " items to database";
                        Debug.WriteLine(succeeded1 + " / " + attempts + " / " + (attempts - succeeded1) + " / " + groupResults.Count);
                    }

                    label1.Text = "Search eBay Databases";
                    DatabaseLog.Items.Add("Added " + succeeded1 + " out of " + attempts + " items in group " + group);
                    DatabaseLog.SelectedIndex = DatabaseLog.Items.Count - 1;
                    groups.Add(succeeded1 + ":" + attempts + ":" + group);
                }

                int succeeded = 0;
                int total3 = 0;
                foreach (string s in groups)
                {
                    string[] s2 = s.Split(':');
                    succeeded += int.Parse(s2[0]);
                    total3 += int.Parse(s2[1]);
                }
                MessageBox.Show("Out of " + total3 + " items, " + succeeded + " were sent to the database. Completed with " + callsPlaced + "calls.");

                Application.Exit();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}