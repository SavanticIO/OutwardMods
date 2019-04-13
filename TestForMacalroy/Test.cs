using Partiality.Modloader;
using UnityEngine;


namespace Test
{
    public class Test: PartialityMod
    {
        public static initializer script;

        public Test()
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
            Test.script = gameObject.AddComponent<initializer>();
            Test.script.Initialize();
        }
    }
}
