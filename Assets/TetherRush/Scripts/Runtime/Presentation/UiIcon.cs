using UnityEngine;
using UnityEngine.UI;

namespace TetherRush.Presentation
{
    public enum UiGlyph { Gear, Pause, SoundOn, SoundOff, Back, Home, Retry, Play, Next, Crystal, Check, Lock, Infinity }

    /// <summary>Original, consistently weighted vector symbols. Hit regions belong to the parent button.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiIcon : MaskableGraphic
    {
        [SerializeField] private UiGlyph glyph;
        public UiGlyph Glyph { get => glyph; set { if (glyph == value) return; glyph = value; SetVerticesDirty(); } }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch (glyph)
            {
                case UiGlyph.Gear:
                    Ring(vh, .5f, .5f, .29f, 0, Mathf.PI * 2);
                    Ring(vh, .5f, .5f, .10f, 0, Mathf.PI * 2);
                    for (int i = 0; i < 8; i++) { float a = i * Mathf.PI / 4; Line(vh, P(.5f, .5f) + P(Mathf.Cos(a), Mathf.Sin(a)) * .29f, P(.5f, .5f) + P(Mathf.Cos(a), Mathf.Sin(a)) * .43f, .10f); }
                    break;
                case UiGlyph.Pause: Line(vh, P(.32f,.18f), P(.32f,.82f), .13f); Line(vh, P(.68f,.18f), P(.68f,.82f), .13f); break;
                case UiGlyph.Back: Path(vh, .68f,.17f, .33f,.5f, .68f,.83f); break;
                case UiGlyph.Next: Path(vh, .32f,.17f, .67f,.5f, .32f,.83f); break;
                case UiGlyph.Check: Path(vh, .16f,.49f, .40f,.25f, .85f,.76f); break;
                case UiGlyph.Play: Triangle(vh, P(.27f,.12f), P(.84f,.5f), P(.27f,.88f)); break;
                case UiGlyph.Crystal: Path(vh, .5f,.94f, .84f,.62f, .72f,.29f, .5f,.06f, .28f,.29f, .16f,.62f, .5f,.94f); Line(vh,P(.16f,.62f),P(.84f,.62f)); Path(vh,.5f,.06f,.36f,.62f,.5f,.94f,.64f,.62f,.5f,.06f); break;
                case UiGlyph.Lock: Path(vh,.22f,.1f,.22f,.55f,.78f,.55f,.78f,.1f,.22f,.1f); Ring(vh,.5f,.56f,.21f,0,Mathf.PI); Line(vh,P(.5f,.25f),P(.5f,.39f)); break;
                case UiGlyph.Home: Path(vh,.12f,.5f,.5f,.87f,.88f,.5f); Path(vh,.25f,.58f,.25f,.15f,.75f,.15f,.75f,.58f); break;
                case UiGlyph.Retry: Ring(vh,.5f,.5f,.33f,-.35f,4.9f); Path(vh,.30f,.91f,.56f,.83f,.43f,.60f); break;
                case UiGlyph.SoundOn:
                case UiGlyph.SoundOff:
                    Path(vh,.13f,.36f,.30f,.36f,.51f,.17f,.51f,.83f,.30f,.64f,.13f,.64f,.13f,.36f);
                    if (glyph == UiGlyph.SoundOn) { Ring(vh,.49f,.5f,.24f,-.8f,.8f); Ring(vh,.49f,.5f,.40f,-.8f,.8f); }
                    else { Line(vh,P(.67f,.35f),P(.92f,.65f)); Line(vh,P(.67f,.65f),P(.92f,.35f)); }
                    break;
                case UiGlyph.Infinity:
                    Vector2 last = InfinityPoint(0);
                    for (int i = 1; i <= 64; i++) { var next = InfinityPoint(i * Mathf.PI / 32); Line(vh,last,next); last = next; }
                    break;
            }
        }
        private static Vector2 P(float x, float y) => new Vector2(x,y);
        private static Vector2 InfinityPoint(float t) => P(.5f + .40f * Mathf.Cos(t), .5f + .25f * Mathf.Sin(t * 2));
        private Vector2 Position(Vector2 p) => rectTransform.rect.min + Vector2.Scale(p, rectTransform.rect.size);
        private void Triangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c)
        {
            int k = vh.currentVertCount; vh.AddVert(Position(a),color,Vector2.zero); vh.AddVert(Position(b),color,Vector2.zero); vh.AddVert(Position(c),color,Vector2.zero); vh.AddTriangle(k,k+1,k+2);
        }
        private void Line(VertexHelper vh, Vector2 a, Vector2 b, float weight = .075f)
        {
            Vector2 n = new Vector2(-(b-a).y,(b-a).x).normalized * weight * .5f;
            int k = vh.currentVertCount;
            vh.AddVert(Position(a-n),color,Vector2.zero); vh.AddVert(Position(a+n),color,Vector2.zero);
            vh.AddVert(Position(b+n),color,Vector2.zero); vh.AddVert(Position(b-n),color,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2); vh.AddTriangle(k,k+2,k+3);
        }
        private void Path(VertexHelper vh, params float[] coordinates)
        { for (int i = 2; i < coordinates.Length; i += 2) Line(vh,P(coordinates[i-2],coordinates[i-1]),P(coordinates[i],coordinates[i+1])); }
        private void Ring(VertexHelper vh, float x, float y, float radius, float from, float to)
        {
            Vector2 last = P(x + Mathf.Cos(from)*radius,y + Mathf.Sin(from)*radius);
            for (int i = 1; i <= 32; i++) { float a = Mathf.Lerp(from,to,i/32f); var next = P(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius); Line(vh,last,next); last=next; }
        }
    }
}
