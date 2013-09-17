using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Resources.Media;
using Sitecore.Security.Accounts;
using Sitecore.SecurityModel;
using Sitecore.Workflows;
using Sitecore.SharedModules.ePub.Configuration;

namespace Sitecore.SharedModules.ePub
{
    public class SitecoreHelper
    {
        /// <summary>
        /// Scans the language versions.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="currentItem">The current item.</param>
        /// <param name="languageIndex">Index of the language.</param>
        /// <param name="currentDataBase">The current data base.</param>
        public static void ScanLanguageVersions(string username, Sitecore.Data.Items.Item currentItem, int languageIndex, Database currentDataBase)
        {
            string str = string.Empty;
            LanguageCollection languages = LanguageManager.GetLanguages(currentDataBase);
            using (new LanguageSwitcher(languages[languageIndex]))
            {
                try
                {
                    if (currentItem.Template != null)
                        str = currentItem.TemplateName;
                }
                catch (Exception ex)
                {
                    Log.Error("ScanLanguageVersions", ex, typeof(SitecoreHelper));
                    return;
                }
                if (str != string.Empty)
                    SitecoreHelper.CheckThisItemVersion(currentItem, languages, username, languageIndex, currentDataBase);
                foreach (Sitecore.Data.Items.Item currentItem1 in currentItem.Children)
                    SitecoreHelper.ScanLanguageVersions(username, currentItem1, languageIndex, currentDataBase);
            }
        }

