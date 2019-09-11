namespace PixelComrades {
    public interface ICommandCost {
        bool CanAct(Entity entity);
        void ProcessCost(Entity entity);
    }
}