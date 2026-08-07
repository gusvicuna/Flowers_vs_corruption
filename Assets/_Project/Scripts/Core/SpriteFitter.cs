using UnityEngine;

namespace FlowersVsCorruption.Core
{
    public static class SpriteFitter
    {
        /// <summary>
        /// Scales the renderer's transform so its sprite covers exactly
        /// width x height world units, whatever the sprite's pixels-per-unit is.
        /// </summary>
        public static void Fit(SpriteRenderer renderer, float width, float height)
        {
            if (renderer == null || renderer.sprite == null)
                return;

            Vector3 spriteSize = renderer.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            renderer.transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
        }
    }
}
