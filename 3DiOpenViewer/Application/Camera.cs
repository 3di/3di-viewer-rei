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
 * 
 * Additionally, portions of this file bear the following BSD-style license
 * from the IdealistViewer project (URL http://idealistviewer.org/):
 * 
 * Copyright (c) Contributors, http://idealistviewer.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenViewer Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using IrrlichtNETCP;

namespace OpenViewer
{
    public class Camera : BaseComponent
    {
        public CameraSceneNode SNCamera;
        public VObject VOtarget;
        private float CAMERASPEED = 0.0175f;
        private float CAMERAZOOMSPEED = 5f;
        private float CAMERAWHEELSPEED = 1.0f;
        private float TWOPI = (float)(Math.PI * 2);
        public float loMouseOffsetPHI = 0;
        public float loMouseOffsetTHETA = 0;
        public Vector3D FollowCamTargetOffset = new Vector3D(0, 1f, 0);

        public ECameraMode CameraMode = ECameraMode.Build;
        public bool SmoothingReset = false;


        private static Vector3 m_lastTargetPos = Vector3.Zero;
        private Vector3[] LookAtCam = new Vector3[3];
        private float nowDistance;
        private Vector3D targetPosition;
        private Vector3D targetTarget;

        /// <summary>
        /// User's Camera
        /// </summary>
        /// <param name="psmgr">Scene Manager</param>
        public Camera(Viewer _viewer)
            : base (_viewer, -1)
        {
            //
            VOtarget = null;
            SNCamera = Reference.SceneManager.AddCameraSceneNode(Reference.SceneManager.RootSceneNode);
            SNCamera.Position = new Vector3D(0f, 235f, -44.70f);
            SNCamera.Target = new Vector3D(-273, -255.3f, 407.3f);
            SNCamera.FarValue = 12800;
            SNCamera.FOV = Reference.Viewer.CameraFOV;
            SNCamera.Name = "Camera";

            LookAtCam[0] = Vector3.Zero;
            LookAtCam[1] = Vector3.Zero;
            LookAtCam[2] = Vector3.Zero;

            nowDistance = Reference.Viewer.CameraDistance;

            UpdateCameraPosition();
        
        }

        //LibOMV camera position
        public Vector3 Position
        {
            get { return new Vector3(SNCamera.Position.X,SNCamera.Position.Y,SNCamera.Position.Z); }
        }


        public IrrlichtNETCP.Vector3D CalculateSphericalCameraCoordinates(IrrlichtNETCP.Vector3D camPos, 
            IrrlichtNETCP.Vector3D targetPos)
        {
            float xCamRotationAnglePHI, xCamRotationAngleTHETA;
            IrrlichtNETCP.Vector3D result = new IrrlichtNETCP.Vector3D(); // result is spherical coordinates as a 3D vector in (radius, theta, phi) order

            IrrlichtNETCP.Vector3D relCamPos = camPos - targetPos;
            float camDistance = (float)Math.Sqrt(relCamPos.DotProduct(relCamPos));
            xCamRotationAnglePHI = 0.5f * (float)Math.PI - (float)Math.Asin((double)(relCamPos.Y / camDistance));
            if (xCamRotationAnglePHI < 0.0)
            {
                xCamRotationAnglePHI += 2.0f * (float)Math.PI;
            }

            if (Math.Abs(relCamPos.X) > 0.0)
            {
                xCamRotationAngleTHETA = (float)Math.Atan((double)(relCamPos.Z / relCamPos.X));
                if (relCamPos.X < 0.0f)
                {
                    xCamRotationAngleTHETA += (float)Math.PI;
                }
                if (xCamRotationAngleTHETA < 0.0)
                {
                    xCamRotationAngleTHETA += 2.0f * (float)Math.PI;
                }
            }
            else
            {
                if (relCamPos.Z > 0)
                {
                    xCamRotationAngleTHETA = 0.5f * (float)Math.PI; // 90 deg
                }
                else
                {
                    xCamRotationAngleTHETA = 1.5f * (float)Math.PI; // 270 deg
                }
            }
            result.Set(camDistance, xCamRotationAngleTHETA, xCamRotationAnglePHI);
            return result;
        }

        public override void Update(uint frame)
        {
            float amount = 0.25f;

            // Auto adjust distance.
            if (Math.Abs(Reference.Viewer.CameraDistance - nowDistance) > 0.1f)
            {
                nowDistance = Utils.Lerp(nowDistance, Reference.Viewer.CameraDistance, amount);
            }

            // Auto camera target, position.
            amount = 0.4f;
            amount = Utils.Lerp(amount, 1, 1 - Reference.Viewer.FPSRate);

            if (SmoothingReset)
            {
                amount = 1.0f;
                SmoothingReset = false;
            }

            if (SNCamera != null)
            {
                if (SNCamera.Position.DistanceFrom(targetPosition) > 0.1f)
                {
                    SNCamera.Position = Util.Lerp(SNCamera.Position, targetPosition, amount);
                }

                if (SNCamera.Target.DistanceFrom(targetTarget) > 0.1f)
                {
                    SNCamera.Target = Util.Lerp(SNCamera.Target, targetTarget, amount);
                }

                SNCamera.UpdateAbsolutePosition();
            }

            base.Update(frame);
        }

        /// <summary>
        /// Update camera position based on it's current PHI, Theta, and mouse offset.
        /// </summary>
        public void UpdateCameraPosition()
        {
            Vector3D newpos = new Vector3D();
            Vector3D oldTarget = targetTarget;
            switch (CameraMode)
            {
                case ECameraMode.ThirdWithArbitraryOffset:
                case ECameraMode.Build:
                    {
                        newpos.X = oldTarget.X + (nowDistance * (float)Math.Cos(Reference.Viewer.CamRotationAngleTHETA + loMouseOffsetTHETA) * (float)Math.Sin(Reference.Viewer.CamRotationAnglePHI + loMouseOffsetPHI));
                        newpos.Y = oldTarget.Y + nowDistance * (float)Math.Cos(Reference.Viewer.CamRotationAnglePHI + loMouseOffsetPHI);
                        newpos.Z = oldTarget.Z + nowDistance * (float)Math.Sin(Reference.Viewer.CamRotationAngleTHETA + loMouseOffsetTHETA) * (float)Math.Sin(Reference.Viewer.CamRotationAnglePHI + loMouseOffsetPHI);

                        targetPosition = newpos;
                        targetTarget = oldTarget;
                    }

                    break;

                case ECameraMode.Third:
                    if (VOtarget != null)
                    {
                        // in 3rd person camera mode, default setting is 5 meter distance, 10 degree phi elevation, behind avatar.
                        // the "behind" avatar part is tricky. we need to calculate the heading (theta) of the avatar in spherical
                        // cam coordinates and use that theta to compute the final camera position.

                        nowDistance = Reference.Viewer.CameraKeyWalkingDistance;
                        Reference.Viewer.CamRotationAnglePHI = ((90.0f - 10.0f) / 180.0f) * (float)Math.PI; // 10 degree elevation. 0 deg=straight overhead

                        // calculate avatar heading based on its prim's orientation.
                        // this calculation here works, but is pretty confusing because of all the coordinate
                        // system transformations going on. This code should be cleaned up to be more
                        // understandable when there is time. In particular, we need to consider the coordinate
                        // system of the world, the spherical camera coordinate system, and the avatar's orientation.

                        OpenMetaverse.Quaternion primRot = VOtarget.Prim.Rotation;
                        IrrlichtNETCP.Quaternion q = new IrrlichtNETCP.Quaternion(primRot.X, primRot.Y, primRot.Z, primRot.W);
                        IrrlichtNETCP.Matrix4 avMatrix = q.Matrix; 
                        Vector3D heading = new Vector3D(avMatrix.GetM(1, 0), avMatrix.GetM(1, 1), avMatrix.GetM(1, 2));
                        heading.Y *= -1.0f; // convert avatar coords to IV cam spherical coords

                        float headingAngle;
                        // calculate heading (theta) in IV cam spherical coords
                        if (Math.Abs(heading.X) > 0.0)
                        {
                            headingAngle = (float)Math.Atan((double)(heading.Y / heading.X));
                            if (heading.X < 0.0f)
                            {
                                headingAngle += (float)Math.PI;
                            }
                            if (headingAngle < 0.0)
                            {
                                headingAngle += 2.0f * (float)Math.PI;
                            }
                        }
                        else
                        {
                            if (heading.Y > 0)
                            {
                                headingAngle = 0.5f * (float)Math.PI; // 90 deg
                            }
                            else
                            {
                                headingAngle = 1.5f * (float)Math.PI; // 270 deg
                            }
                        }

                        //Console.WriteLine("avatar heading seems to be " + heading + " angle " + headingAngle * 360.0f / (2.0f*(float)Math.PI));
                        //Console.WriteLine("avatar matrix seems to be " + avMatrix);
                        Reference.Viewer.CamRotationAngleTHETA = headingAngle - 0.5f * (float)Math.PI; // position camera behind avatar. This calculation is a little strange, but it works; no time to make it cleaner right now.

                        {
                            newpos.X = oldTarget.X + (nowDistance * (float)Math.Cos(Reference.Viewer.CamRotationAngleTHETA + loMouseOffsetTHETA) * (float)Math.Sin(Reference.Viewer.CamDefaultRotationAnglePHI + loMouseOffsetPHI));
                            newpos.Y = oldTarget.Y + nowDistance * (float)Math.Cos(Reference.Viewer.CamDefaultRotationAnglePHI + loMouseOffsetPHI);
                            newpos.Z = oldTarget.Z + nowDistance * (float)Math.Sin(Reference.Viewer.CamRotationAngleTHETA + loMouseOffsetTHETA) * (float)Math.Sin(Reference.Viewer.CamDefaultRotationAnglePHI + loMouseOffsetPHI);

                            targetPosition = newpos;
                            targetTarget = oldTarget;
                        }
                    }
                    break;

            }
        }

        /// <summary>
        /// Key handler for camera actions
        /// </summary>
        /// <param name="key"></param>
        public void DoKeyAction(KeyCode key)
        {
            bool isUpdate = false;

            switch (key)
            {
                case KeyCode.Up:
                    Reference.Viewer.CameraDistance -= (CAMERAZOOMSPEED - ((Reference.Viewer.CameraMinDistance - Reference.Viewer.CameraDistance) / Reference.Viewer.CameraDistance));
                    isUpdate = true;
                    break;

                case KeyCode.Down:
                    Reference.Viewer.CameraDistance += CAMERAZOOMSPEED - ((Reference.Viewer.CameraMinDistance - Reference.Viewer.CameraDistance) / Reference.Viewer.CameraDistance);
                    isUpdate = true;
                    break;

                case KeyCode.Left:
                    Reference.Viewer.CamRotationAngleTHETA -= CAMERASPEED;
                    while (Reference.Viewer.CamRotationAngleTHETA < TWOPI)
                        Reference.Viewer.CamRotationAngleTHETA += TWOPI;

                    isUpdate = true;
                    break;

                case KeyCode.Right:
                    Reference.Viewer.CamRotationAngleTHETA += CAMERASPEED;
                    while (Reference.Viewer.CamRotationAngleTHETA > TWOPI)
                        Reference.Viewer.CamRotationAngleTHETA -= TWOPI;

                    isUpdate = true;
                    break;

                    // Page Down
                case KeyCode.Next:

                    Reference.Viewer.CamRotationAnglePHI -= CAMERASPEED;
                    while (Reference.Viewer.CamRotationAnglePHI < TWOPI)
                        Reference.Viewer.CamRotationAnglePHI += TWOPI;

                    isUpdate = true;
                    break;

                    // Page Up
                case KeyCode.Prior:
                    //vOrbit.Y += 2f;
                    Reference.Viewer.CamRotationAnglePHI += CAMERASPEED;
                    while (Reference.Viewer.CamRotationAnglePHI > TWOPI)
                        Reference.Viewer.CamRotationAnglePHI -= TWOPI;

                    isUpdate = true;
                    break;

            }

            if (isUpdate)
            {
                Reference.Viewer.CameraDistance = Util.Clamp<float>(Reference.Viewer.CameraDistance, Reference.Viewer.CameraMinDistance, Reference.Viewer.CameraMaxDistance);
                UpdateCameraPosition();
            }
        }

        /// <summary>
        /// Mouse Offset Reset.  This usually gets called after applying the 
        /// values to the actual camera phi and theta first
        /// </summary>
        public void ResetMouseOffsets()
        {
            loMouseOffsetPHI = 0;
            loMouseOffsetTHETA = 0;
        }

        /// <summary>
        /// Applies offset PHI and THETA to the camera's values.  
        /// Usually ResetMouseOffsets gets called immediately after this
        /// or you get a double effect.
        /// </summary>
        public void ApplyMouseOffsets()
        {
            if (loMouseOffsetPHI != 0 || loMouseOffsetTHETA != 0)
            {
                Reference.Viewer.CamRotationAnglePHI = Reference.Viewer.CamRotationAnglePHI + loMouseOffsetPHI;
                Reference.Viewer.CamRotationAngleTHETA = Reference.Viewer.CamRotationAngleTHETA + loMouseOffsetTHETA;
                loMouseOffsetPHI = 0;
                loMouseOffsetTHETA = 0;
                UpdateCameraPosition();
            }
        }

        /// <summary>
        /// Action based on mousewheel change
        /// </summary>
        /// <param name="delta"></param>
        public void MouseWheelAction(float delta)
        {
            Reference.Viewer.CameraDistance += ((CAMERAZOOMSPEED - 4.8f) + ((Reference.Viewer.CameraDistance / Reference.Viewer.CameraMaxDistance) * 2.5f)) * ((-delta * CAMERAWHEELSPEED));
            Reference.Viewer.CameraDistance = Util.Clamp<float>(Reference.Viewer.CameraDistance, Reference.Viewer.CameraMinDistance, Reference.Viewer.CameraMaxDistance);
            UpdateCameraPosition();
        }

        /// <summary>
        /// Set Camera target to a Position
        /// </summary>
        /// <param name="ptarget"></param>
        private void SetTarget(Vector3D ptarget)
        {
            if (CameraMode == ECameraMode.First)
                return;

            targetTarget = ptarget;
            UpdateCameraPosition();
        }

        /// <summary>
        /// Set Target to a VObject (for tracking)
        /// </summary>
        /// <param name="pTarget"></param>
        public void SetTarget(VObject pTarget)
        {
            if (pTarget == null)
                return;

            if (pTarget.Prim != null)
            {
                //Vector3D target = new Vector3D(pTarget.Prim.Position.X + pTarget.ParentPosition.X, pTarget.Prim.Position.Z + pTarget.ParentPosition.Z + Reference.Viewer.CameraOffsetY, pTarget.Prim.Position.Y + pTarget.ParentPosition.Y);
                //Vector3D target = pTarget.Node.Position + new Vector3D( pTarget.ParentPosition.X, pTarget.ParentPosition.Z + Reference.Viewer.CameraOffsetY, pTarget.ParentPosition.Y);
                Vector3D target = pTarget.Node.Position + new Vector3D(0, Reference.Viewer.CameraOffsetY, 0);

                // note: prim is in libomv coords, but settarget is in irr coords, so swap y and z
                SetTarget(target);
                VOtarget = pTarget;
            }
        }

        public void SetFOV(float _fov)
        {
            float maxDegreeValue = 180;
            _fov = Util.Clamp<float>(_fov, 0.00001f, (maxDegreeValue / 180.0f * (float)Math.PI));
            Reference.Viewer.CameraFOV = _fov;

            lock (SNCamera)
            {
                SNCamera.FOV = _fov;
            }
        }

        /// <summary>
        /// Prepare Camera LookAT for LibOMV
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetCameraLookAt()
        {
            IrrlichtNETCP.Matrix4 viewm = SNCamera.ViewMatrix;
            IrrlichtNETCP.Matrix4 transform = Viewer.Coordinate_XYZ_XZY.Matrix;
            transform.MakeInverse();
            viewm = viewm * transform;

            LookAtCam[0].X = viewm.M[0];
            LookAtCam[0].Y = viewm.M[2];
            LookAtCam[0].Z = viewm.M[1];
            LookAtCam[1].X = viewm.M[4];
            LookAtCam[1].Y = viewm.M[6];
            LookAtCam[1].Z = viewm.M[5];
            LookAtCam[2].Z = viewm.M[8];
            LookAtCam[2].Z = viewm.M[10];
            LookAtCam[2].Z = viewm.M[9];
            return LookAtCam;
        }

        /// <summary>
        /// Used for Orbiting the Camera based on the mouse
        /// </summary>
        /// <param name="deltaX">Mouse change X (pixels)</param>
        /// <param name="deltaY">Mouse change Y (pixels)</param>

        public void SetDeltaFromMouse(float deltaX, float deltaY)
        {
            float testPHI;
            testPHI = Reference.Viewer.CamRotationAnglePHI + ((deltaY * CAMERASPEED) * 0.2f);
            // convert testPHI to be in range of 0..2PI 
            while(testPHI <0)
            {
                testPHI += 2.0f * (float)Math.PI;
            }
            
            if (testPHI > Reference.Viewer.CameraMinAngleY
                && testPHI < Reference.Viewer.CameraMaxAngleY)
            {
                Reference.Viewer.CamRotationAnglePHI = Reference.Viewer.CamRotationAnglePHI + ((deltaY * CAMERASPEED) * 0.2f);
            }
            Reference.Viewer.CamRotationAngleTHETA = Reference.Viewer.CamRotationAngleTHETA + ((-deltaX * CAMERASPEED) * 0.2f);

            UpdateCameraPosition();
        }

        /// <summary>
        /// Translate the mouse position on the screen into a ray in 3D space
        /// WARNING: cam.ProjectRayPoints seems buggy, use irrlicht CollisionManager.GetSceneNodeFromRay instead
        /// </summary>
        /// <param name="mpos"></param>
        /// <param name="WindowWidth_DIV2"></param>
        /// <param name="WindowHeight_DIV2"></param>
        /// <param name="aspect"></param>
        /// <returns></returns>
        public Vector3D[] ProjectRayPoints(Position2D mpos, float WindowWidth_DIV2, float WindowHeight_DIV2, float aspect)
        {

            Vector3 pos = Vector3.Zero;
            pos.X = (float)(Math.Tan(SNCamera.FOV * 0.5f) * (mpos.X / WindowWidth_DIV2 - 1.0f));
            pos.Y = (float)(Math.Tan(SNCamera.FOV * 0.5f) * (1.0f - mpos.Y / WindowHeight_DIV2) / aspect);

            Vector3D p1 = new Vector3D(pos.X * SNCamera.NearValue, pos.Y * SNCamera.NearValue, SNCamera.NearValue);
            Vector3D p2 = new Vector3D(pos.X * SNCamera.FarValue, pos.Y * SNCamera.FarValue, SNCamera.FarValue);

            // Inverse the view matrix
            IrrlichtNETCP.Matrix4 viewm = SNCamera.ViewMatrix;
            viewm.MakeInverse();

            p1 = viewm.TransformVect(ref p1);
            p2 = viewm.TransformVect(ref p2);
            //m_log.DebugFormat("Ray: <{0},{1},{2}>, <{3},{4},{5}>", p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z);
            Vector3D[] returnvectors = new Vector3D[2];
            returnvectors[0] = p1;
            returnvectors[1] = p2;
            return returnvectors;
        }

        public void SwitchMode(ECameraMode pNewMode)
        {
            if (pNewMode == ECameraMode.Build)
            {
                if (CameraMode == ECameraMode.First)
                {
                    pNewMode = ECameraMode.Third;
                }
                else
                {
                    IrrlichtNETCP.Vector3D camSphericalCoordinates;
                    camSphericalCoordinates = CalculateSphericalCameraCoordinates(SNCamera.AbsolutePosition, SNCamera.Target);
                    Reference.Viewer.CameraDistance = camSphericalCoordinates.X;
                    Reference.Viewer.CamRotationAngleTHETA = camSphericalCoordinates.Y;
                    Reference.Viewer.CamRotationAnglePHI = camSphericalCoordinates.Z;
                }
            }
            CameraMode = pNewMode;
            UpdateCameraPosition();
        }

        #region Move function.
        public enum MoveSpeedType
        {
            Fast,
            Normal,
            Slow,
        }

        private MoveSpeedType moveSpeedMode = MoveSpeedType.Normal;
        public MoveSpeedType MoveSpeed { get { return moveSpeedMode; } }
        private float turnSpeed = 0.1f;
        private float moveSpeed = 0.25f;
        private Vector3D movePosition = new Vector3D();
        private Vector3D moveTarget = new Vector3D();

        public void MoveLookAt(float _px, float _py, float _pz, float _tx, float _ty, float _tz)
        {
            MoveModeChange();

            targetPosition = new Vector3D(_px, _py, _pz);
            targetTarget = new Vector3D(_tx, _ty, _tz);
        }

        public void MoveUp()
        {
            MoveModeChange();

            targetPosition = targetPosition + new Vector3D(0, moveSpeed, 0);
            targetTarget = targetTarget + new Vector3D(0, moveSpeed, 0);
        }

        public void MoveDown()
        {
            MoveModeChange();

            targetPosition = targetPosition + new Vector3D(0, -moveSpeed, 0);
            targetTarget = targetTarget + new Vector3D(0, -moveSpeed, 0);
        }

        public void MoveForward()
        {
            MoveModeChange();

            Vector3D front = GetFrontVector() * moveSpeed;

            targetPosition = targetPosition + front;
            targetTarget = targetTarget + front;
        }

        public void MoveBackward()
        {
            MoveModeChange();

            Vector3D front = GetFrontVector() * -moveSpeed;

            targetPosition = targetPosition + front;
            targetTarget = targetTarget + front;
        }

        public void MoveLeft()
        {
            MoveModeChange();

            Vector3D front = GetFrontVector();
            Vector3D left = front.CrossProduct(new Vector3D(0, 1, 0));

            left *= moveSpeed;

            targetPosition = targetPosition + left;
            targetTarget = targetTarget + left;
        }

        public void MoveRight()
        {
            MoveModeChange();

            Vector3D front = GetFrontVector();
            Vector3D left = front.CrossProduct(new Vector3D(0, 1, 0));

            left *= -moveSpeed;

            targetPosition = targetPosition + left;
            targetTarget = targetTarget + left;
        }

        public void MoveTurnLeft()
        {
            MoveModeChange();

            IrrlichtNETCP.Matrix4 mat = IrrlichtNETCP.Matrix4.Identity;
            mat.RotationRadian = new Vector3D(0, -turnSpeed, 0);

            Vector3D front = GetFrontVector();
            Vector3D tar = mat.TransformVect(ref front);

            targetTarget = targetPosition + tar;
        }

        public void MoveTurnRight()
        {
            MoveModeChange();

            IrrlichtNETCP.Matrix4 mat = IrrlichtNETCP.Matrix4.Identity;
            mat.RotationRadian = new Vector3D(0, turnSpeed, 0);

            Vector3D front = GetFrontVector();
            Vector3D tar = mat.TransformVect(ref front);

            targetTarget = targetPosition + tar;
        }

        public void MoveSwitchSpeed()
        {
            MoveModeChange();

            if (moveSpeedMode == MoveSpeedType.Slow)
            {
                moveSpeedMode = MoveSpeedType.Fast;
            }
            else
            {
                moveSpeedMode++;
            }


            switch (moveSpeedMode)
            {
                case MoveSpeedType.Fast:
                    turnSpeed = 0.2f;
                    moveSpeed = 2.5f;
                    break;

                case MoveSpeedType.Normal:
                    turnSpeed = 0.1f;
                    moveSpeed = 0.25f;
                    break;

                case MoveSpeedType.Slow:
                    turnSpeed = 0.01f;
                    moveSpeed = 0.025f;
                    break;
            }
        }

        public void MoveModeChange()
        {
            if (CameraMode == ECameraMode.First)
                return;

            if (SNCamera != null)
            {
                movePosition = SNCamera.Position;
                moveTarget = SNCamera.Target;

                CameraMode = ECameraMode.First;
            }
        }

        private Vector3D GetFrontVector()
        {
            Vector3D front = SNCamera.Target - SNCamera.Position;
            front.Y = 0;
            if (front.LengthSQ > 0)
                front = front.Normalize();

            return front;
        }
        #endregion
    }
    public enum ECameraMode : int
    {
        Build = 1,
        Third = 2, 
        First = 3,
        ThirdWithArbitraryOffset = 4
    }
}
