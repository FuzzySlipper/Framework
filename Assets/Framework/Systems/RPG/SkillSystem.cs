using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class SkillSystem : SystemBase, IReceive<DataDescriptionUpdating> {

        public SkillSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(SkillRequirement)
            }));
        }

        public void Handle(DataDescriptionUpdating arg) {
            var skillReq = arg.Data.GetEntity().Get<SkillRequirement>();
            if (skillReq == null || skillReq.Required == 0) {
                return;
            }
            FastString.Instance.Clear();
            FastString.Instance.AppendBoldLabelNewLine("Requires " + Skills.GetNameAt(skillReq.Skill), skillReq.Required.ToDescription());
            arg.Data.Text += FastString.Instance.ToString();
        }
    }
}
