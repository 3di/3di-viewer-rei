using OpenMetaverse;

namespace OpenViewer.Utility
{
    public static class UtilityAnimation
    {
        #region Animation key label
        public const string ANIMATION_KEY_STANDING = "standing";
        public const string ANIMATION_KEY_CROUCHWALK = "crouchwalk";
        public const string ANIMATION_KEY_WALKING = "walking";
        public const string ANIMATION_KEY_RUNNING = "running";
        public const string ANIMATION_KEY_SITSTART = "sitstart";
        public const string ANIMATION_KEY_STANDUP = "standup";
        public const string ANIMATION_KEY_FLYING = "flying";
        public const string ANIMATION_KEY_HOVER = "hover";
        public const string ANIMATION_KEY_CROUCHING = "crouching";
        public const string ANIMATION_KEY_SITTING = "sitting";

        public const string ANIMATION_KEY_SPEAK_STANDING = "standing_speak";
        public const string ANIMATION_KEY_SPEAK_STANDING_END = "standing_speak_end";
        public const string ANIMATION_KEY_SPEAK_SITTING = "sitting_speak";
        public const string ANIMATION_KEY_SPEAK_SITTING_END = "sitting_speak_end";

        #endregion

        #region Customize animation key
        public static readonly UUID CUSTOMIZE_ANIM_00 = new UUID("{C5829C0B-B82C-4f3d-9475-0826D48E5DB8}");
        public static readonly UUID CUSTOMIZE_ANIM_01 = new UUID("{C006EBC3-A40D-4a7d-B24D-A8323A198DF2}");
        public static readonly UUID CUSTOMIZE_ANIM_02 = new UUID("{046903EF-9358-45e1-BBDF-433CE99D3366}");
        public static readonly UUID CUSTOMIZE_ANIM_03 = new UUID("{290FE528-5128-4451-B9A7-39E761D8F60F}");
        public static readonly UUID CUSTOMIZE_ANIM_04 = new UUID("{43DEFB09-3996-41d2-ACBC-E1E217111396}");
        public static readonly UUID CUSTOMIZE_ANIM_05 = new UUID("{3BF9354B-8D8F-4d74-AB1B-98B867101285}");
        public static readonly UUID CUSTOMIZE_ANIM_06 = new UUID("{7B5960F3-5634-4e97-8225-E5DFCFC78654}");
        public static readonly UUID CUSTOMIZE_ANIM_07 = new UUID("{CF5A0D1D-8CCC-48ba-9D3F-81A4A50A7D11}");
        public static readonly UUID CUSTOMIZE_ANIM_08 = new UUID("{0F56F522-AB3F-44ae-B3A1-9425CF47DF7E}");
        public static readonly UUID CUSTOMIZE_ANIM_09 = new UUID("{2A356685-63C5-4454-9AC9-BAB87E37DA5A}");
        public static readonly UUID CUSTOMIZE_ANIM_10 = new UUID("{84B2F1B0-534C-4c51-82C0-0917BEA3C673}");
        public static readonly UUID CUSTOMIZE_ANIM_11 = new UUID("{20C1A61A-BC42-4202-9A53-9976BE26545E}");
        public static readonly UUID CUSTOMIZE_ANIM_12 = new UUID("{F7B995EA-9F96-4b7c-9C32-15BB50A17F73}");
        public static readonly UUID CUSTOMIZE_ANIM_13 = new UUID("{83A1870B-BAB5-4c2c-BAC9-558C7E22D0A9}");
        public static readonly UUID CUSTOMIZE_ANIM_14 = new UUID("{228B2569-8AD1-42f6-9C86-78DF237F0A86}");
        public static readonly UUID CUSTOMIZE_ANIM_15 = new UUID("{A377FEBB-2732-4e77-952D-A2F7326D6539}");
        public static readonly UUID CUSTOMIZE_ANIM_16 = new UUID("{81563482-4C13-4063-8986-1473D8AD2235}");
        public static readonly UUID CUSTOMIZE_ANIM_17 = new UUID("{2BA7296F-9F84-43fe-B078-C79047CF3085}");
        public static readonly UUID CUSTOMIZE_ANIM_18 = new UUID("{11D022B0-3851-4d77-BB85-08DFBBFC3BD4}");
        public static readonly UUID CUSTOMIZE_ANIM_19 = new UUID("{3DDFE90E-A50A-454d-8B87-5B48AB20E29D}");
        public static readonly UUID CUSTOMIZE_ANIM_20 = new UUID("{E980C815-0CAC-4ec4-9C19-3071085C4804}");
        #endregion

        #region GetAnimationKeyFromAnimationUUID
        public static string GetAnimationKeyFromAnimationUUID(UUID animationUUID)
        {
            string key = string.Empty;

            if (animationUUID == Animations.STAND
                || animationUUID == Animations.STAND_1
                || animationUUID == Animations.STAND_2
                || animationUUID == Animations.STAND_3
                || animationUUID == Animations.STAND_4)
            {
                key = ANIMATION_KEY_STANDING;
            }

            else if (animationUUID == Animations.CROUCHWALK)
            {
                key = ANIMATION_KEY_CROUCHWALK;
            }

            else if (animationUUID == Animations.WALK
                || animationUUID == Animations.FEMALE_WALK)
            {
                key = ANIMATION_KEY_WALKING;
            }

            else if (animationUUID == Animations.RUN)
            {
                key = ANIMATION_KEY_RUNNING;
            }

            else if (animationUUID == Animations.SIT
                || animationUUID == Animations.SIT_FEMALE
                || animationUUID == Animations.SIT_GENERIC
                || animationUUID == Animations.SIT_GROUND
                || animationUUID == Animations.SIT_GROUND_staticRAINED)
            {
                key = ANIMATION_KEY_SITSTART;
            }

            else if (animationUUID == Animations.STANDUP
                || animationUUID == Animations.SIT_TO_STAND)
            {
                key = ANIMATION_KEY_STANDUP;
            }

            else if (animationUUID == Animations.FLY
                || animationUUID == Animations.FLYSLOW)
            {
                key = ANIMATION_KEY_FLYING;
            }

            else if (animationUUID == Animations.HOVER
                || animationUUID == Animations.HOVER_DOWN
                || animationUUID == Animations.HOVER_UP)
            {
                key = ANIMATION_KEY_HOVER;
            }

            else if (animationUUID == Animations.CROUCH)
            {
                key = ANIMATION_KEY_CROUCHING;
            }

            return key;
        }
        #endregion

        public static bool IsPossibleVoiceAnimation(string _animationKey)
        {
            switch (_animationKey)
            {
                case ANIMATION_KEY_STANDING:
                case ANIMATION_KEY_SITTING:
                case ANIMATION_KEY_SPEAK_STANDING:
                case ANIMATION_KEY_SPEAK_STANDING_END:
                case ANIMATION_KEY_SPEAK_SITTING:
                case ANIMATION_KEY_SPEAK_SITTING_END:
                    return true;
            }

            return false;
        }
    }
}
