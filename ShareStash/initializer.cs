using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace ShareStash
{
    public class initializer : MonoBehaviour
    {
        public static ShareStash mod;

        List<ItemData> itemData;

        public void Initialize()
        {
            itemData = this.LoadItems();
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
                if (!itemData.ContainsItem(item.UID))
                {
                    itemData.AddItem(item.UID);
                    initializer.SaveItems(itemData);
                    Debug.Log("item added" + item.UID);
                }
            }
            original.Invoke(item, container);
        }

        public bool ShareStashRemoveItemPatch(On.ItemContainer.orig_RemoveItem original, ItemContainer container, Item item)
        {
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash && !NetworkLevelLoader.Instance.IsSceneLoading)
            {
                itemData.RemoveItem(item.UID);
                initializer.SaveItems(itemData);
                Debug.Log("item removed" + item.UID);
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
                Debug.Log("NUmber in stash " + itemData.itemList.Count);
                for (int i = 0; i < itemData.itemList.Count; i++)
                {
                    Debug.Log("Item in container? " + container.Contains(itemData.itemList[i]));
                    if (!container.Contains(itemData.itemList[i]))
                    {
                        Debug.Log("item add start " + itemData.itemList[i]);
                        Item localItem = ItemManager.Instance.GetItem(itemData.itemList[i]);
                        if(localItem == null)
                        {
                            Debug.Log("Yo local item is null! ");
                        }
                        container.AddItem(localItem);
                        Debug.Log("item add finish " + itemData.itemList[i]);
                    }
                }
                
            }
            original.Invoke(instance);
        }

        public List<ItemData> LoadItems()
        {
            if (!File.Exists(Application.persistentDataPath + "/Stash.json"))
            {
                StreamWriter sw = File.CreateText(Application.persistentDataPath+"/Stash.json");
                sw.Close();

                string json = "{\"itemList\":[]}";
                File.WriteAllText(Application.persistentDataPath + "/Stash.json", json);
                Debug.Log("Blank json created at "+ Application.persistentDataPath);
            }

            using (StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/Stash.json"))
            {
                List<ItemData> itemData = JsonUtility.FromJson<List<ItemData>>(streamReader.ReadToEnd());
                return itemData;
            }

        }

        public static void SaveItems(List<ItemData> itemData)
        {

            string json = JsonUtility.ToJson(itemData);
            File.WriteAllText(Application.persistentDataPath + "/Stash.json", json);

        }
    }
}
