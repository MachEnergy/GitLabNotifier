using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO.Packaging;

namespace GitLabNotifier
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    public MainWindow()
    {
      InitializeComponent();

      ListViewFeed.ItemsSource = new List<object>();

      foreach (FeedType ftype in Enum.GetValues(typeof(FeedType)))
      {
        ComboBoxFeedTypes.Items.Add(ftype);
      }

      m_Settings.Reload();

      if (string.IsNullOrWhiteSpace(m_Settings.UserFeedType))
      {
        m_Settings.UserFeedType = m_Settings.DefaultFeedType;
      }

      if (string.IsNullOrWhiteSpace(m_Settings.UserUrl))
      {
        m_Settings.UserUrl = m_Settings.DefaultUrl;
      }

      m_Settings.Save();

      CurrentFeedType = (FeedType)Enum.Parse(typeof(FeedType), m_Settings.UserFeedType);
      CurrentUrl = m_Settings.UserUrl;

      bool parseAtStart = true;
      if (parseAtStart && !string.IsNullOrWhiteSpace(CurrentUrl) && CurrentFeedType != null)
      {
        StartParseTimer();
      }
    }

    AppSettings m_Settings = new AppSettings();

    Timer m_Timer;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public FeedType CurrentFeedType
    {
      get { return m_CurrentFeedType; }
      set { m_CurrentFeedType = value; OnPropertyChanged(); }
    }
    private FeedType m_CurrentFeedType = FeedType.NOT_SET;

    public string CurrentUrl
    {
      get { return m_CurrentUrl; }
      set { m_CurrentUrl = value; OnPropertyChanged(); }
    }
    private string m_CurrentUrl;

    private void ButtonParse_Click(object sender, RoutedEventArgs e)
    {
      StartParseTimer();
    }

    private void StartParseTimer()
    {
      m_Timer?.Dispose();
      m_Timer = new Timer(new TimerCallback(ParseFeedThreadSafe), null, 0, 5000);
    }

    private void ParseFeedThreadSafe(object state)
    {
      Dispatcher.Invoke(() => { ParseFeed(state); });
    }

    private void ParseFeed(object state)
    {
      IList<string> errors = new List<string>();
      IList<Item> parsedItems = new List<Item>();

      switch (CurrentFeedType)
      {
        case FeedType.RSS:
          parsedItems = FeedParser.ParseRss(TextBoxUrl.Text, ref errors);
          break;
        case FeedType.RDF:
          parsedItems = FeedParser.ParseRdf(TextBoxUrl.Text, ref errors);
          break;
        case FeedType.Atom:
          parsedItems = FeedParser.ParseAtom(TextBoxUrl.Text, ref errors);
          break;
        case FeedType.NOT_SET:
          errors.Add("CurrentFeedType is NOT_SET");
          break;
      }

      if (!CompareLists(ListViewFeed.ItemsSource, parsedItems, ref errors))
      {
        if (errors.Count == 0)
        {
          ListViewFeed.ItemsSource = parsedItems;
          NotifyFeedUpdated();
        }
      }

      if (errors.Count > 0)
      {
        MessageBox.Show(errors.ToString(), $"({errors.Count}) Errors During Parse");
      }
    }

    [DllImport("user32")] static extern int FlashWindow(IntPtr hwnd, bool bInvert);
    private void NotifyFeedUpdated()
    {
      WindowInteropHelper wih = new WindowInteropHelper(this);
      FlashWindow(wih.Handle, true);
    }

    private bool CompareLists(IEnumerable itemsSource, IList<Item> parsedItems, ref IList<string> errors)
    {
      if (itemsSource == null || parsedItems == null)
      {
        errors.Add("Input parameter was null");
        return false;
      }

      List<Item> itemsSourceList = new List<Item>(itemsSource.Cast<Item>());
      if (itemsSourceList.Count != parsedItems.Count)
      {
        return false;
      }

      for (int i = 0; i < itemsSourceList.Count; i++)
      {
        if ((itemsSourceList[i].Content != parsedItems[i].Content) ||
            (itemsSourceList[i].FeedType != parsedItems[i].FeedType) ||
            (itemsSourceList[i].Link != parsedItems[i].Link) ||
            (itemsSourceList[i].PublishDate != parsedItems[i].PublishDate) ||
            (itemsSourceList[i].Title != parsedItems[i].Title)
            )
        {
          return false;
        }
      }

      return true;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      m_Settings.UserFeedType = CurrentFeedType.ToString();
      m_Settings.UserUrl = CurrentUrl;
      m_Settings.Save();
    }

    private void ItemsPresenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      string url = ((Item)((Grid)sender).DataContext).Link;
      Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
    }

    private void BrowserContent_Initialized(object sender, EventArgs e)
    {
      WebBrowser browser = sender as WebBrowser;
      browser?.NavigateToString((browser.DataContext as Item).Content);
    }
  }
}