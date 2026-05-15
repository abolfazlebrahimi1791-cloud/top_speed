using System;
using TopSpeed.Physics.Fuel;

namespace TopSpeed.Vehicles
{
    internal partial class Car
    {
        private const float MinUsableFuelLiters = 0.0001f;
        private const float HorsepowerToKilowatts = 0.745699872f;

        private void ConfigureFuelModel(VehicleDefinition definition)
        {
            var tankCapacityLiters = SanitizeFinite(
                definition.FuelTankCapacityLiters > 0f
                    ? definition.FuelTankCapacityLiters
                    : VehicleDefinition.FuelTankCapacityDefaultLiters,
                VehicleDefinition.FuelTankCapacityDefaultLiters);
            var displacementLiters = SanitizeFinite(
                definition.EngineDisplacementLiters > 0f
                    ? definition.EngineDisplacementLiters
                    : VehicleDefinition.EngineDisplacementDefaultLiters,
                VehicleDefinition.EngineDisplacementDefaultLiters);

            _fuelConfiguration = new FuelConfig(tankCapacityLiters, displacementLiters);
            _fuelTankCapacityLiters = _fuelConfiguration.TankCapacityLiters;
            _fuelEngineDisplacementLiters = _fuelConfiguration.EngineDisplacementLiters;
            _fuelState = new FuelRuntimeState(_fuelTankCapacityLiters, 0f);
            _fuelBurnLitersPerHour = 0f;
            _fuelEstimatedRangeMeters = 0f;
            _fuelEfficiencyLitersPer100Km = 0f;
            _fuelEfficiencyMpg = 0f;
            _fuelLow = false;
            _fuelEmpty = false;
            _fuelPowerScale = 1f;

            _baseMassKgAtFullTank = Math.Max(1f, _massKg);
            ApplyFuelMassForRemaining(_fuelState.RemainingLiters);
        }

        private void UpdateFuelModel(float elapsedSeconds)
        {
            var throttleNormalized = Math.Max(0f, Math.Min(100f, _currentThrottle)) / 100f;
            var netPowerKw = Math.Max(0f, _engine.NetHorsepower) * HorsepowerToKilowatts;
            var speedMps = Math.Max(0f, _speed / 3.6f);
            var result = FuelRuntime.Step(
                _fuelConfiguration,
                _fuelState,
                new FuelRuntimeInput(
                    elapsedSeconds: Math.Max(0f, elapsedSeconds),
                    combustionActive: _combustionState == EngineCombustionState.On && !_engineStalled,
                    throttleNormalized: throttleNormalized,
                    netPowerKw: netPowerKw,
                    speedMps: speedMps));

            _fuelState = result.State;
            _fuelBurnLitersPerHour = result.BurnLitersPerHour;
            _fuelEstimatedRangeMeters = result.EstimatedRangeMeters;
            _fuelEfficiencyLitersPer100Km = result.EfficiencyLitersPer100Km;
            _fuelEfficiencyMpg = result.EfficiencyMpg;
            _fuelLow = result.LowFuel;
            _fuelEmpty = result.EmptyFuel;
            _fuelPowerScale = result.PowerScale;
            ApplyFuelMassForRemaining(_fuelState.RemainingLiters);

            if (_fuelEmpty && _combustionState == EngineCombustionState.On && !_engineStalled)
                StallEngine(playFailureCue: false);
        }

        private void ApplyFuelMassForRemaining(float remainingLiters)
        {
            var clampedRemaining = Math.Max(0f, Math.Min(_fuelTankCapacityLiters, remainingLiters));
            var consumedLiters = Math.Max(0f, _fuelTankCapacityLiters - clampedRemaining);
            var consumedMassKg = consumedLiters * _fuelConfiguration.FuelDensityKgPerLiter;
            var effectiveMassKg = _baseMassKgAtFullTank - consumedMassKg;
            _massKg = Math.Max(1f, effectiveMassKg);
        }

        private bool CanStartEngineWithFuel()
        {
            return _fuelState.RemainingLiters > MinUsableFuelLiters;
        }

        private void PlayFuelStartBlockedCue()
        {
            if (!_soundBadSwitch.IsPlaying)
                _soundBadSwitch.Play(loop: false);
        }
    }
}
