using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

namespace ShareStash
{
    public class initializer : MonoBehaviour
    {
        public static ShareStash mod;

        public Dictionary<string, string> itemList;

        public void Initialize()
        {
            itemList = this.LoadItems();
            Patch();
        }

        public void Patch()
        {
            On.Item.Store += new On.Item.hook_Store(this.ShareStashStorePatch);
            On.ItemContainer.RemoveItem += new On.ItemContainer.hook_RemoveItem(this.ShareStashRemoveItemPatch);
            //On.ItemContainer.ShowContent += new On.ItemContainer.hook_ShowContent(this.ShareStashShowContentPatch);
            On.InteractionOpenContainer.OnActivationDone += new On.InteractionOpenContainer.hook_OnActivationDone(this.ShareStashOnActivationDonePatch);
        }

        public void ShareStashStorePatch(On.Item.orig_Store original, Item item, ItemContainer container)
        {
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash)
            {
                if (!itemList.ContainsKey(item.UID))
                {
                    itemList.Add(item.UID,item.ItemIDString);
                    initializer.SaveItems(itemList);
                    Debug.Log("item added" + item.ItemIDString);
                }
            }
            original.Invoke(item, container);
        }

        public bool ShareStashRemoveItemPatch(On.ItemContainer.orig_RemoveItem original, ItemContainer container, Item item)
        {
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash && !NetworkLevelLoader.Instance.IsSceneLoading)
            {
                itemList.Remove(item.ItemIDString);
                initializer.SaveItems(itemList);
                Debug.Log("item removed" + item.ItemIDString);
            }
            return original.Invoke(container, item);
        }

        public void ShareStashOnActivationDonePatch(On.InteractionOpenContainer.orig_OnActivationDone original, InteractionOpenContainer instance)
        {
            FieldInfo m_container = typeof(InteractionOpenContainer).GetField("m_container", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo m_bag = typeof(InteractionOpenContainer).GetField("m_bag", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            ItemContainer container = (ItemContainer)m_container.GetValue(instance);
            Bag bag = (Bag)m_bag.GetValue(instance);
            FieldInfo m_containedItems = typeof(ItemContainer).GetField("m_containedItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            DictionaryExt<string, Item> itemDict = (DictionaryExt<string, Item>)m_containedItems.GetValue(container);
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash)
            {
                if(container == null)
                {
                    Debug.Log("container is null son!");
                }
                if (bag == null)
                {
                    Debug.Log("bag is null son!");
                }
                Debug.Log("NUmber in stash " + itemList.Count);
                for (int i = 0; i < itemList.Count; i++)
                {
                    Debug.Log("Item in container? " + container.Contains(itemList.Keys.ElementAt(i)));
                    if (!container.Contains(itemList.Keys.ElementAt(i)))
                    {
                        Debug.Log("item add start " + itemList.Keys.ElementAt(i));
                        Item localItem = ItemManager.Instance.GetItem(itemList.Keys.ElementAt(i));
                        if(localItem == null)
                        {
                            Debug.Log("Yo local item is null! ");
                        }
                        container.AddItem(localItem);
                        Debug.Log("item add finish " + itemList.Keys.ElementAt(i));
                    }
                }
                
            }
            original.Invoke(instance);
        }

        public Dictionary<string, string> LoadItems()
        {
            if (!File.Exists(Application.persistentDataPath + "/Stash.json"))
            {
                StreamWriter sw = File.CreateText(Application.persistentDataPath+"/Stash.json");
                sw.Close();

                string json = "{}";
                File.WriteAllText(Application.persistentDataPath + "/Stash.json", json);
                Debug.Log("Blank json created at "+ Application.persistentDataPath);
            }

            using (StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/Stash.json"))
            {
                itemList =  JsonConvert.DeserializeObject<Dictionary<string, string>>(streamReader.ReadToEnd());
                return itemList;
            }

        }

        public static void SaveItems(Dictionary<string, string> itemList)
        {

            string json = JsonConvert.SerializeObject(itemList, Formatting.Indented);
            File.WriteAllText(Application.persistentDataPath + "/Stash.json", json);

        }
    }
}
