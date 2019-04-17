namespace ShareStash
{
    [System.Serializable]
    public class ItemData
    {
        public int itemID;
        public string itemUID;

        public AddItem(int ID, string UID)
        {
            this.itemID = ID;
            this.itemUID = UID;
        }

        public bool ContainsItem(string UID)
        {
            if (UID == this.itemUID)
            {
                return true
            }
            return false
        }
    }
}