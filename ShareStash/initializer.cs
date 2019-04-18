using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ShareStash
{
    public class initializer : MonoBehaviour
    {
        public static ShareStash mod;

        ContainerData itemData;

        public void Initialize()
        {
            itemData = this.LoadItems();
            Patch();
        }

        public void Patch()
        {
            On.ItemContainer.AddItem += new On.ItemContainer.hook_AddItem(this.ShareStashAddItemPatch);
            On.Item.Store += new On.Item.hook_Store(this.ShareStashStorePatch);
            On.ItemContainer.RemoveItem += new On.ItemContainer.hook_RemoveItem(this.ShareStashRemoveItemPatch);
            On.InteractionOpenContainer.OnActivationDone += new On.InteractionOpenContainer.hook_OnActivationDone(this.ShareStashOnActivationDonePatch);
        }

        public bool ShareStashAddItemPatch(On.ItemContainer.orig_AddItem original, ItemContainer container, Item item)
        {
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash)
            {
                if (!itemData.itemList.Contains(item.ItemID+":"+item.UID))
                {
                    string localitemData = item.ItemID + ":" + item.UID;
                    itemData.itemList.Add(localitemData);
                    initializer.SaveItems(itemData);
                    Debug.Log("item added" + item.UID);
                }
            }
            return original.Invoke(container, item);
        }

        public void ShareStashStorePatch(On.Item.orig_Store original, Item item, ItemContainer container)
        {
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash)
            {
                Debug.Log("item store used");
                if (!itemData.itemList.Contains(item.ItemID + ":" + item.UID))
                {
                    string localitemData = item.ItemID + ":" + item.UID;
                    itemData.itemList.Add(localitemData);
                    initializer.SaveItems(itemData);
                    Debug.Log("item added" + item.UID);
                }
            }
            original.Invoke(item,container);
        }

        public bool ShareStashRemoveItemPatch(On.ItemContainer.orig_RemoveItem original, ItemContainer container, Item item)
        {   
            FieldInfo m_interactionTrigger = typeof(InteractionOpenContainer).GetField("m_interactionTrigger", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            InteractionOpenContainer interactionTrigger = (InteractionOpenContainer)m_interactionTrigger.GetValue(instance);
            if (container.SpecialType == ItemContainer.SpecialContainerTypes.Stash && !NetworkLevelLoader.Instance.IsSceneLoading and !interactionTrigger.IsBusy )
            {
                Debug.Log("remove item used");
                if (itemData.itemList.Contains(item.ItemID + ":" + item.UID))
                {
                    itemData.itemList.Remove(item.ItemID + ":" + item.UID);
                    initializer.SaveItems(itemData);
                    Debug.Log("item removed" + item.UID);
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
                container.ClearPouch();
                for (int i = 0; i < itemData.itemList.Count; i++)
                {
                    string localitemID = itemData.itemList[i].Substring(0, itemData.itemList[i].IndexOf(":"));
                    string localitemUID = itemData.itemList[i].Substring(itemData.itemList[i].IndexOf(":") + 1);
                    Debug.Log("Item in container? " + container.Contains(localitemUID));
                    if (!container.Contains(localitemUID))
                    {
                        Debug.Log("item add start " + localitemUID);
                        Item localItem = ItemManager.Instance.GetItem(localitemUID);
                        if (localItem == null)
                        {
                            localItem = ItemManager.Instance.GenerateItem(Int32.Parse(localitemID));
                            ItemManager.Instance.RequestItemInitialization(localItem);
                            ItemManager.Instance.ItemHasBeenAdded(ref localItem);
                        }
                        itemData.itemList.RemoveAt(i);
                        container.AddItem(localItem);
                        Debug.Log("item add finish " + localitemUID);
                    }
                }
                m_container.SetValue(instance,container);
            }
            original.Invoke(instance);
        }

        public ContainerData LoadItems()
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
                ContainerData itemData = JsonUtility.FromJson<ContainerData>(streamReader.ReadToEnd());
                return itemData;
            }
        }

        public static void SaveItems(ContainerData itemData)
        {
            string json = JsonUtility.ToJson(itemData);
            File.WriteAllText(Application.persistentDataPath + "/Stash.json", json);
        }
    }
}
