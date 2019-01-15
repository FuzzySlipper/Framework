using UnityEngine;
using System.Collections;

public class WhileLoopLimiter {

    public static WhileLoopLimiter Instance = new WhileLoopLimiter(5500);

    public static void ResetInstance() {
        Instance.Reset();
    }

    public static bool InstanceAdvance() {
        return Instance.Advance();
    }
    
    public WhileLoopLimiter(int limit) {
        Activate(limit);
    }

    private bool _active;
    private int _count;
    private int _limit;

    public bool Active { get { return _active && _count <= _limit; } }
    public bool HitLimit { get { return _count > _limit; } }
    public int Count {get { return _count; } }

    private void Activate(int limit) {
        _limit = limit;
        _count = 0;
        _active = true;
    }

    public void Reset() {
        _count = 0;
        _active = true;
    }

    public void Reset(int limit) {
        _limit = limit;
        _count = 0;
        _active = true;
    }

    public void Cancel() {
        _count = _limit + 1;
        _active = false;
    }

    public bool Advance() {
        _count++;
        return _count <= _limit;
    }

    public void Finish() {
        _active = false;
    }
}
