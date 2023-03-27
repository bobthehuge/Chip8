// See https://aka.ms/new-console-template for more information

using System;
using Emudev;

/*var trans = Cheepl.Translate("given_files/triangle.ch8");

foreach(var dt in trans)
    Console.WriteLine(dt);*/

var test = new Chip8(new Random());
//test.PrintDebug(Debug.Display);
test.LoadRom("roms/test.ch8");
//test.PrintDebug(Debug.Memory);

//test.ParseInput("roms/input.in");

test.RunProgram();

//test.PrintDebug(Debug.Input);
//test.PrintDebug(Debug.Display);
