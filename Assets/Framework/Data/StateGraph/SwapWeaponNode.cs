using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class SwapWeaponNode : StateGraphNode {
        protected override Vector2 GetNodeSize { get { return base.GetNodeSize * 0.5f; } }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override string Title { get { return "Swap Weapon"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {

            private RuntimeStateNode _exitNode;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeNode(SwapWeaponNode node, RuntimeStateGraph graph) : base(node, graph) {
                _exitNode = GetOriginalNodeExit();
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                
            }

            public override void OnExit() {
                base.OnExit();
                Graph.Entity.Post(new SwapWeaponMessage(Graph.Entity));
            }

            public override bool TryComplete(float dt) {
                return true;
            }
        }
    }

    public struct SwapWeaponMessage : IEntityMessage {
        public Entity Entity { get; }

        public SwapWeaponMessage(Entity entity) {
            Entity = entity;
        }
    }
}
