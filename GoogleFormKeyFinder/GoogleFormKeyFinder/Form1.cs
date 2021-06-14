using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using RestSharp;
namespace GoogleFormKeyFinder
{
    public partial class Form1 : Form
    {
        public String url = "";
        public String responseURL = "";
        public Form1()
        {
            
            InitializeComponent();
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        public void button1_Click(object sender, EventArgs e)
        {
            if (groupBox1.Text.Length > 1)
            {
                groupBox1.Text = "";
                richTextBox1.Text = "";
            }

            url = textBox1.Text;
            
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("download.default_directory", AppDomain.CurrentDomain.BaseDirectory);
            chromeOptions.AddArguments(new List<string>() {
                "--silent-launch",
                "--no-startup-window",
                "no-sandbox",
               "headless",

            });
            var chromeDriverService = ChromeDriverService.CreateDefaultService(Environment.CurrentDirectory);
            chromeDriverService.HideCommandPromptWindow = true;   // This is to hidden the console.
            
            ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions);
            
            try
            {
                driver.Navigate().GoToUrl(url);

            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                MessageBox.Show("Please enter a valid URL");
            }
            try
            {
                
                
                    var keyDiv = driver.FindElement(By.XPath("/html/body/div/div[2]/form/div[1]/div"));
                    var keys = keyDiv.FindElements(By.XPath(".//*[@type='hidden']"));
                    foreach (var key in keys)
                    {

                        groupBox1.Text += key.GetAttribute("name");
                        groupBox1.Text += "=";
                        groupBox1.Text += "\n";
                    }
                
            }
            catch (Exception)
            {
                MessageBox.Show("Form Keys not found on site");
            }
            
            driver.Close();
            driver.Quit();
           
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
        public void button2_Click(object sender, EventArgs e)
        {
            int slashIndex = url.LastIndexOf("/");
            responseURL = url.Substring(0, slashIndex) + "/formResponse";
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Data Sources(*.ini) | *.ini *| All Files | *.*";
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    File.AppendAllText(dialog.FileName, "[FormKeys]\n");
                    File.AppendAllText(dialog.FileName, groupBox1.Text);
                    File.AppendAllText(dialog.FileName, "[FormLink]\n");
                    File.AppendAllText(dialog.FileName, responseURL);
                }
            }
        }

        public void button3_Click(object sender, EventArgs e)
        {
            try
            {
                int slashIndex = url.LastIndexOf("/");
                responseURL = url.Substring(0, slashIndex) + "/formResponse";
                String text = groupBox1.Text.Replace("\n", "");
                var client = new RestClient(responseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                String[] formKeys = text.Split('=');
                DateTime date = DateTime.Now;
                String bodString = $"$body = ";
                for (int index = 0; index < formKeys.Length-1; index++)
                {
                    if (index == 0)
                    {
                        request.AddParameter(formKeys[index], date.Year.ToString());
                        bodString += $"'{formKeys[index]}={date.Year.ToString()}&";
                    }
                    else if (index == 1)
                    {
                        request.AddParameter(formKeys[index], date.Month.ToString());
                        bodString += $"{formKeys[index]}={date.Month.ToString()}&";
                    }
                    else if (index == 2)
                    {
                        request.AddParameter(formKeys[index], date.Day.ToString());
                        bodString += $"{formKeys[index]}={date.Day.ToString()}&";
                    }
                    else if (index == formKeys.Length-2)
                    {
                        request.AddParameter(formKeys[index], "Testing");
                        bodString += $"{formKeys[index]}=Testing'";
                    }
                    else
                    {
                        request.AddParameter(formKeys[index], "Testing");
                        bodString += $"{formKeys[index]}=Testing&";
                    }
                }
                IRestResponse response = client.Execute(request);
                MessageBox.Show("POST Request Successful");
                String headerString = "$headers = New-Object 'System.Collections.Generic.Dictionary[[String],[String]]';";
                String headerAdd = "$headers.Add('Content-Type', 'application/x-www-form-urlencoded');";
                String responseString = $"$response = Invoke-RestMethod '{responseURL}' -Method 'POST' -Headers $headers -Body $body";

                richTextBox1.Text = $"{headerString}\n\n{headerAdd}\n\n{bodString}\n\n{responseString}";
            }
            catch(Exception ex) { 
                MessageBox.Show("POST Request Failure");
                MessageBox.Show(ex.Message.ToString());
                
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
