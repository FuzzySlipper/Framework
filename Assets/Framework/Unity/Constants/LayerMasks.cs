using UnityEngine;
using System.Collections;

public static class LayerMasks  {
    public static int NumberDefault = 0;
    public static int NumberActorMovement = 8;
    public static int NumberEnvironment = 9;
    public static int NumberCollision = 10;
    public static int NumberWorldControls = 11;
    public static int NumberSensors = 12;
    public static int NumberPlayerExemptColl = 13;
    public static int NumberPlayer = 14;
    public static int NumberMap = 15;
    public static int NumberNodeMap = 16;
    public static int NumberNoActorCollision = 17;
    public static int NumberRiftMap = 18;
    public static int NumberParallax = 19;
    public static int NumberEnemy = 20;
    public static int NumberCeiling = 21;
    public static int NumberWall = 22;
    public static int NumberFloor = 23;
    public static int NumberWeapon = 24;

    private static LayerMask _default = 1 << 0;
    private static LayerMask _collision = 1 << NumberCollision;
    private static LayerMask _environmentOnly = 1 << NumberEnvironment;
    private static LayerMask _map = 1 << NumberMap;
    //private static LayerMask _worldControls = 1 << NumberWorldControls;
    private static LayerMask _player = 1 << NumberPlayer;
    private static LayerMask _actorMovement = 1 << NumberActorMovement;
    private static LayerMask _ceiling = 1 << NumberCeiling;
    private static LayerMask _wall = 1 << NumberWall;
    private static LayerMask _floor = 1 << NumberFloor;
    private static LayerMask _enemy = 1 << NumberEnemy;
    private static LayerMask _environment = _environmentOnly | _ceiling | _wall | _floor;
    private static LayerMask _wallsEnvironment = _environmentOnly | _wall;
    private static LayerMask _defaultCollision = _default | _collision | _environment | _player | _enemy;
    private static LayerMask _dropPanel = _default | _environment;

    public static LayerMask DefaultCollision { get { return _defaultCollision; } }
    public static LayerMask DropPanel { get { return _dropPanel; } }
    public static LayerMask Environment {get { return _environment; } }
    public static LayerMask WallsEnvironment { get { return _wallsEnvironment; } }
    public static LayerMask ActorMovement {get { return _actorMovement; } }
    public static LayerMask Map { get { return _map; } }
    public static LayerMask Floor { get { return _floor; } }
    public static LayerMask Ceiling { get { return _ceiling; } }
    public static LayerMask Walls { get { return _wall; } }
}
