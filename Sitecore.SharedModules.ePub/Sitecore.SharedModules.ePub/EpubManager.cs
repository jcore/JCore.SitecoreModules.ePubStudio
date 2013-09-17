using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Helpers;
using Sitecore.Mvc.Presentation;
using Sitecore.Presentation;
using Sitecore.SharedModules.ePub;
using Sitecore.SharedModules.ePub.Publishing;

namespace Sitecore.SharedModules.ePub
{
    /// <summary>
    /// 
    /// </summary>
    public class EpubManager
    {
        private static readonly Database Database = Factory.GetDatabase("master");
        private const string EpubDeviceId = "{CCA3E1C9-504C-447F-8B19-36EBC25A421C}";

        /// <summary>
        /// Publishes the project.
        /// </summary>
        /// <param name="projectId">The project unique identifier.</param>
        /// <returns></returns>
        public static string PublishProject(string projectId = null)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                projectId = "{B80EB639-5E3E-445C-BE73-B5CA3F4119BA}";
            }
            var printOptions = new PrintOptions()
            {
                ResultFileName = "testingDynamic"
            };
            var manager = new PrintManager(Database, Sitecore.Context.Language);
            return manager.Print(projectId, printOptions);
        }

        public static string CreateBiography(string id)
        {
            var projectId = "{B80EB639-5E3E-445C-BE73-B5CA3F4119BA}";
            var projectItem = Database.GetItem(projectId);
            try
            {
                if (projectItem != null)
                {
                    var epubDoc = new Epub.Document();

                    var contentItem = ((ReferenceField)projectItem.Fields["Item Reference"]).InnerField.Item;

                    //Metadata
                    epubDoc.AddAuthor("Weil - Julia Gavrilova");
                    epubDoc.AddTitle("Testing ePub book Generation");
                    epubDoc.AddLanguage("en");

                    //embeded fonts
                    epubDoc.AddFile("D:\\siteroot\\local7cms.weil.com\\Website\\Content\\fontawesome-webfont.ttf", "fonts/fontawesome-webfont.ttf", "application/octet-stream");

                    //add stylesheet with @font-face
                    epubDoc.AddStylesheetFile("D:\\siteroot\\local7cms.weil.com\\Website\\Content\\styles\\screen.css", "screen.css");

                    // Add image files (figures)
                    epubDoc.AddImageFile("D:\\siteroot\\local7cms.weil.com\\Website\\Content\\images\\office-map.png", "office-map.png");
                    epubDoc.AddImageFile("D:\\siteroot\\local7cms.weil.com\\Website\\Content\\images\\icon-select.png", "icon-select.png");

                    // add chapters' xhtml and setup TOC entries
                    int navCounter = 1;
                    for (int chapterCounter = 1; chapterCounter < 10; chapterCounter++)
                    {
                        String chapterFile = String.Format("page{0}.xhtml", chapterCounter);
                        String chapterName = String.Format("Chapter {0}", chapterCounter);
                        epubDoc.AddXhtmlData("tempdir/" + chapterFile, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"DTD/xhtml1-strict.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head><title> Strict DTD XHTML Example </title></head><html><body>this is content for the chapter</body></html>");
                        var chapterTOCEntry =
                            epubDoc.AddNavPoint(chapterName, chapterFile, navCounter++);
                        // add nested TOC entries
                        for (int part = 0; part < 3; part++)
                        {
                            String partName = String.Format("Part {0}", part);
                            String partHref = chapterFile + String.Format("#{0}", part);
                            chapterTOCEntry.AddNavPoint(partName, partHref, navCounter++);
                        }
                    }

                    // Generate resulting epub file
                    //epubDoc.Generate(fileOutputLocation);    
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, typeof(EpubManager));
            }
            return projectId;
        }
        /// <summary>
        /// Creates the html.
        /// </summary>
        /// <returns></returns>
        public static string CreateDocument(Controller controller)
        {
            var fileOutputLocation = @"\\\\JCSHP10DEV14\\Public\\ePubBooks\\testEPubBook.epub";

            var projectId = "{B80EB639-5E3E-445C-BE73-B5CA3F4119BA}";
            var projectItem = Database.GetItem(projectId);
            try
            {
                if (projectItem != null)
                {
                    var documentItem = projectItem.Children.FirstOrDefault(itm => itm.TemplateID.Equals(ID.Parse("{E3261481-32CE-4D68-B5D1-79E7CB9C1CB9}")));
                    if (documentItem != null)
                    {
                        var epubDoc = new Epub.Document();

                        var contentItem = ((ReferenceField)documentItem.Fields["Item Reference"]).TargetItem;

                        //Metadata
                        epubDoc.AddAuthor(documentItem["Author"]);
                        epubDoc.AddTitle(documentItem["Title"]);
                        epubDoc.AddLanguage(documentItem["Language"]);

                        //embeded fonts
                        foreach (var fontId in ((MultilistField)documentItem.Fields["Fonts"]).TargetIDs)
                        {
                            var font = Database.GetItem(fontId);
                            if (font != null)
                            {
                                epubDoc.AddFile(font["Path Base"] + font["File Name"], font["ePub File Base"] + font["ePub File Name"], "application/octet-stream");
                            }
                        }

                        //add stylesheet with @font-face
                        foreach (var cssId in ((MultilistField)documentItem.Fields["StyleSheets"]).TargetIDs)
                        {
                            var css = Database.GetItem(cssId);
                            if (css != null)
                            {
                                epubDoc.AddStylesheetFile(css["Path Base"] + css["File Name"], css["ePub File Base"] + css["ePub File Name"]);
                            }
                        }

                        // Add image files (figures)
                        //epubDoc.AddImageFile("D:\\siteroot\\local7cms.weil.com\\Website\\Content\\images\\office-map.png", "office-map.png");
                        //epubDoc.AddImageFile("D:\\siteroot\\local7cms.weil.com\\Website\\Content\\images\\icon-select.png", "icon-select.png");

                        // add chapters' xhtml and setup TOC entries
                        int navCounter = 0;
                        for (int chapterCounter = 0; chapterCounter < documentItem.Children.Count; chapterCounter++)
                        {
                            var chapter = documentItem.Children[chapterCounter];
                            String chapterFile = String.Format("{0}.xhtml", chapter["Title"]);
                            String chapterName = chapter["Title"];

                            var xpath = chapter["Item Selector"];
                            if (!string.IsNullOrEmpty(xpath))
                            {
                                Item selectorDataItem = contentItem.Axes.SelectSingleItem(xpath);
                                if (selectorDataItem != null)
                                {
                                    epubDoc.AddXhtmlData(chapterFile, GenerateXhtml(selectorDataItem, controller));
                                }
                            }
                            else
                            {
                                epubDoc.AddXhtmlData(chapterFile, GenerateXhtml(contentItem, controller));
                            }

                            var chapterTOCEntry =
                                epubDoc.AddNavPoint(chapterName, chapterFile, navCounter++);
                            //// add nested TOC entries
                            //for (int part = 0; part < 3; part++)
                            //{
                            //    String partName = String.Format("Part {0}", part);
                            //    String partHref = chapterFile + String.Format("#{0}", part);
                            //    chapterTOCEntry.AddNavPoint(partName, partHref, navCounter++);
                            //}
                        }

                        // Generate resulting epub file
                        epubDoc.Generate(fileOutputLocation);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, typeof(EpubManager));
            }
            return projectId;
        }

        #region Private
        /// <summary>
        /// Generates the XHTML.
        /// </summary>
        /// <param name="selectorDataItem">The selector data item.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private static string GenerateXhtml(Item item, Controller controller)
        {
            if (item == null)
            {
                return string.Empty;
            }

            

            return string.Empty;
        }

        
        #endregion
    }
}
