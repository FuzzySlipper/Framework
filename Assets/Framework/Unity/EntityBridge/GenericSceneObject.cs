using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class GenericSceneObject : MonoBehaviour {

        [SerializeField] private string _id = "";
        
        public string SheetName;
        public string ID { get { return string.IsNullOrEmpty(_id) ? name : _id; } }

        public Entity BuildEntity() {
            var playEntity = Entity.New(ID);
            playEntity.Add(new GenericSceneObjectComponent(gameObject));
            playEntity.Add(new TransformComponent(transform));
            var animator = GetComponentInChildren<Animator>(true);
            if (animator != null) {
                playEntity.Add(new UnityAnimatorComponent(animator));
            }
            var poseAnimator = GetComponentInChildren<PoseAnimator>(true);
            if (poseAnimator != null) {
                playEntity.Add(new PoseAnimatorComponent(poseAnimator.Avatar, poseAnimator.DefaultPose, poseAnimator.transform));
            }
            var ikPoser = GetComponentInChildren<AnimationIkPoser>();
            if (ikPoser != null) {
                playEntity.Add(new AnimationIkPoserComponent(ikPoser));
            }
            return playEntity;
        }

        public static GenericSceneObject Find() {
            var obj = FindObjectOfType<GenericSceneObject>();
            if (obj != null) {
                var root = obj.transform.root.GetComponent<GenericSceneObject>();
                if (root != null) {
                    return root;
                }
            }
            return obj;
        }
    }
}
