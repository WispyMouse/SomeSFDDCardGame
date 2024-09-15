namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class ElementDatabase
    {
        public static Dictionary<string, Element> ElementData { get; set; } = new Dictionary<string, Element>();

        public static void AddElement(ElementImport toAdd, Sprite forSprite, Sprite greyscaleSprite, int? spriteIndex = null)
        {
            Element newElement = new Element()
            {
                Id = toAdd.Id.ToLower(),
                Name = toAdd.Name,
                Sprite = forSprite,
                SpriteIndex = spriteIndex,
                GreyscaleSprite = greyscaleSprite
            };

            ElementData.Add(toAdd.Id.ToLower(), newElement);
        }

        public static Element GetElement(string id)
        {
            return ElementData[id.ToLower()];
        }
    }
}