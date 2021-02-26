using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GitLabNotifier
{
  /// <summary>
  /// Represents a feed item.
  /// </summary>
  public class Item
  {
    public string Link { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishDate { get; set; }
    public FeedType FeedType { get; set; }

    public Item()
    {
      Link = "";
      Title = "";
      Content = "";
      PublishDate = DateTime.Today;
      FeedType = FeedType.Atom;
    }
  }

  /// <summary>
  /// A simple RSS, RDF and ATOM feed parser.
  /// </summary>
  static public class FeedParser
  {
    static public XDocument LastParsedFeed = new XDocument();

    /// <summary>
    /// Parses an Atom feed and returns a <see cref="IList&amp;lt;Item&amp;gt;"/>.
    /// </summary>
    static public IList<Item> ParseAtom(string url, [Optional] ref IList<string> errors)
    {
      try
      {
        XDocument doc = XDocument.Load(url);
        // Feed/Entry

        var test_entries = doc.Root.Elements().Where(i => i.Name.LocalName == "entry");
        List<Item> test_entry_list = new List<Item>();

        foreach (var test_entry in test_entries)
        {
          Item newItem = new Item();

          newItem.FeedType = FeedType.Atom;

          try
          {
            newItem.Content = test_entry.Elements().First(i => i.Name.LocalName == "content")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.Link = test_entry.Elements().First(i => i.Name.LocalName == "link").Attribute("href")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.PublishDate = ParseDate(test_entry.Elements().First(i => i.Name.LocalName == "updated")?.Value);
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.Title = test_entry.Elements().First(i => i.Name.LocalName == "title")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          test_entry_list.Add(newItem);
        }

        return test_entry_list;
      }
      catch (Exception ex)
      {
        errors.Append(ex.Message);
        return new List<Item>();
      }
    }

    /// <summary>
    /// Parses an RSS feed and returns a <see cref="IList&amp;lt;Item&amp;gt;"/>.
    /// </summary>
    static public IList<Item> ParseRss(string url, [Optional] ref IList<string> errors)
    {
      try
      {
        XDocument doc = XDocument.Load(url);
        // RSS/Channel/item

        var test_channels = doc.Root.Descendants().Where(i => i.Name.LocalName == "channel");
        var test_items = test_channels.Descendants().Where(i => i.Name.LocalName == "item");
        List<Item> test_item_list = new List<Item>();

        foreach (var test_item in test_items)
        {
          Item newItem = new Item();

          newItem.FeedType = FeedType.RSS;

          try
          {
            newItem.Content = test_item.Elements().First(i => i.Name.LocalName == "description")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.Link = test_item.Elements().First(i => i.Name.LocalName == "link")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.PublishDate = ParseDate(test_item.Elements().First(i => i.Name.LocalName == "pubDate")?.Value);
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.Title = test_item.Elements().First(i => i.Name.LocalName == "title")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          test_item_list.Add(newItem);
        }

        return test_item_list;
      }
      catch (Exception ex)
      {
        errors.Append(ex.Message);
        return new List<Item>();
      }
    }

    /// <summary>
    /// Parses an RDF feed and returns a <see cref="IList&amp;lt;Item&amp;gt;"/>.
    /// </summary>
    static public IList<Item> ParseRdf(string url, [Optional] ref IList<string>errors)
    {
      try
      {
        XDocument doc = XDocument.Load(url);
        // <item> is under the root

        var test_items = doc.Root.Descendants().Where(i => i.Name.LocalName == "item");
        List<Item> test_item_list = new List<Item>();

        foreach (var test_item in test_items)
        {
          Item newItem = new Item();

          newItem.FeedType = FeedType.RDF;

          try
          {
            newItem.Content = test_item.Elements().First(i => i.Name.LocalName == "description")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.Link = test_item.Elements().First(i => i.Name.LocalName == "link")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.PublishDate = ParseDate(test_item.Elements().First(i => i.Name.LocalName == "date")?.Value);
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          try
          {
            newItem.Title = test_item.Elements().First(i => i.Name.LocalName == "title")?.Value;
          }
          catch (Exception ex)
          {
            errors.Append(ex.Message);
          }

          test_item_list.Add(newItem);
        }        

        return test_item_list;
      }
      catch (Exception ex)
      {
        errors.Append(ex.Message);
        return new List<Item>();
      }
    }

    static private DateTime ParseDate(string date)
    {
      DateTime result;
      if (DateTime.TryParse(date, out result))
        return result;
      else
        return DateTime.MinValue;
    }

    static private bool CompareFeeds(XDocument doc1, XDocument doc2)
    {
      return doc1 != doc2;
    }
  }
  /// <summary>
  /// Represents the XML format of a feed.
  /// </summary>
  public enum FeedType
  {
    NOT_SET,
    /// <summary>
    /// Really Simple Syndication format.
    /// </summary>
    RSS,
    /// <summary>
    /// RDF site summary format.
    /// </summary>
    RDF,
    /// <summary>
    /// Atom Syndication format.
    /// </summary>
    Atom
  }
}
