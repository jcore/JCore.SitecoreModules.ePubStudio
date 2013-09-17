using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Security.Accounts;

namespace Sitecore.SharedModules.ePub.Publishing
{
    public class PrintContext
    {
        private List<ID> ancestors = new List<ID>();
        private User user;
        private Language language;
        private Database database;

        public Epub.Document EpubDocument { get; set; }
        public int PageNumber { get; set; }
        public IDictionary<string, Stream> Images { get; set; }
        public IDictionary<string, string> StyleSheets { get; set; }
        
        public User User
        {
            get
            {
                return this.user ?? (this.user = Context.User);
            }
            set
            {
                this.user = value;
            }
        }

        public Database Database
        {
            get
            {
                return this.database ?? (this.database = Context.Database);
            }
            set
            {
                this.database = value;
            }
        }

        public Language Language
        {
            get
            {
                return this.language ?? (this.language = Context.Language);
            }
            set
            {
                this.language = value;
            }
        }

        public PrintOptions Settings { get; private set; }

        public List<ID> CurrentItemAncestors
        {
            get
            {
                return this.ancestors;
            }
            set
            {
                this.ancestors = value;
            }
        }

        public HtmlDocument PageContainer { get; set; }

        public Epub.Document DocumentContainer { get; set; }
        
        public PrintContext(Item publishItem, PrintOptions settings)
        {
            this.StartItem = publishItem;
            this.Settings = settings;
        }

        public Item StartItem { get; set; }
    }
}
