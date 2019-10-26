namespace NonVisuals.Interfaces
{

    public enum EnumStreamDeckButtonActionType
    {
        Unknown = 0,
        KeyPress = 1,
        DCSBIOS = 2,
        OSCommand = 4,
        LayerNavigation = 16
    }

    public interface IStreamDeckButtonAction
    {
        EnumStreamDeckButtonActionType ActionType { get; }
        int ExecutionDelay { get; set; }
        void Execute();
        //void SetAction(object action);
        //object GetAction();

        /*string ExportJSON();
        void ImportJSON(string json);*/




    }
}
