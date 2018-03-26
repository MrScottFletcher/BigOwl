﻿using BigOwl.Devices;
using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigOwl.OwlMasterController
{
    public class Owl : OwlControllerBase
    {

        public OwlCommandQueue CommandQueue { get; set; }


        private StepperControl.OwlStepperController _head;
        private StepperControl.OwlStepperController _wings;

        //private PwmControl.OwlPwmController _leftEye;
        //private PwmControl.OwlPwmController _rightEye;

        private ServoBoardDriver _pwmDriver;
        private ServoBoardDriver.ServoPort _leftEye;
        private ServoBoardDriver.ServoPort _rightEye;


        public List<OwlControllerBase> PartsList { get; set; }

        public Owl() : base(42)
        {
            CommandQueue = new OwlCommandQueue();
            PartsList = new List<OwlControllerBase>();
            this.LastError = String.Empty;

            //"Forward" is "righty-tighty" drive from the motor's perspective. Like a hand drill

            //===================================================
            //HEAD AND WINGS
            try
            {
                //Forward = Head moves "right", so we have a forward sensor on it.

                _head = new StepperControl.OwlStepperController("Head", 19, 26, 13, 6, 5, 21, 16, null, 5, false);
                _head.MinPosition = 0;
                _head.MaxPosition = 100;
                _head.HomePosition = 50;

                //Only for initial testing
                _head.CurrentPosition = 100;

                _head.DeviceError += _component_DeviceError;
                _head.MoveCompleted += _component_MoveCompleted;

                //one of these is not used
                _head.BackwardLimitSensorChanged += _head_BackwardLimitReached;
                _head.ForwardLimitSensorChanged += _head_ForwardLimitReached;

                PartsList.Add(_head);
            }
            catch (Exception exHead)
            {
                this.Status = OwlDeviceStateBase.StatusTypes.InError;
                this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
                this.LastError += exHead.ToString();
            }

            try
            {
                //Forward = Wings up
                //Screw drive travels 0.125 incehes per 100 steps.  
                //5" would be 4000 steps
                //Converting to position factor: 4000 / 100 = 40 steps per position increment
                _wings = new StepperControl.OwlStepperController("Wings", 17, 18, 22, 23, 24, 25, 12, 4, 40, true);
                _wings.MinPosition = 0;
                _wings.MaxPosition = 100;
                _wings.HomePosition = 95;

                //Only for initial testing
                _wings.CurrentPosition = 50;

                _wings.DeviceError += _component_DeviceError;
                _wings.MoveCompleted += _component_MoveCompleted;

                _wings.ForwardLimitSensorChanged += _wings_ForwardLimitReached;
                _wings.BackwardLimitSensorChanged += _wings_BackwardLimitReached;

                PartsList.Add(_wings);

            }
            catch (Exception exWings)
            {
                this.Status = OwlDeviceStateBase.StatusTypes.InError;
                this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
                this.LastError += exWings.ToString();
            }

            //===================================================

            try
            {
                //Uses the defaultI2C address
                _pwmDriver = new ServoBoardDriver();
                _pwmDriver.Initialize();
                try
                {
                    _leftEye = new ServoBoardDriver.ServoPort("Left Eye", 0, _pwmDriver._pca9685, false);
                    _rightEye = new ServoBoardDriver.ServoPort("Right Eye", 1, _pwmDriver._pca9685, true);
                    _leftEye.Initialize();
                    PartsList.Add(_leftEye);
                    _rightEye.Initialize();
                    PartsList.Add(_rightEye);
                }
                catch (Exception exEyes)
                {
                    this.Status = OwlDeviceStateBase.StatusTypes.InError;
                    this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
                    this.LastError += exEyes.ToString();
                }
            }
            catch (Exception exPWM)
            {
                this.Status = OwlDeviceStateBase.StatusTypes.InError;
                this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
                this.LastError += exPWM.ToString();
            }
        }

        private void _wings_BackwardLimitReached(object sender)
        {
            FireDeviceInfoMessage(sender.ToString() + " _wings_BackwardLimitReached");
        }

        private void _wings_ForwardLimitReached(object sender)
        {
            FireDeviceInfoMessage(sender.ToString() + " _wings_ForwardLimitReached");
        }

        private void _head_ForwardLimitReached(object sender)
        {
            FireDeviceInfoMessage(sender.ToString() + " _head_ForwardLimitReached");
        }

        private void _head_BackwardLimitReached(object sender)
        {
            FireDeviceInfoMessage(sender.ToString() + " _head_BackwardLimitReached");
        }

        public void RunCommand(OwlCommand c)
        {
            CommandQueue.Add(c);
            //Need to set up a queue event to fire this automatically.
            ProcessCommandQueue();
        }

        protected void ProcessCommandQueue()
        {
            Status = OwlDeviceStateBase.StatusTypes.Busy;
            StatusReason = OwlDeviceStateBase.StatusReasonTypes.ExecutingMove;

            while (CommandQueue.PeekNext() != null)
            {
                OwlCommand c = CommandQueue.GetNext();

                if (c != null)
                {
                    switch (c.Command)
                    {
                        case OwlCommand.Commands.Recalibrate:
                            DoRecalibrate();
                            break;
                        case OwlCommand.Commands.CancelAllAndReset:
                            DoCancelAllAndReset();
                            break;
                        case OwlCommand.Commands.Flap:
                            DoFlap();
                            break;
                        case OwlCommand.Commands.HeadLeft:
                            DoHeadLeft();
                            break;
                        case OwlCommand.Commands.HeadRight:
                            DoHeadRight();
                            break;
                        case OwlCommand.Commands.RandomFull:
                            DoRandomFull();
                            break;
                        case OwlCommand.Commands.RandomLong:
                            DoRandomLong();
                            break;
                        case OwlCommand.Commands.RandomShort:
                            DoRandomShort();
                            break;
                        case OwlCommand.Commands.Rest:
                            GoHomePosition();
                            break;
                        case OwlCommand.Commands.SmallWiggle:
                            DoSmallWiggle();
                            break;
                        case OwlCommand.Commands.Wink:
                            DoWink();
                            break;
                        case OwlCommand.Commands.ApplyState:
                            ApplyOwlStateFromCommand(c);
                            break;
                    }

                }
            }

            Status = OwlDeviceStateBase.StatusTypes.Ready;
        }

        private void DoWink()
        {
            _rightEye.GotoPosition(50);
            _rightEye.GotoPosition(0);
            _rightEye.GotoPosition(50);
            _rightEye.GotoPosition(0);
            _rightEye.GotoPosition(50);
            _rightEye.GotoPosition(0);

            _leftEye.GotoPosition(50);
            _leftEye.GotoPosition(0);
            _leftEye.GotoPosition(50);
            _leftEye.GotoPosition(0);
            _leftEye.GotoPosition(50);
            _leftEye.GotoPosition(0);

        }

        private void DoSmallWiggle()
        {
            throw new NotImplementedException();
        }

        private void DoRandomShort()
        {
            throw new NotImplementedException();
        }

        private void DoRandomLong()
        {
            throw new NotImplementedException();
        }

        private void DoRandomFull()
        {
            throw new NotImplementedException();
        }

        private void DoHeadRight()
        {
            throw new NotImplementedException();
        }

        private void DoHeadLeft()
        {
            throw new NotImplementedException();
        }

        private void DoFlap()
        {
            throw new NotImplementedException();
        }

        private void DoCancelAllAndReset()
        {
            throw new NotImplementedException();
        }

        private void DoRecalibrate()
        {
            Recalibrate();
        }

        private void ApplyOwlStateFromCommand(OwlCommand c)
        {
            if (c.State != null)
            {
                ApplyControllerState(_head, c.State.Head, c.StayAfterComplete);
                ApplyControllerState(_leftEye, c.State.LeftEye, c.StayAfterComplete);
                ApplyControllerState(_rightEye, c.State.RightEye, c.StayAfterComplete);
                ApplyControllerState(_wings, c.State.Wings, c.StayAfterComplete);
            }
        }

        private static void ApplyControllerState(OwlControllerBase controller, OwlDeviceStateBase state, bool stayAfterComplete)
        {
            if (state != null)
            {
                controller.ApplyState(state);
            }
        }

        /// <summary>
        /// Not typically used?  Should do it from Command
        /// </summary>
        /// <param name="state"></param>
        public override void ApplyState(OwlDeviceStateBase state)
        {
            Parallel.ForEach(PartsList, (part) =>
            {
                part.ApplyState(state);
            });
        }

        public override bool CancelApplyState()
        {
            bool bOK = true;

            Parallel.ForEach(PartsList, (part) =>
            {
                part.CancelApplyState();
            });

            return bOK;
        }

        public override bool Disable()
        {
            bool bOK = true;

            Parallel.ForEach(PartsList, (part) =>
            {
                part.Disable();
            });

            return bOK;
        }

        public override void DoPositionList(List<int> positions, int msDelayBetween, bool returnToHome)
        {
            Parallel.ForEach(PartsList, (part) =>
            {
                part.DoPositionList(positions, msDelayBetween, returnToHome);
            });
        }

        public override bool Enable()
        {
            bool bOK = true;

            Parallel.ForEach(PartsList, (part) =>
            {
                part.Enable();
            });

            return bOK;
        }

        public override OwlDeviceStateBase GetState()
        {
            //which one to return?
            return null;
        }

        public override void GoHomePosition()
        {
            Parallel.ForEach(PartsList, (part) =>
            {
                part.GoHomePosition();
            });
        }

        public override void GotoPosition(int position)
        {
            Parallel.ForEach(PartsList, (part) =>
            {
                part.GotoPosition(position);
            });
        }

        public override bool Initialize()
        {
            bool bOK = true;

            Parallel.ForEach(PartsList, (part) =>
            {
                part.Initialize();
            });
            return bOK;
        }

        public override void Recalibrate()
        {
            Parallel.ForEach(PartsList, (part) =>
            {
                part.Recalibrate();
            });
        }

        public override bool Shutdown()
        {
            bool bOK = true;

            Parallel.ForEach(PartsList, (part) =>
            {
                part.Shutdown();
            });

            return bOK;
        }

        private void _component_MoveCompleted(object sender)
        {
            //In final version, we'll only fire after all devices report ready
            FireMoveCompleted();
        }

        private void _component_DeviceError(object sender, string error)
        {
            FireDeviceError(sender.ToString() + " : " + error);
        }
    }
}
