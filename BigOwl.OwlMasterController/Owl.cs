using BigOwl.Devices;
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

        private PwmControl.OwlPwmController _leftEye;
        private PwmControl.OwlPwmController _rightEye;

        public Owl()
        {
            _head = new StepperControl.OwlStepperController(19, 26, 13, 6, 5, 21, false);
            _head.StepToPositionFactor = 4;
            _head.MinPosition = 0;
            _head.MaxPosition = 100;
            _head.HomePosition = 50;

            //Only for initial testing
            _head.CurrentPosition = 100;

            _head.DeviceError += _component_DeviceError;
            _head.MoveCompleted += _component_MoveCompleted;


            _wings = new StepperControl.OwlStepperController(17, 18, 22, 23, 24, 25, true);
            _wings.StepToPositionFactor = 8;
            _wings.MinPosition = 0;
            _wings.MaxPosition = 100;
            _wings.HomePosition = 95;

            //Only for initial testing
            _wings.CurrentPosition = 50;

            _wings.DeviceError += _component_DeviceError;
            _wings.MoveCompleted += _component_MoveCompleted;

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
            throw new NotImplementedException();
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
            if(c.State != null)
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

        public override void ApplyState(OwlDeviceStateBase state)
        {
            _head.ApplyState(state);
            _wings.ApplyState(state);
        }

        public override bool CancelApplyState()
        {
            bool bOK = true;

            _head.CancelApplyState();
            _wings.CancelApplyState();

            return bOK;
        }

        public override bool Disable()
        {
            bool bOK = true;
            _head.Disable();
            _wings.Disable();
            return bOK;
        }

        public override void DoPositionList(List<int> positions, int msDelayBetween, bool returnToHome)
        {
            _head.GoHomePosition();
            _wings.GoHomePosition();
        }

        public override bool Enable()
        {
            bool bOK = true;

            _head.GoHomePosition();
            _wings.GoHomePosition();

            return bOK;
        }

        public override OwlDeviceStateBase GetState()
        {
            //which one to return?
            return null;
        }

        public override void GoHomePosition()
        {
            _head.GoHomePosition();
            _wings.GoHomePosition();
        }

        public override void GotoPosition(int position)
        {
            _head.GotoPosition(position);
            _wings.GotoPosition(position);
        }

        public override bool Initialize()
        {
            bool bOK = true;

            _head.Initialize();
            _wings.Initialize();

            return bOK;
        }

        public override void Recalibrate()
        {
            _head.Recalibrate();
            _wings.Recalibrate();
        }

        public override bool Shutdown()
        {
            bool bOK = true;

            _head.Shutdown();
            _wings.Shutdown();

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
