using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }

    AppSettings m_Settings = new AppSettings();

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
    private FeedType m_CurrentFeedType;

    public string CurrentUrl
    {
      get { return m_CurrentUrl; }
      set { m_CurrentUrl = value; OnPropertyChanged(); }
    }
    private string m_CurrentUrl;

    private void ButtonParse_Click(object sender, RoutedEventArgs e)
    {
      ParseFeed();
    }

    private void ParseFeed()
    {
      IList<string> errors = new List<string>();

      switch (CurrentFeedType)
      {
        case FeedType.RSS:
          ListViewFeed.ItemsSource = FeedParser.ParseRss(TextBoxUrl.Text, ref errors);
          break;
        case FeedType.RDF:
          ListViewFeed.ItemsSource = FeedParser.ParseRdf(TextBoxUrl.Text, ref errors);
          break;
        case FeedType.Atom:
          ListViewFeed.ItemsSource = FeedParser.ParseAtom(TextBoxUrl.Text, ref errors);
          break;
      }

      if (errors.Count > 0)
      {
        MessageBox.Show(errors.ToString(), $"({errors.Count}) Errors During Parse");
      }
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
  }
}
