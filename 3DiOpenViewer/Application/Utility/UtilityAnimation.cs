/*
 * Copyright (c) 2008-2009, 3Di, Inc. (http://3di.jp/) and contributors.
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of 3Di, Inc., nor the name of the 3Di Viewer
 *       "Rei" project, nor the names of its contributors may be used to
 *       endorse or promote products derived from this software without
 *       specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY 3Di, Inc. AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 3Di, Inc. OR THE
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using OpenMetaverse;
using System.Collections.Generic;

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

        #region Custom animation keys
        public static readonly List<UUID> CUSTOM_ANIMATIONS = new List<UUID>
        {
            new UUID("{C5829C0B-B82C-4f3d-9475-0826D48E5DB8}"),
            new UUID("{C006EBC3-A40D-4a7d-B24D-A8323A198DF2}"),
            new UUID("{046903EF-9358-45e1-BBDF-433CE99D3366}"),
            new UUID("{290FE528-5128-4451-B9A7-39E761D8F60F}"),
            new UUID("{43DEFB09-3996-41d2-ACBC-E1E217111396}"),
            new UUID("{3BF9354B-8D8F-4d74-AB1B-98B867101285}"),
            new UUID("{7B5960F3-5634-4e97-8225-E5DFCFC78654}"),
            new UUID("{CF5A0D1D-8CCC-48ba-9D3F-81A4A50A7D11}"),
            new UUID("{0F56F522-AB3F-44ae-B3A1-9425CF47DF7E}"),
            new UUID("{2A356685-63C5-4454-9AC9-BAB87E37DA5A}"),
            new UUID("{84B2F1B0-534C-4c51-82C0-0917BEA3C673}"),
            new UUID("{20C1A61A-BC42-4202-9A53-9976BE26545E}"),
            new UUID("{F7B995EA-9F96-4b7c-9C32-15BB50A17F73}"),
            new UUID("{83A1870B-BAB5-4c2c-BAC9-558C7E22D0A9}"),
            new UUID("{228B2569-8AD1-42f6-9C86-78DF237F0A86}"),
            new UUID("{A377FEBB-2732-4e77-952D-A2F7326D6539}"),
            new UUID("{81563482-4C13-4063-8986-1473D8AD2235}"),
            new UUID("{2BA7296F-9F84-43fe-B078-C79047CF3085}"),
            new UUID("{11D022B0-3851-4d77-BB85-08DFBBFC3BD4}"),
            new UUID("{3DDFE90E-A50A-454d-8B87-5B48AB20E29D}"),
            new UUID("{E980C815-0CAC-4ec4-9C19-3071085C4804}")
        };
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
