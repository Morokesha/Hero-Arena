namespace Core.Extension
{
    public enum Element { Fire, Water, Earth }
    
    public static class ElementUtils
    {
        // Пример: Water слаб к Fire, Fire слаб к Earth, etc. (добавь свои правила)
        public static bool IsWeakTo(Element attackerElement, Element targetElement)
        {
            return (attackerElement == Element.Water && targetElement == Element.Fire) ||
                   (attackerElement == Element.Fire && targetElement == Element.Earth) ||
                   (attackerElement == Element.Earth && targetElement == Element.Water);
        }
    }
}