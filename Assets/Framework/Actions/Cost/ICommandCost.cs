namespace PixelComrades {
    public interface ICommandCost {
        void ProcessCost(ActionTemplate action, CharacterTemplate owner);
        bool CanAct(ActionTemplate action, CharacterTemplate owner);
    }
}