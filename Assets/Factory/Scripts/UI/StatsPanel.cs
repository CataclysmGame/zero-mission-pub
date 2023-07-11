using UnityEngine;
using UnityEngine.UI;

namespace Factory.Scripts.UI
{
    public class StatsPanel : MonoBehaviour
    {
        
        public Image[] healthStars;
        public Image[] munitionsStars;
        public Image[] damageStars;
        public Image[] speedStars;

        public Texture2D emptyStar;
        public Texture2D fullStar;

        private Sprite _emtpyStarSprite;
        private Sprite _fullStarSprite;

        private void Awake()
        {
            // Create sprites from textures
            _emtpyStarSprite = Sprite.Create(emptyStar, new Rect(0, 0, emptyStar.width, emptyStar.height),
                new Vector2(0.5f, 0.5f));
            _fullStarSprite = Sprite.Create(fullStar, new Rect(0, 0, fullStar.width, fullStar.height),
                new Vector2(0.5f, 0.5f));
        }

        public void SetCharacter(Character character)
        {
            //TODO: add variables
            var hpRatio = Mathf.Clamp01(((character.maxHp - 100) / 50.0f) / 5.0f);
            var munitionsRatio = Mathf.Clamp01(((character.maxMunitions - 1) / 2.0f) / 5.0f);
            var damageRatio = Mathf.Clamp01(Mathf.FloorToInt((character.damage - 11.5f) / 2.5f) / 5.0f);
            var speedRatio = Mathf.Clamp01((character.movementSpeed - 3.0f) * 2.0f / 5.0f);

            SetStars(healthStars, hpRatio);
            SetStars(munitionsStars, munitionsRatio);
            SetStars(damageStars, damageRatio);
            SetStars(speedStars, speedRatio);
        }

        private void SetStars(Image[] stars, float ratio)
        {
            var fullStars = Mathf.FloorToInt(ratio * stars.Length);
            for (var i = 0; i < stars.Length; i++)
            {
                stars[i].sprite = i < fullStars ? _fullStarSprite : _emtpyStarSprite;
            }
        }
    }
}