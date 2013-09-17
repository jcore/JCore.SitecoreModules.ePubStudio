using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Reflection;
using Sitecore.SharedModules.ePub.Publishing.Renderers;

namespace Sitecore.SharedModules.ePub.Publishing
{
    /// <summary>
    /// 
    /// </summary>
    public class PrintFactory
    {
        private static readonly Hashtable PrintRendererTypes = new Hashtable();

        static PrintFactory()
        {
        }

        /// <summary>
        /// Adds the type to cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type.</param>
        public static void AddTypeToCache(string key, Type type)
        {
            lock (PrintFactory.PrintRendererTypes.SyncRoot)
            {
                if (PrintFactory.PrintRendererTypes[(object)key] != null)
                    return;
                PrintFactory.PrintRendererTypes.Add((object)key, (object)type);
            }
        }

        /// <summary>
        /// Removes the type from cache.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public static void RemoveTypeFromCache(string typeName)
        {
            lock (PrintFactory.PrintRendererTypes.SyncRoot)
            {
                if (PrintFactory.PrintRendererTypes[(object)typeName] == null)
                    return;
                PrintFactory.PrintRendererTypes.Remove((object)typeName);
            }
        }

        /// <summary>
        /// Gets the renderer.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static EpubItemRendererBase GetRenderer(Item item)
        {
            if (item == null)
                return (EpubItemRendererBase)null;
            ReferenceField referenceField = (ReferenceField)item.Fields["Renderer"];
            if (referenceField == null)
                return (EpubItemRendererBase)null;
            Item targetItem = referenceField.TargetItem;
            if (targetItem != null)
            {
                string str = targetItem["type"];
                if (string.IsNullOrEmpty(str))
                    return (EpubItemRendererBase)null;
                try
                {
                    Type type;
                    lock (PrintFactory.PrintRendererTypes.SyncRoot)
                        type = PrintFactory.PrintRendererTypes[(object)str] as Type;
                    if (type == null)
                    {
                        Type typeInfo = ReflectionUtil.GetTypeInfo(str);
                        if (typeInfo == null)
                        {
                            Log.Error("Could not load type " + str,typeof(PrintFactory));
                            return (EpubItemRendererBase)null;
                        }
                        else
                        {
                            PrintFactory.AddTypeToCache(str, typeInfo);
                            type = typeInfo;
                        }
                    }
                    EpubItemRendererBase result = ReflectionUtil.CreateObject(type) as EpubItemRendererBase;
                    if (result != null)
                    {
                        result.RenderingItem = item;
                        PrintFactory.SetRenderingProperties(item, result);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex, typeof(PrintFactory));
                    return (EpubItemRendererBase)null;
                }
            }
            return (EpubItemRendererBase)null;
        }

        /// <summary>
        /// Sets the rendering properties.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="result">The result.</param>
        private static void SetRenderingProperties(Item item, EpubItemRendererBase result)
        {
            Assert.ArgumentNotNull((object)item, "item");
            Assert.ArgumentNotNull((object)result, "result");
            string parameters = item["parameters"];
            if (string.IsNullOrEmpty(parameters))
                return;
            ReflectionUtil.SetProperties((object)result, parameters, true);
        }
    }
}
