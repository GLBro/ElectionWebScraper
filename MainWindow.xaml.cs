using System.Collections;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using HtmlAgilityPack;

namespace ElectionWebScraper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }


    private void SearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (EnterBox.GetLineText(0).ToLower().Equals("all"))
        {
            grid.Background = new SolidColorBrush(Colors.Red);
            UpdateCandidates(new String[]{"Keir Starmer", "Rishi Sunak", "Nigel Farage", "Ed Davey", "Carla Denyer/Adrian Ramsay", "John Swinney"});
            UpdateParty(new String[]{"Labour", "Conservative", "Reform UK", "Liberal Democrats", "Green", "Scottish National Party"});
            UpdateNumber(new String[]{"9,704,655", "6,827,311", "4,117,221", "3,519,199", "1,943,265", "724,758"});
            UpdatePercentage(new String[]{"33.7", "23.7", "14.3", "12.2", "6.7", "2.5"});
        }
        else
        {
            String url = "https://en.wikipedia.org/wiki/" + FixInput(EnterBox.GetLineText(0)) +
                         "(UK_Parliament_constituency)";
            try
            {
                HtmlDocument html = GetWebInfo(url);
                UpdateCandidates(GetCandidates(html));
                UpdateParty(GetParties(html));
                ChangeBackGroundColor(html);
                UpdateNumber(GetNumbers(html));
                UpdatePercentage(GetPercentages(html));
            }
            catch (Exception exception)
            {
                InvalidText.Text = "Invalid Constituency";
            }
        }
    }

    private void ChangeBackGroundColor(HtmlDocument htmlDocument)
    {
        String style = htmlDocument.DocumentNode.SelectSingleNode("//tr[@class='vcard']/td[1]").GetAttributeValue("style", null);
        String[] temp = style.Split(";");
        style = "#000000";
        foreach (String s in temp)
        {
            if (s.Contains("background"))
            {
                style = s;
            }
        }
        style = style.Replace("background-color:", "").Trim();
        Byte red = Convert.ToByte(style.Substring(1, 2), 16);
        Byte green = Convert.ToByte(style.Substring(3, 2), 16);
        Byte blue = Convert.ToByte(style.Substring(5, 2), 16);
        grid.Background = new SolidColorBrush(Color.FromRgb(red, green, blue));
    }

    private void UpdateCandidates(String[] candidates)
    {
        List<TextBlock> blocks = new List<TextBlock>() { Can1, Can2, Can3, Can4, Can5, Can6 };
        for (int i = 0; i < candidates.Length; i++)
        {
            blocks[i].Text = candidates[i];
        }
    }
    
    private void UpdateParty(String[] parties)
    {
        List<TextBlock> blocks = new List<TextBlock>() { Par1, Par2, Par3, Par4, Par5, Par6 };
        for (int i = 0; i < parties.Length; i++)
        {
            blocks[i].Text = parties[i];
        }
    }
    
    private void UpdateNumber(String[] numbers)
    {
        List<TextBlock> blocks = new List<TextBlock>() { Num1, Num2, Num3, Num4, Num5, Num6 };
        for (int i = 0; i < numbers.Length; i++)
        {
            blocks[i].Text = numbers[i];
        }
    }
    
    private void UpdatePercentage(String[] percentages)
    {
        List<TextBlock> blocks = new List<TextBlock>() {Per1, Per2, Per3, Per4, Per5, Per6};
        for (int i = 0; i < percentages.Length; i++)
        {
            blocks[i].Text = percentages[i]+"%";
            if (blocks[i].Text.Equals("%"))
            {
                blocks[i].Text = "";
            }
        }
    }

    private HtmlDocument GetWebInfo(String url)
    {
        HttpClient client = new HttpClient();
        var html = client.GetStringAsync(url).Result;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        InvalidText.Text = "";
        return doc;
    }
    

    private String FixInput(String text)
    {
        text = text.ToLower();
        text = text.Trim();
        String[] texts = text.Split(" ");
        String ans = "";
        for (int i = 0; i < texts.Length; i++)
        {
            if (!texts[i].Equals("and"))
            {
                texts[i] = texts[i].Substring(0, 1).ToUpper() + texts[i].Substring(1);
            }

            ans += texts[i] + "_";
        }
        return ans;
    }

    private String[] GetCandidates(HtmlDocument htmlDocument)
    {
        String[] ans = new String[6];
        try
        {
            for (int i = 2; i < 8; i++)
            {
                ans[i - 2] = htmlDocument.DocumentNode
                    .SelectSingleNode("//table[@class='wikitable plainrowheaders'][1]/tbody/tr[" + i + "][@class='vcard']/td[@class='fn']").InnerText;
                ans[i - 2] = ans[i - 2].Replace("\n", "");
                if (ans[i - 2].Contains("&"))
                {
                    String[] temp = ans[i - 2].Split("&");
                    ans[i - 2] = temp[0];
                }
            }
        }
        catch (NullReferenceException nullReferenceException)
        {
            return ans;
        }

        return ans;
    }
    
    private String[] GetParties(HtmlDocument htmlDocument)
    {
        String[] ans = new String[6];
        try
        {
            for (int i = 0; i < 6; i++)
            {
                ans[i] = htmlDocument.DocumentNode
                    .SelectSingleNode("//table[@class='wikitable plainrowheaders'][1]/tbody/tr[" + ((i) + 2) +
                                      "][@class='vcard']/td[@class='org']").InnerText;
                ans[i] = ans[i].Replace("\n", "");
                if (ans[i].Contains("&"))
                {
                    String[] temp = ans[i].Split("&");
                    ans[i] = temp[0];
                }
            }
        }
        catch (NullReferenceException nullReferenceException)
        {
            return ans;
        }

        return ans;
    }

    private String[] GetNumbers(HtmlDocument htmlDocument)
    {
        String[] ans = new String[6];
        try
        {
            for (int i = 0; i < 6; i++)
            {
                ans[i] = htmlDocument.DocumentNode
                    .SelectSingleNode("//table[@class='wikitable plainrowheaders'][1]/tbody/tr[" + ((i) + 2) +
                                      "][@class='vcard']").InnerText;
                ans[i] = ans[i].Trim();
                String[] temp = ans[i].Split("\n");
                ans[i] = temp[4].Trim();
            }
        }
        catch (NullReferenceException nullReferenceException)
        {
            return ans;
        }

        return ans;
    } 
    
    private String[] GetPercentages(HtmlDocument htmlDocument)
    {
        String[] ans = new String[6];
        try
        {
            for (int i = 0; i < 6; i++)
            {
                ans[i] = htmlDocument.DocumentNode
                    .SelectSingleNode("//table[@class='wikitable plainrowheaders'][1]/tbody/tr[" + ((i) + 2) +
                                      "][@class='vcard']").InnerText;
                ans[i] = ans[i].Trim();
                String[] temp = ans[i].Split("\n");
                ans[i] = temp[6].Trim();
            }
        }
        catch (NullReferenceException nullReferenceException)
        {
            return ans;
        }

        return ans;
    } 
}