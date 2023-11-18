`OpenMacroBoard.SDK` is library that helps you implement custom functions for different macro boards (with focus on devices with display keys, like the Stream Deck or Stream Deck Mini)

# Getting started
Create a new console project in Visual Studio (>= .Net 4.0), add the nuget package `OpenMacroBoard.VirtualBoard` and copy/paste the following lines:

```CSharp
using(var device = BoardFactory.SpawnVirtualBoard())
{
    var redKey = KeyBitmap.Create.FromRgb(255,0,0);
    device.SetKeyBitmap(redKey);
}
```

If you want to run your program on a real device you add the provider via nuget (for example `StreamDeckSharp`).

# Plans for the future
I'd like to write an open source alternative for the elgato stream deck software that supports more devices and lets developers write functions/plugins/widgets for all devices with an existing `IMacroBoard` provider. If you want to help feel free to contact me (just create a ticket or send me a mail).

# Is device _XYZ_ supported?
At the moment there are only three `IMacroBoard` providers
1. Stream Deck (via `StreamDeckSharp`)
    1. Stream Deck (original/classic)
    2. Stream Deck Rev 2
    3. Stream Deck MK2
    4. Stream Deck Mini
    5. Stream Deck XL
4. Virtual Macro Board (via `OpenMacroBoard.SocketIO` and VirtualBoard.exe)

I'd love to add more. You can implement it by yourself if you are a developer by referencing `OpenMacroBoard.SDK` and write a method that returns an `IMacroBoard`. If want me to implement it, you can donate hardware (or the money so I can buy that specific hardware you want implemented) - just create a ticket and we talk about it ;-)
