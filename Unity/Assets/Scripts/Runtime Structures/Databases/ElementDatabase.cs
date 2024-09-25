namespace SFDDCards
{
    using SFDDCards.ImportModels;
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
            if (ElementData.TryGetValue(id.ToLower(), out Element foundElement))
            {
                return foundElement;
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse element {id}.", GlobalUpdateUX.LogType.RuntimeError);

            return null;
        }

        public static void ClearDatabase()
        {
            ElementData.Clear();
        }
    }
}