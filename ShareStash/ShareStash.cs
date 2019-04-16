using Partiality.Modloader;
using UnityEngine;


namespace ShareStash
{
    public class ShareStash : PartialityMod
    {
        public static initializer script;

        public ShareStash()
        {
            this.ModID = "ShareStash";
            this.Version = "0100";
            this.author = "Savantic";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            initializer.mod = this;
            GameObject gameObject = new GameObject();
            ShareStash.script = gameObject.AddComponent<initializer>();
            ShareStash.script.Initialize();
        }
    }

}