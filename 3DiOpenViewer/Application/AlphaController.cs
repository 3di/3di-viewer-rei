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

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenViewer
{
    public class AlphaController
    {
        private const float MAX = 255;

        #region Element.
        public event EventHandler OnEnd;

        public float Start { get; set; }
        public float End { get; set; }
        public float Speed { get; set; }
        public bool IsEnd { get { return isEnd; } }

        private float value;
        private float vector;
        private bool isEnd;
        #endregion

        #region Public function.
        public AlphaController()
            : this(0, 255, 1) { }

        public AlphaController(float _start, float _end, float _speed)
        {
            Set(_start, _end, _speed);
        }

        public void Update()
        {
            if (isEnd)
                return;

            value += vector * Speed;

            if ((vector > 0 && value >= End))
            {
                value = End;
                isEnd = true;

                if (OnEnd != null)
                    OnEnd(this, EventArgs.Empty);
            }

            if ((vector < 0 && value <= End))
            {
                value = End;
                isEnd = true;

                if (OnEnd != null)
                    OnEnd(this, EventArgs.Empty);
            }
        }

        public void Set(float _start, float _end, float _speed)
        {
            value = _start;
            isEnd = false;

            Start = _start;
            End = _end;
            Speed = _speed;

            CalcVector();
        }

        public float Value { get { return (value / MAX); } }
        public float Value255 { get { return value; } }
        #endregion

        #region Private function.
        private void CalcVector()
        {
            vector = (End > Start) ? 1 : -1;
        }
        #endregion
    }
}
