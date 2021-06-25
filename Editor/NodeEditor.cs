using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Port
    {
        public Rect rect;
        public BaseNode node;
        public int id;
        public Port(BaseNode node, int id)
        {
            this.node = node;
            this.id = id;
            rect = new Rect(0, 0, 2f, 2f);
        }

        public void Draw()
        {
            rect.height = node.rect.height * 0.5f;
            rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;
            switch (id)
            {
                case 2:
                    rect.x = node.rect.x + node.rect.width / 2 - 7;
                    break;

                case 1:
                    rect.x = node.rect.x + node.rect.width / 2 + 7;
                    break;
            }
        }
    }
}
