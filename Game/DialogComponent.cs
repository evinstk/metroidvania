using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nez;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    class Dialog
    {
        public string StartingNode;
        public Dictionary<string, Node> Nodes;
        public List<Line> Script;
    }

    class Node
    {
        public List<Line> Lines;
        public string NextNode;
    }

    class Line
    {
        public string Speaker;
        public string Speech;
    }

    class DialogComponent : Component
    {
        public Dialog Dialog { get; private set; }

        string _dialogSrc;
        string _currNode;

        public DialogComponent(string dialogSrc)
        {
            _dialogSrc = dialogSrc;
            Insist.IsNotNull(_dialogSrc);
        }

        public override void OnAddedToEntity()
        {
            var path = "Content/Dialog/" + _dialogSrc;
            using (var stream = TitleContainer.OpenStream(path))
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                Dialog = new JsonSerializer().Deserialize<Dialog>(jsonTextReader);
            }

            _currNode = Dialog.StartingNode;
        }

        public IEnumerable<Line> FeedLines()
        {
            var node = Dialog.Nodes[_currNode];
            foreach (var line in node.Lines)
            {
                yield return line;
            }
            if (node.NextNode != null)
            {
                _currNode = node.NextNode;
            }
        }
    }
}