        /// <summary>
        /// Adds the version.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="itemID">The item ID.</param>
        /// <param name="languageIndex">Index of the language.</param>
        /// <param name="currentDatabase">The current database.</param>
        /// <returns></returns>
        public static string AddVersion(string username, string itemID, int languageIndex, Database currentDatabase)
        {
            string str = string.Empty;
            try
            {
                if ((Account)SitecoreHelper.GetApiUser(username) != (Account)null)
                {
                    using (new SecurityDisabler())
                    {
                        using (new LanguageSwitcher(LanguageManager.GetLanguages(currentDatabase)[languageIndex]))
                        {
                            Sitecore.Data.Items.Item obj1 = currentDatabase.GetItem(itemID);
                            Sitecore.Data.Items.Item[] versions = obj1.Versions.GetVersions(true);
                            Sitecore.Data.Items.Item obj2 = Enumerable.FirstOrDefault<Sitecore.Data.Items.Item>((IEnumerable<Sitecore.Data.Items.Item>)versions, (Func<Sitecore.Data.Items.Item, bool>)(ver => ver.Language == LanguageManager.DefaultLanguage));
                            if (obj2 == null && versions.Length > 0)
                                obj2 = versions[0];
                            obj1.Editing.BeginEdit();
                            obj1.Versions.AddVersion();
                            if (obj2 != null)
                            {
                                Sitecore.Data.Items.Item latestVersion = obj1.Versions.GetLatestVersion();
                                latestVersion.Editing.BeginEdit();
                                foreach (TemplateFieldItem templateFieldItem in obj1.Template.Fields)
                                {
                                    if (!templateFieldItem.IsShared)
                                        latestVersion.Fields[templateFieldItem.Name].Value = obj2.Fields[templateFieldItem.Name].Value;
                                }
                                latestVersion.Editing.EndEdit();
                            }
                            obj1.Editing.EndEdit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Add Version", ex, typeof(SitecoreHelper));
            }
            return str;
        }

        /// <summary>
        /// Lookups the html.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        /// <param name="documentItem">The html item.</param>
        public static void LookupDocument(Sitecore.Data.Items.Item currentItem, ref Sitecore.Data.Items.Item documentItem)
        {
            if (currentItem.Template == null)
                return;
            documentItem = currentItem;
            if (currentItem.Template.Name == "ePub_Document")
                return;
            SitecoreHelper.LookupDocument(currentItem.Parent, ref documentItem);
        }

        /// <summary>
        /// Lookups the project.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        /// <param name="projectItem">The project item.</param>
        public static void LookupProject(Sitecore.Data.Items.Item currentItem, ref Sitecore.Data.Items.Item projectItem)
        {
            if (currentItem.Template == null)
                return;
            projectItem = currentItem;
            if (currentItem.TemplateName == "ePub_Project")
                return;
            SitecoreHelper.LookupProject(currentItem.Parent, ref projectItem);
        }

        /// <summary>
        /// Lookups the project.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="languageIndex">Index of the language.</param>
        /// <param name="currentDatabase">The current database.</param>
        /// <returns></returns>
        public static string LookupProject(string id, int languageIndex, Database currentDatabase)
        {
            string str = string.Empty;
            if (currentDatabase != null)
            {
                using (new LanguageSwitcher(LanguageManager.GetLanguages(currentDatabase)[languageIndex]))
                {
                    Sitecore.Data.Items.Item currentItem = currentDatabase.GetItem(id);
                    Sitecore.Data.Items.Item projectItem = (Sitecore.Data.Items.Item)null;
                    if (currentItem != null)
                    {
                        SitecoreHelper.LookupProject(currentItem, ref projectItem);
                        if (projectItem != null)
                            str = projectItem.ID.ToString();
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// Lookups the project.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="languageIndex">Index of the language.</param>
        /// <returns></returns>
        public static string LookupProject(string id, int languageIndex)
        {
            Database database = Factory.GetDatabase(ConfigHandler.CommonSettings.Database);
            return SitecoreHelper.LookupProject(id, languageIndex, database);
        }

        /// <summary>
        /// Lookups the project.
        /// </summary>
        /// <param name="startItem">The start item.</param>
        /// <param name="ancestorsList">The ancestors list.</param>
        /// <returns></returns>
        public static Sitecore.Data.Items.Item LookupProject(Sitecore.Data.Items.Item startItem, out List<ID> ancestorsList)
        {
            ancestorsList = new List<ID>();
            Sitecore.Data.Items.Item[] ancestors = startItem.Axes.GetAncestors();
            Sitecore.Data.Items.Item obj = Enumerable.FirstOrDefault<Sitecore.Data.Items.Item>((IEnumerable<Sitecore.Data.Items.Item>)ancestors, (Func<Sitecore.Data.Items.Item, bool>)(t => t.TemplateName == "P_Project"));
            if (obj == null)
                return startItem;
            if (Enumerable.Count<Sitecore.Data.Items.Item>((IEnumerable<Sitecore.Data.Items.Item>)ancestors) > 0)
            {
                IEnumerable<ID> collection = Enumerable.Select<Sitecore.Data.Items.Item, ID>(Enumerable.TakeWhile<Sitecore.Data.Items.Item>(Enumerable.Reverse<Sitecore.Data.Items.Item>((IEnumerable<Sitecore.Data.Items.Item>)ancestors), (Func<Sitecore.Data.Items.Item, bool>)(t => t.TemplateName != "P_Project")), (Func<Sitecore.Data.Items.Item, ID>)(t => t.ID));
                ancestorsList.Add(startItem.ID);
                ancestorsList.AddRange(collection);
            }
            return obj;
        }

        /// <summary>
        /// Lookups the path to parent.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        /// <param name="projectItem">The project item.</param>
        /// <param name="itemIds">The item ids.</param>
        public static void LookupPathToParent(Sitecore.Data.Items.Item currentItem, ref Sitecore.Data.Items.Item projectItem, ref List<ID> itemIds)
        {
            if (currentItem == null || currentItem.Template == null)
                return;
            projectItem = currentItem;
            itemIds.Add(projectItem.ID);
            if (currentItem.TemplateName == "ePub_Project")
                return;
            SitecoreHelper.LookupPathToParent(currentItem.Parent, ref projectItem, ref itemIds);
        }

        /// <summary>
        /// Lookups the path to parent.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        /// <param name="projectItem">The project item.</param>
        /// <param name="itemIds">The item ids.</param>
        /// <param name="itemType">Type of the item.</param>
        public static void LookupPathToParent(Sitecore.Data.Items.Item currentItem, ref Sitecore.Data.Items.Item projectItem, ref List<ID> itemIds, string itemType)
        {
            if (currentItem.Template == null)
                return;
            projectItem = currentItem;
            itemIds.Add(projectItem.ID);
            if (currentItem.TemplateName.Equals(itemType, StringComparison.InvariantCultureIgnoreCase))
                return;
            SitecoreHelper.LookupPathToParent(currentItem.Parent, ref projectItem, ref itemIds, itemType);
        }

        public static string GetReferencePath(Database db, Sitecore.Data.Items.Item item, string fieldName)
        {
            item.Fields.ReadAll();
            if (item.Fields["Reference Store Paths"] != null && !string.IsNullOrEmpty(item.Fields["Reference Store Paths"].Value))
            {
                Sitecore.Data.Items.Item obj = db.GetItem(item.Fields["Reference Store Paths"].Value);
                if (obj != null)
                {
                    obj.Fields.ReadAll();
                    if (obj.Fields[fieldName] != null)
                        return obj.Fields[fieldName].Value;
                }
            }
            return string.Empty;
        }

        public static string GetFullPath(Database db, Sitecore.Data.Items.Item item, string fieldName, string partPath)
        {
            string referencePath = SitecoreHelper.GetReferencePath(db, item, fieldName);
            if (!string.IsNullOrEmpty(partPath) && !string.IsNullOrEmpty(referencePath) && !partPath.Contains(referencePath))
                partPath = FileUtil.MakePath(referencePath, partPath);
            return partPath;
        }

        /// <summary>
        /// Fetches the field value.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        /// <param name="fieldname">The fieldname.</param>
        /// <param name="currentDatabase">The current database.</param>
        /// <returns></returns>
        public static string FetchFieldValue(Sitecore.Data.Items.Item currentItem, string fieldname, Database currentDatabase)
        {
            string str1 = string.Empty;
            Field field = currentItem.Fields[fieldname];
            if (fieldname == "Item Reference" | fieldname == "Item Reference" | fieldname == "MasterSnippet Reference" | fieldname == "Preview Reference" | fieldname == "Content Reference" | fieldname == "MediaLibrary Reference" | fieldname == "Reference")
                str1 = currentItem.Fields[fieldname].Value;
            else if (field.Type.ToLower() == "date")
                str1 = ((DateField)field).DateTime.ToString();
            else if (field.Type.ToLower() == "datetime")
                str1 = ((DateField)field).DateTime.ToString();
            else if (field.Type.ToLower() == "checkbox")
                str1 = currentItem.Fields[fieldname].Value;
            else if (field.Type == "Droplink")
            {
                LookupField lookupField = (LookupField)field;
                string path = lookupField.InnerField.GetValue(true, true);
                Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                str1 = !(obj.Template.Name == "Pattern") ? lookupField.InnerField.GetValue(true, true) : obj.Name;
            }
            else if (field.Type == "Grouped Droplink")
            {
                LookupField lookupField = (LookupField)field;
                string path = lookupField.InnerField.GetValue(true, true);
                Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                str1 = !(obj.Template.Name == "Pattern") ? lookupField.InnerField.GetValue(true, true) : obj.Name;
            }
            else if (field.Type == "Droptree")
            {
                LookupField lookupField = (LookupField)field;
                string path = lookupField.InnerField.GetValue(true, true);
                Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                str1 = !(obj.Template.Name == "Pattern") ? lookupField.InnerField.GetValue(true, true) : obj.Name;
            }
            else if (field.Type == "Droplist")
                str1 = ((LookupField)field).InnerField.GetValue(true, true);
            else if (field.Type == "Grouped Droplist")
                str1 = ((LookupField)field).InnerField.GetValue(true, true);
            else if (field.Type == "Checklist")
            {
                string str2 = field.Value;
                char[] chArray = new char[1]
        {
          '|'
        };
                foreach (string path in str2.Split(chArray))
                {
                    Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                    str1 = str1 + obj.Name + "\n";
                }
            }
            else if (field.Type == "Single-Line Text" || field.Type == "Multi-Line Text" || field.Type == "Rich Text")
            {
                str1 = field.Value;
            }            
            else if (field.Type == "Image")
            {
                MediaItem mediaItem = (MediaItem)((ImageField)field).MediaItem;
                string destination = MediaManager.GetMediaUrl(mediaItem);
                
                str1 = destination;
            }
            else if (field.Type.ToLower() == "text")
                str1 = field.Value;
            else if (field.Type == "Integer" | field.Type == "Number")
                str1 = field.Value;
            
            return str1;
        }

        /// <summary>
        /// Gets the API user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public static User GetApiUser(string username)
        {
            User user = (User)null;
            string fullName = Factory.GetDomain(ConfigHandler.CommonSettings.Domain).GetFullName(username);
            if (User.Exists(fullName))
                user = (User)Account.FromName(fullName, AccountType.User);
            return user;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="id">The id.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="languageIndex">Index of the language.</param>
        /// <returns></returns>
        //public static string GetAttributeValue(string username, string id, string attributeName, int languageIndex)
        //{
        //    string str1 = string.Empty;
        //    try
        //    {
        //        User apiUser = SitecoreHelper.GetApiUser(username);
        //        if ((Account)apiUser != (Account)null)
        //        {
        //            using (new UserSwitcher(apiUser))
        //            {
        //                Database database = Factory.GetDatabase(ConfigHandler.CommonSettings.Database);
        //                using (new LanguageSwitcher(LanguageManager.GetLanguages(database)[languageIndex]))
        //                {
        //                    Sitecore.Data.Items.Item obj1 = database.GetItem(id);
        //                    string name = obj1.Template.Name;
        //                    string str2 = SitecoreHelper.FetchPrintEngineTemplatesFolderPath(database);
        //                    foreach (Sitecore.Data.Items.Item obj2 in database.GetItem(str2 + "/" + name + "/attributes").Children)
        //                    {
        //                        if (obj2.Name == attributeName)
        //                            return obj1.Fields[obj2.Name].Value;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        str1 = ex.Message;
        //        Log.Error("GetAttributeValue", ex, typeof(SitecoreHelper));
        //    }
        //    return str1;
        //}

        //public static Sitecore.Data.Items.Item FetchPrintEngineTemplate(string templateName, Database currentDatabase)
        //{
        //    try
        //    {
        //        return (Sitecore.Data.Items.Item)(TemplateItem)currentDatabase.Items.GetItem(ConfigHandler.EpubPrintEngineSettings.EngineTemplates + templateName);
        //    }
        //    catch
        //    {
        //        return (Sitecore.Data.Items.Item)(TemplateItem)currentDatabase.Items.GetItem("/sitecore/templates/p_templates/" + templateName);
        //    }
        //}

        /// <summary>
        /// Fetches the print engine templates folder path.
        /// </summary>
        /// <param name="currentDatabase">The current database.</param>
        /// <returns></returns>
        //public static string FetchPrintEngineTemplatesFolderPath(Database currentDatabase)
        //{
        //    try
        //    {
        //        return currentDatabase.Items.GetItem(ConfigHandler.EpubPrintEngineSettings.EngineTemplates.TrimEnd(new char[1]
        //{
        //  '/'
        //})).Paths.FullPath;
        //    }
        //    catch
        //    {
        //        return currentDatabase.Items.GetItem("/sitecore/templates/p_templates").Paths.FullPath;
        //    }
        //}

        /// <summary>
        /// Checks the priveleges.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="itemID">The item ID.</param>
        /// <returns></returns>
        public static int CheckPriveleges(string username, string itemID)
        {
            itemID = itemID.Replace("[WB]", string.Empty);
            int num = 0;
            try
            {
                Sitecore.Data.Items.Item obj = Factory.GetDatabase(ConfigHandler.CommonSettings.Database).GetItem(itemID);
                User apiUser = SitecoreHelper.GetApiUser(username);
                if ((Account)apiUser != (Account)null)
                {
                    using (new UserSwitcher(apiUser))
                    {
                        if (obj.Access.CanRead())
                            ++num;
                        if (obj.Access.CanDelete())
                            num += 2;
                        if (obj.Access.CanRename())
                            num += 4;
                        if (obj.Access.CanRead() && obj.Access.CanWrite())
                            num += 8;
                        if (obj.Access.CanWrite())
                            num += 16;
                        if (obj.Access.CanRead() && obj.Access.CanCreate() && obj.Access.CanWrite())
                            num += 32;
                        if (obj.Access.CanWrite())
                        {
                            if (obj.Access.CanRename())
                            {
                                if (obj.Access.CanCreate())
                                {
                                    if (obj.Access.CanDelete())
                                        num += 64;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                num = 0;
                Log.Error("CheckPriveleges", ex, typeof(SitecoreHelper));
            }
            return num;
        }

        /// <summary>
        /// Checks if item exists.
        /// </summary>
        /// <param name="parentItem">The parent item.</param>
        /// <param name="newItemName">New name of the item.</param>
        /// <returns></returns>
        public static string CheckIfItemExists(Sitecore.Data.Items.Item parentItem, string newItemName)
        {
            string str = string.Empty;
            foreach (Sitecore.Data.Items.Item obj in parentItem.Children)
            {
                if (obj.Name.ToLower() == newItemName.ToLower())
                {
                    str = obj.ID.ToString();
                    break;
                }
            }
            return str;
        }

        /// <summary>
        /// Gets the default setting root item id.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        //public static string GetDefaultSettingRootItemId(string fieldName)
        //{
        //    string str = string.Empty;
        //    try
        //    {
        //        using (new SecurityDisabler())
        //        {
        //            Database database = Factory.GetDatabase(ConfigHandler.CommonSettings.Database);
        //            Sitecore.Data.Items.Item obj = database.GetItem(ConfigHandler.PrintStudioEngineSettings.InDesignConnectorDefaultSettings);
        //            str = database.GetItem(obj.Fields[fieldName].Value).ID.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("GetDefaultSettingRootItemId", ex, typeof(SitecoreHelper));
        //    }
        //    return str;
        //}

        /// <summary>
        /// Gets the workflows.
        /// </summary>
        /// <param name="currentDatabase">The current database.</param>
        /// <returns></returns>
        public static Hashtable GetWorkflows(Database currentDatabase)
        {
            Hashtable hashtable = new Hashtable();
            foreach (IWorkflow workflow in currentDatabase.WorkflowProvider.GetWorkflows())
                hashtable.Add((object)workflow.WorkflowID, (object)workflow.WorkflowID);
            return hashtable;
        }

        /// <summary>
        /// Gets the state of the workflow items of.
        /// </summary>
        /// <param name="workFlow">The work flow.</param>
        /// <param name="workFlowState">State of the work flow.</param>
        /// <returns></returns>
        public static Hashtable GetWorkflowItemsOfState(IWorkflow workFlow, WorkflowState workFlowState)
        {
            Hashtable hashtable = new Hashtable();
            foreach (DataUri dataUri in workFlow.GetItems(workFlowState.StateID))
            {
                try
                {
                    if (!hashtable.Contains((object)dataUri.ItemID.ToString()))
                        hashtable.Add((object)dataUri.ItemID.ToString(), (object)dataUri.ItemID.ToString());
                }
                catch (Exception ex)
                {
                    Log.Error("GetWorkflowItemsOfState", ex, typeof(SitecoreHelper));
                }
            }
            return hashtable;
        }

        /// <summary>
        /// Gets the state of the workflow items of.
        /// </summary>
        /// <param name="workFlow">The work flow.</param>
        /// <param name="workFlowState">State of the work flow.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public static List<string> GetWorkflowItemsOfState(IWorkflow workFlow, WorkflowState workFlowState, Language language)
        {
            List<string> list = new List<string>();
            foreach (DataUri dataUri in workFlow.GetItems(workFlowState.StateID))
            {
                try
                {
                    if (dataUri.Language == language)
                    {
                        if (!list.Contains(dataUri.ItemID.ToString()))
                            list.Add(dataUri.ItemID.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("GetWorkflowItemsOfState", ex, typeof(SitecoreHelper));
                }
            }
            return list;
        }

        public static string MapFile(string filename)
        {
            Assert.IsNotNullOrEmpty(filename, "filename");
            return FileUtil.MapPath(filename);
        }

        public static string GetPrintFilePath(Sitecore.Data.Items.Item processingJobItem, string settingName)
        {
            ReferenceField referenceField = (ReferenceField)processingJobItem.Fields["Storing Settings"];
            if (referenceField != null && referenceField.TargetItem != null)
                return referenceField.TargetItem[settingName];
            else
                return string.Empty;
        }

        public static string GetMessage(Sitecore.Data.Items.Item processingJobItem, Language language, string messageFieldName)
        {
            if (processingJobItem.Fields[messageFieldName] == null || string.IsNullOrEmpty(processingJobItem.Fields[messageFieldName].Value))
                return string.Empty;
            string str = string.Empty;
            if (processingJobItem.Fields[messageFieldName].Type.ToLower() == "droplink" || processingJobItem.Fields[messageFieldName].Type.ToLower() == "droplist")
            {
                Sitecore.Data.Items.Item obj = SitecoreHelper.GetItem(ID.Parse(processingJobItem.Fields[messageFieldName].Value), language, processingJobItem.Database);
                if (obj != null && obj.Fields["Message"] != null)
                    str = obj.Fields["Message"].Value;
            }
            else
                str = processingJobItem.Fields[messageFieldName].Value;
            return str;
        }

        public static Sitecore.Data.Items.Item GetItem(ID itemID, Language language, Database dataBase)
        {
            bool langOk = true;
            Sitecore.Data.Items.Item obj = dataBase.GetItem(itemID, language);
            if (obj == null)
                return (Sitecore.Data.Items.Item)null;
            if (obj.Language == language && SitecoreHelper.LanguageVersionExist(obj, language))
                return obj;
            else
                return SitecoreHelper.GetExistingLanguageVersion(obj, language, ref langOk);
        }

        public static Sitecore.Data.Items.Item GetItem(ID itemID, Language language)
        {
            Database database = Factory.GetDatabase(ConfigHandler.CommonSettings.Database);
            return SitecoreHelper.GetItem(itemID, language, database);
        }

        public static Sitecore.Data.Items.Item GetItem(ID itemID, string lang)
        {
            Language language = LanguageManager.GetLanguage(lang);
            if (language != (Language)null)
                return SitecoreHelper.GetItem(itemID, language);
            else
                return (Sitecore.Data.Items.Item)null;
        }

        public static Sitecore.Data.Items.Item GetItem(string itemPath, Language language, Database database)
        {
            bool langOk = true;
            Sitecore.Data.Items.Item obj = database.GetItem(itemPath, language);
            if (obj == null)
                return (Sitecore.Data.Items.Item)null;
            else
                return SitecoreHelper.GetExistingLanguageVersion(obj, language, ref langOk);
        }

        public static Sitecore.Data.Items.Item GetItem(string itemPath, Language language)
        {
            Database database = Factory.GetDatabase(ConfigHandler.CommonSettings.Database);
            return SitecoreHelper.GetItem(itemPath, language, database);
        }

        public static bool LanguageVersionExist(Sitecore.Data.Items.Item item, Language lang)
        {
            if (item == null)
                return false;
            else
                return Enumerable.Any<Sitecore.Data.Items.Item>((IEnumerable<Sitecore.Data.Items.Item>)item.Versions.GetVersions(true), (Func<Sitecore.Data.Items.Item, bool>)(vi => vi.Language == lang));
        }

        public static bool LanguageVersionExist(Guid itemID, Language language, Database database)
        {
            try
            {
                if (language != (Language)null)
                    return SitecoreHelper.LanguageVersionExist(database.GetItem(new ID(itemID), language), language);
            }
            catch (Exception ex)
            {
                Log.Error("LanguageVersionExist", ex, typeof(SitecoreHelper));
            }
            return false;
        }

        public static bool LanguageVersionExist(Guid itemID, Language language)
        {
            Database database = Factory.GetDatabase(ConfigHandler.CommonSettings.Database);
            return SitecoreHelper.LanguageVersionExist(itemID, language, database);
        }

        public static Sitecore.Data.Items.Item GetExistingLanguageVersion(Sitecore.Data.Items.Item item, Language lang, ref bool langOk)
        {
            bool flag = false;
            Sitecore.Data.Items.Item obj1 = (Sitecore.Data.Items.Item)null;
            if (item != null)
            {
                foreach (Sitecore.Data.Items.Item obj2 in item.Versions.GetVersions(true))
                {
                    if (obj2.Language == lang)
                    {
                        flag = true;
                        break;
                    }
                    else if (obj2.Language == LanguageManager.DefaultLanguage)
                        obj1 = obj2;
                }
                if (flag)
                    obj1 = item;
                if (!flag)
                    langOk = false;
            }
            return obj1;
        }

        public static Language GetUserLanguage(User user, Database database)
        {
            Language language = LanguageManager.GetLanguage(Settings.ClientLanguage, database);
            if (!string.IsNullOrEmpty(user.Profile.ClientLanguage) && LanguageManager.IsLanguageNameDefined(database, user.Profile.ClientLanguage))
                language = LanguageManager.GetLanguage(user.Profile.ClientLanguage);
            else if (!string.IsNullOrEmpty(user.Profile.ContentLanguage) && LanguageManager.IsLanguageNameDefined(database, user.Profile.ContentLanguage))
                language = LanguageManager.GetLanguage(user.Profile.ContentLanguage);
            return language;
        }

        private static void CheckThisItemVersion(Sitecore.Data.Items.Item currentItem, LanguageCollection languageArr, string username, int languageIndex, Database currentDataBase)
        {
            if (Enumerable.Count<Sitecore.Data.Items.Item>((IEnumerable<Sitecore.Data.Items.Item>)currentItem.Versions.GetVersions(true), (Func<Sitecore.Data.Items.Item, bool>)(item => item.Language.Name == languageArr[languageIndex].Name)) != 0)
                return;
            SitecoreHelper.AddVersion(username, currentItem.ID.ToString(), languageIndex, currentDataBase);
        }
    }
}
