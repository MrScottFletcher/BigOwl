
namespace BigOwl.Entity
{
    public class OwlState
    {
        public ServoState LeftEye { get; set; }
        public ServoState RightEye { get; set; }
        public StepperState Head { get; set; }
        public StepperState Wings { get; set; }

    }
}
