using Partiality.Modloader;
using UnityEngine;

namespace CustomDifficulty
{
    public class CustomDifficulty : PartialityMod
    {
        public static initializer script;

        public CustomDifficulty()
        {
            this.ModID = "CustomDifficulty";
            this.Version = "0100";
            this.author = "Savantic";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            initializer.mod = this;
            GameObject gameObject = new GameObject();
            CustomDifficulty.script = gameObject.AddComponent<initializer>();
            CustomDifficulty.script.Initialize();
        }
    }
}

