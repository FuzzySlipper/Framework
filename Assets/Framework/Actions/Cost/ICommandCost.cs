namespace PixelComrades {
    public interface ICommandCost {
        bool CanAct(Entity owner, Entity action);
        void ProcessCost(Entity owner, Entity action);
    }
}