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
            On.ItemContainer.AddItem += new On.Item.hook_AddItem(this.ShareStashAddItemPatch);
            On.ItemContainer.RemoveItem += new On.ItemContainer.hook_RemoveItem(this.ShareStashRemoveItemPatch);
            On.InteractionOpenContainer.OnActivationDone += new On.InteractionOpenContainer.hook_OnActivationDone(this.ShareStashOnActivationDonePatch);
        }

        public void ShareStashAddItemPatch(On.Item.orig_AddItem original, ItemContainer container, Item item)
        {
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash)
            {   
                for (int i = 0; i < itemData.Count; i++)
                {
                    if (!itemData[i].ContainsItem(item.UID))
                    {   
                        ItemData localitemData = new ItemData();
                        localitemData.AddItem(item.ItemID,item.UID);
                        itemData.Add(localitemData);
                        initializer.SaveItems(itemData);
                        Debug.Log("item added" + item.UID);
                    }
                }
            }
            original.Invoke(container, item);
        }

        public bool ShareStashRemoveItemPatch(On.ItemContainer.orig_RemoveItem original, ItemContainer container, Item item)
        {
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash && !NetworkLevelLoader.Instance.IsSceneLoading)
            {   
                for (int i = 0; i < itemData.Count; i++)
                {   
                    if (itemData[i].ContainsItem(item.UID))
                    {
                        itemData.RemoveAt(i);
                        initializer.SaveItems(itemData);
                        Debug.Log("item removed" + item.UID);
                    }
                }
            }
            return original.Invoke(container, item);
        }

        public void ShareStashOnActivationDonePatch(On.InteractionOpenContainer.orig_OnActivationDone original, InteractionOpenContainer instance)
        {   
            FieldInfo m_container = typeof(InteractionOpenContainer).GetField("m_container", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            ItemContainer container = (ItemContainer)m_container.GetValue(instance);
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash)
            {
                Debug.Log("NUmber in stash " + itemData.Count);
                for (int i = 0; i < itemData.Count; i++)
                {
                    Debug.Log("Item in container? " + container.Contains(itemData[i].itemUID));
                    if (!container.Contains(itemData[i].itemUID))
                    {
                        Debug.Log("item add start " + itemData[i].itemUID);
                        Item localItem = ItemManager.Instance.GenerateItem(itemData[i].itemID);
                        if(localItem == null)
                        {
                            Debug.Log("Yo local item is null! ");
                        }
                        itemData.RemoveAt(i)
                        container.AddItem(localItem);
                        Debug.Log("item add finish " + itemData[i].itemUID);
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

                string json = "";
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
