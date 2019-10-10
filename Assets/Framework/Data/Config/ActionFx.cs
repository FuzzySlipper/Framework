using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {

    [System.Serializable]
	public sealed class ImpactRendererComponent : IComponent {
        private CachedGenericComponent<IImpactRenderer> _renderer;

        public ImpactRendererComponent(IImpactRenderer renderer) {
            _renderer = new CachedGenericComponent<IImpactRenderer>(renderer);
        }

        public void PlayAnimation(SpriteAnimation animation, Color color) {
            _renderer.Value.PlayAnimation(animation, color);
        }

        public ImpactRendererComponent(SerializationInfo info, StreamingContext context) {
            _renderer = info.GetValue(nameof(_renderer), _renderer);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_renderer), _renderer);
        }
    }

    public interface IImpactRenderer {
        void PlayAnimation(SpriteAnimation animation, Color color);
    }

    [CreateAssetMenu(menuName =  "Assets/ActionFx")]
    public class ActionFx : ScriptableObject {

        [SerializeField] private ActionFxData[] _actionData = new ActionFxData[0];

        public bool TryGetColor(out Color color) {
            for (int i = 0; i < _actionData.Length; i++) {
                switch (_actionData[i].Event) {
                    case ActionState.Collision:
                    case ActionState.CollisionOrImpact:
                    case ActionState.Impact:
                        if (_actionData[i].Particle.Animation != null) {
                            color = _actionData[i].Particle.Color;
                            return true;
                        }
                        break;
                }
            }
            for (int i = 0; i < _actionData.Length; i++) {
                if (_actionData[i].Particle.Animation != null) {
                    color = _actionData[i].Particle.Color;
                    return true;
                }
            }
            color = Color.red;
            return false;
        }

        public void TriggerEvent(ActionEvent actionEvent) {
            for (int i = 0; i < _actionData.Length; i++) {
                if (_actionData[i].Event == actionEvent.State || (_actionData[i].Event == ActionState.CollisionOrImpact && (actionEvent.State == ActionState.Impact || actionEvent.State == ActionState.Collision))) {
                    if (_actionData[i].Sound != null) {
                        AudioPool.PlayClip(_actionData[i].Sound, actionEvent.Position, 0.5f);
                    }
                    if (_actionData[i].Particle.Animation == null) {
                        
                        continue;
                    }
                    switch (_actionData[i].Event) {
                        case ActionState.Collision:
                        case ActionState.CollisionOrImpact:
                        case ActionState.Impact:
                            if (actionEvent.Target != null) {
                                var impactRenderer = actionEvent.Target.Get<ImpactRendererComponent>();
                                if (impactRenderer != null) {
                                    impactRenderer.PlayAnimation(_actionData[i].Particle.Animation, _actionData[i].Particle.Color);
                                    continue;
                                }
                            }
                            break;
                    }
                    //var spawn = ItemPool.SpawnScenePrefab(_actionPrefabs[i].Prefab, actionEvent.Position, actionEvent.Rotation);
                    //CheckObjectForListener(spawn, actionEvent);
                    var particle = SpriteParticleSystem.PlayParticle(_actionData[i].Particle, actionEvent.Position, actionEvent.Rotation);
                    if (!_actionData[i].Parent) {
                        continue;
                    }
                    TransformComponent tr = null;
                    if (actionEvent.Target == null) {
                        tr = actionEvent.Origin.Tr;
                    }
                    else {
                        tr = actionEvent.Target.Tr;
                    }
                    if (tr != null) {
                        tr.SetChild(particle.Tr);
                    }
                }
            }
        }

        private void CheckObjectForListener(GameObject newObject, ActionEvent stateEvent) {
            var actionListener = newObject.GetComponent<IActionPrefab>();
            if (actionListener != null) {
                actionListener.OnActionSpawn(stateEvent);
            }
        }
    }
}