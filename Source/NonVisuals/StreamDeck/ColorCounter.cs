namespace NonVisuals.StreamDeck
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    internal class ColorCounter
    {
        private readonly List<ColorCount> _colorList = new List<ColorCount>(50);

        public void RegisterColor(Color color)
        {
            if (_colorList.Count(o => o.Color == color) == 0)
            {
                var colorCount = new ColorCount
                {
                    Color = color
                };
                _colorList.Add(colorCount);
            }
            else
            {
                _colorList.Find(o => o.Color == color).Count++;
            }
        }

        public Color GetMajority()
        {
            var maxValue = 1;
            var result = Color.Black;

            foreach (var colorCount in _colorList.Where(colorCount => colorCount.Count > maxValue))
            {
                maxValue = colorCount.Count;
                result = colorCount.Color;
            }

            return result;
        }


        class ColorCount
        {
            internal int Count = 1;
            internal Color Color = Color.Black;
        }
    }

}
