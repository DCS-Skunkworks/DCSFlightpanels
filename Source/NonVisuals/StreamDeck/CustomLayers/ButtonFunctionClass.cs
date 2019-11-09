using System;
using System.Collections.Generic;

namespace NonVisuals.StreamDeck.CustomLayers
{
    public class ButtonFunction
    {
        public uint Id { get;}
        public string Description { get;}

        public ButtonFunction(uint id, string description)
        {
            Id = id;
            Description = description;
        }
    }

    public class ButtonFunctionList : List<ButtonFunction>
    {

        public new void Add(ButtonFunction buttonFunction)
        {
            if (FindAll(o => o.Id == buttonFunction.Id).Count > 0)
            {
                throw new Exception("ButtonFunctionList already contains an entry for Id " + buttonFunction.Id);
            }
            if (FindAll(o => o.Description == buttonFunction.Description).Count > 0)
            {
                throw new Exception("ButtonFunctionList already contains an entry having description " + buttonFunction.Description);
            }
            Add(buttonFunction);
        }
    }
}
