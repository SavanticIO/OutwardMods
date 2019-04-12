using Partiality.Modloader;
using UnityEngine;


namespace Meditation 
{
    public class Meditation : PartialityMod
    {
        public static initializer script;

        public Meditation()
        {
            this.ModID = "Meditation";
            this.Version = "0100";
            this.author = "Savantic";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            initializer.mod = this;
            GameObject gameObject = new GameObject();
            Meditation.script = gameObject.AddComponent<initializer>();
            Meditation.script.Initialize();
        }
    }

}
