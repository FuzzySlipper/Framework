using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SensorSystem : SystemBase, IPeriodicUpdate {

        public void OnPeriodicUpdate() {
            //var cellList = EntityController.GetList<SensorCellsComponent>();
            //if (cellList != null) {
            //    cellList.RunAction(UpdateSenses);
            //}
            var simpleList = EntityController.GetComponentArray<SensorSimpleComponent>();
            if (simpleList != null) {
                simpleList.RunAction(UpdateSenses);
            }
        }
        
        private void UpdateSenses(SensorSimpleComponent simple) {
            simple.TempList.Clear();
            World.Get<FactionSystem>().FillFactionEnemiesList(simple.TempList, simple.Faction);
            var owner = simple.GetEntity();
            var gridPos = owner.Get<GridPosition>().Position;
            for (int i = 0; i < simple.TempList.Count; i++) {
                var target = simple.TempList[i];
                if (gridPos.Distance(target.Get<GridPosition>().Position) > simple.MaxHearDistance) {
                    continue;
                }
                if (World.Get<LineOfSightSystem>().CanSeeOrHear(owner, target, out var hearingBlocked)) {
                    simple.AddWatch(target, true);
                }
                else if (!hearingBlocked) {
                    simple.AddWatch(target, false);
                }
            }
            simple.UpdateWatchTargets();
        }
        
    }
}
