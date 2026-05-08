using System;

namespace TopSpeed.Vehicles.Control
{
    internal sealed class PitLaneController : ICarController
    {
        private const float TargetSpeedKmh = 56.33f;
        private const float BandKmh = 2f;
        private const float SteerThresholdM = 0.3f;

        private readonly Func<float, int> _gearForSpeed;

        public bool BrakeMode { get; set; }
        public float? SteerTargetX { get; set; }

        public PitLaneController(Func<float, int> gearForSpeed)
        {
            _gearForSpeed = gearForSpeed;
        }

        public CarControlIntent ReadIntent(in CarControlContext context)
        {
            var steering = 0;
            if (SteerTargetX.HasValue)
            {
                var diff = SteerTargetX.Value - context.PositionX;
                if (diff > SteerThresholdM) steering = 100;
                else if (diff < -SteerThresholdM) steering = -100;
            }

            if (BrakeMode)
                return new CarControlIntent(steering, 10, -100, 100, false, false, false);

            var speed = context.Speed;
            var error = TargetSpeedKmh - speed;

            // Gear management for manual transmissions only;
            // automatic transmissions use their own auto-shifter.
            var gearDown = false;
            var gearUp = false;
            var clutch = 0;

            if (context.ManualTransmission && context.Gear > 0)
            {
                var gear = context.Gear;
                var targetGear = _gearForSpeed(speed);
                if (gear > targetGear)
                {
                    gearDown = true;
                    clutch = 100;
                }
                else if (gear < targetGear && error > BandKmh)
                {
                    gearUp = true;
                    clutch = 100;
                }
            }

            if (error > BandKmh)
                return new CarControlIntent(steering, 100, 0, clutch, false, gearUp, gearDown);

            if (error < -BandKmh)
            {
                var brake = (int)Math.Max(-100f, error * 2f);
                return new CarControlIntent(steering, 0, brake, clutch, false, false, gearDown);
            }

            return new CarControlIntent(steering, 50, 0, clutch, false, false, gearDown);
        }
    }
}
