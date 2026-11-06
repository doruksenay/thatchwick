using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Işıksız (unlit) sprite materyali. Geçici: sahnede Light2D olmadan da görünsün diye.
    /// Gerçek varlık hattında materyal bir asset olarak projeye eklenecek (Shader.Find
    /// derlemelerde shader ayıklanırsa null dönebilir).
    /// </summary>
    public static class SpriteMaterials
    {
        private static Material _unlit;

        public static Material Unlit
        {
            get
            {
                if (_unlit != null) return _unlit;

                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                _unlit = new Material(shader) { name = "RuntimeUnlitSprite" };
                return _unlit;
            }
        }

        /// <summary>1x1 beyaz sprite (1 dünya birimi), renk SpriteRenderer.color ile verilir.</summary>
        public static Sprite WhiteSquare
        {
            get
            {
                if (_white != null) return _white;
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                tex.SetPixel(0, 0, Color.white);
                tex.Apply(false);
                _white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                return _white;
            }
        }

        private static Sprite _white;
    }
}
