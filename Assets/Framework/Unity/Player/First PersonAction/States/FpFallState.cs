using UnityEngine;
using System.Collections;
using PixelComrades;

public class FpFallState : FpState {

    //private const float TryStopStateTime = 2.5f;

    public override Labels Label{ get { return FpState.Labels.Fall; } }
    public override float MoveSpeed { get { return Settings.WalkSpeed * Settings.InAirSpeed; } }

    private float _fallingStartPos, _fallingDist;

    public override void Enter() {
        _fallingStartPos = _context.transform.position.y;
        _fallingDist = 0;
    }

    public override void UpdateMovement(Vector3 moveVector, bool isForward, ref Vector3 moveDirection) {
        if (FirstPersonController.Grounded) {
            if (_fallingDist > Settings.FallingDistanceToDamage) {
                //var damage = Mathf.RoundToInt(_fallingDist * Settings.FallingDamageMultiplier);
                //var damageEvent = DamageEvent.Get(null, damage,DamageTypes.Impact, _context.transform.position + Vector3.downPlayer.Actor);
                //Combat.ApplyDamage(damageEvent);
                //Debug.Log("fall damage " + damage);
            }
            _machine.ChangeState<FpNormalState>();
            return;
        }
        if (Settings.FallExtraForce > 0) {
            
        }
        //if (_enterStateTime + TryStopStateTime < Global.Time) {
        //    moveDirection = Vector3.zero;
        //    _enterStateTime = Global.Time;
        //    return;
        //}
        _fallingDist = _fallingStartPos - _context.transform.position.y;
        if (Settings.AirControl) {
            moveDirection = moveVector*(MoveSpeed*Settings.InAirSpeed);
        }
        
        //moveDirection = FallVector(moveDirection);
        //if (_context.transform.position.y < Global.YFloor) {
        //    if (Global.Debug) {
        //        Debug.LogError("player below ground");
        //    }
        //    _context.transform.position = DungeonPathfinder.main.ClosestPointOnGrid(_context.transform.position);
        //    moveDirection = Vector3.zero;
        //}
    }
}