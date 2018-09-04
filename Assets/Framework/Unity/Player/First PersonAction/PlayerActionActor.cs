//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using PixelComrades;


//public class PlayerActionActor : PlayerActor {

//    private const float ThrowTime = 0.2f;

//    //[SerializeField, Range(5, 15)] private float _dropForce = 10f;
//    [SerializeField] private FloatRange _throwForce = new FloatRange(1, 50);
//    [SerializeField, Range(0.5f,5)] private float _throwChargeTime = 2f;
//    [SerializeField] private Vector3 _throwExtra = new Vector3(0,15f,0);

//    private FirstPersonController _controller;
//    private MecanimAnimator _animator;
//    private ScaledTimer _throwTimeout = new ScaledTimer(ThrowTime*3);
//    private float _throwStartTime;
//    private TweenFloat _throwCharge;
//    private TweenV3 _throwFinish;
//    private Task _throwCharging; 
//    private HashSet<string> _activeAttemptedEquip = new HashSet<string>();


//    public override ActorAnimator Animator { get { return _animator; } }
//    public override bool CanMove { get { return _controller.CanMove; } set { _controller.CanMove = value; } }
//    public DestructibleObject CurrentHeld { get; set; }

//    protected override void OnCreate() {
//        base.OnCreate();
//        Inventory.OnAdd += CheckNewItem;
//        _controller = GetComponent<FirstPersonController>();
//        _animator = GetComponent<MecanimAnimator>();
//        _throwCharge = new TweenFloat(0, 1, _throwChargeTime, EasingTypes.QuarticOut, true);
//        _throwFinish = new TweenV3(Vector3.zero, Vector3.one, ThrowTime, EasingTypes.ElasticOut);
//        Player.Controller = _controller;
//    }

//    private void CheckNewItem(Entity item) {
//        //if (HotbarInventory.IsFull || item.ItemType != ItemTypes.Equipment || 
//        //    _activeAttemptedEquip.Contains(item.Id)) {
//        //    return;
//        //}
//        //_activeAttemptedEquip.Add(item.Id);
//        //if (HotbarInventory.AddItem(item)) {
//        //    UIPlayerComponents.HotbarUI.FlashActive(2.5f);
//        //}
//    }


//    protected override void StartNewGame() {
//        _activeAttemptedEquip.Clear();
//        base.StartNewGame();
//    }

//    public void PrimaryStart() {
//        //_animator.SetButtonStatus(LayerAnimations.Primary, true);
//        //if (CurrentHeld != null) {
//        //    StartHeldCharge();
//        //}
//        //else {
//        //    _animator.PrimaryStart();
//        //}
//    }

//    public void PrimaryEnd() {
//        //_animator.SetButtonStatus(LayerAnimations.Primary, false);
//        //if (CurrentHeld != null) {
//        //    FinishThrowAction();
//        //}
//    }

//    public void SecondaryStart() {
//        //_animator.SetButtonStatus(LayerAnimations.Secondary, true);
//        //if (CurrentHeld != null) {
//        //    CancelThrow();
//        //}
//        //else {
//        //    _animator.SecondaryStart();
//        //}
//    }

//    public void SecondaryEnd() {
//        //_animator.SetButtonStatus(LayerAnimations.Secondary, false);
//    }

//    public bool PickupDestructible(DestructibleObject destructible) {
//        if (_throwTimeout.IsActive) {
//            return false;
//        }
//        //_animator.ActiveSlot.SetItemHidden();
//        //_animator.OffHand.SetItemHidden();
//        CurrentHeld = destructible;
//        CurrentHeld.Tr.SetParent(FirstPersonCamera.HoldTr);
//        var offset = new Vector3(0, destructible.Collider.bounds.size.y * -0.75f, 
//            destructible.Collider.bounds.size.z *2);
//        _throwCharging = TimeManager.Start(PickupAnimation(offset), () => { _throwCharging = null; });
//        return true;
//    }

//    private IEnumerator PickupAnimation(Vector3 offset) {
//        var startRot = CurrentHeld.Tr.localRotation;
//        _throwCharge.Restart(0,1,0.35f);
//        while (_throwCharge.Active) {
//            CurrentHeld.Tr.localPosition = Vector3.Lerp(CurrentHeld.Tr.localPosition, offset, _throwCharge.Percent);
//            CurrentHeld.Tr.localRotation = Quaternion.Slerp(startRot, Quaternion.identity, _throwCharge.Percent);
//            _throwCharge.Get();
//            yield return null;
//        }
//    }

//    public void StartHeldCharge() {
//        if (CurrentHeld == null) {
//            return;
//        }
//        if (_throwCharging != null) {
//            TimeManager.Stop(_throwCharging);
//            _throwCharging = null;
//        }
//        _throwStartTime = TimeManager.Time;
//        UIChargeCircle.StartCharge(_throwChargeTime);
//        _throwCharging = TimeManager.Start(ChargeThrow(), () => { _throwCharging = null; });
//    }

//    private IEnumerator ChargeThrow() {
//        _throwCharge.Restart(0,1,_throwChargeTime);
//        while (_throwCharge.Active) {
//            FirstPersonCamera.HoldTr.position += 
//                FirstPersonCamera.HoldTr.forward*(_throwCharge.Get()*-0.5f)* Game.DeltaTime;
//            yield return null;
//        }
//    }

//    public void CancelThrow() {
//        if (CurrentHeld == null) {
//            return;
//        }
//        if (_throwCharging != null) {
//            TimeManager.Stop(_throwCharging);
//            _throwCharging = null;
//        }
//        TimeManager.Start(FinishThrow(1f));
//    }

//    public void FinishThrowAction() {
//        if (CurrentHeld == null) {
//            return;
//        }
//        if (_throwCharging != null) {
//            TimeManager.Stop(_throwCharging);
//            _throwCharging = null;
//        }
//        var timeCharging = TimeManager.Time - _throwStartTime;
//        var force = _throwForce.Lerp(Mathf.Clamp01(timeCharging/_throwChargeTime));
//        TimeManager.Start(FinishThrow(force));
//    }

//    private IEnumerator FinishThrow(float force) {
//        UIChargeCircle.StopCharge();
//        _throwTimeout.Activate();
//        var start = FirstPersonCamera.HoldTr.localPosition;
//        var end = FirstPersonCamera.HoldStartPosition;
//        _throwFinish.Restart(start,end);
//        bool activated = false;
//        while (_throwFinish.Active) {
//            FirstPersonCamera.HoldTr.localPosition = _throwFinish.Get();
//            if (!activated && _throwFinish.Percent > 0.8f) {
//                activated = true;
//                ActivateThrow(force);
//            }
//            yield return null;
//        }
//        //_animator.ActiveSlot.SetItemVisible();
//        //_animator.OffHand.SetItemVisible();
//    }

//    private void ActivateThrow(float force) {
//        var held = CurrentHeld;
//        if (held == null) {
//            return;
//        }
//        CurrentHeld = null;
//        held.Tr.SetParent(ItemPool.ActiveSceneTr);
//        held.Throw((FirstPersonCamera.main.transform.forward + _throwExtra) * force);
//        Physics.IgnoreCollision(held.Collider, Collider, true);
//        TimeManager.PauseFor(2f, false, () => {Physics.IgnoreCollision(held.Collider, Collider, false);});
//    }

//    protected override void OnDeath(ImpactEvent dmg) {
//        base.OnDeath(dmg);
//        //_animator.DeathAnimation();
//        if (CurrentHeld != null) {
//            CancelThrow();
//        }
//    }
//}