using MoonSharp.Interpreter;

namespace Game.Scripting
{
    class ChestContentsProxy
    {
        ChestContents Contents;

        [MoonSharpHidden]
        public ChestContentsProxy(ChestContents contents)
        {
            Contents = contents;
        }

        public void Add(string itemName, int quantity = 1)
        {
            var item = Item.Get(itemName);
            Contents.Items.Add(new ChestItem
            {
                Item = item,
                Quantity = quantity,
            });
        }
    }
}
