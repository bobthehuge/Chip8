using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Emudev;
using Emulator;

Console.Clear();
Console.WriteLine("Welcome to Chip8 Emulator! Type 'help' to show available options.");

Chip8 chip = new Chip8(new Random());
List<string> roms = Directory.GetFiles("Roms", "*.ch8").ToList();
int rom_index = 0;
EmulStatus status = EmulStatus.Idle;

while (status != EmulStatus.Exit && !(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
{
    Console.WriteLine();
    Console.Write("> ");
    var input = Console.ReadLine();

    switch (input)
    {
        case "exit": case "Exit":
            status = EmulStatus.Exit;
            break;
        
        case "help":
            Console.WriteLine("Options available:");
            Console.WriteLine("\thelp : print this message");
            Console.WriteLine("\texit / Exit : exit Chip8 emulator program");
            Console.WriteLine("\trefresh_roms : refresh rom list with those present in 'Roms' directory");
            Console.WriteLine("\tlist_roms : list all currently loaded roms");
            Console.WriteLine("\tload : load rom with provided index (must be within listed roms range)");
            Console.WriteLine("\treset : set chip8 to new object");
            Console.WriteLine("\trun : runs currently selected rom");
            Console.WriteLine("\tcheepl : deepl for chip8 : translates currently loaded rom to assembly like language, prints it and write it to {name}.chasm");
            break;
        
        case "refresh_roms":
            roms = Directory.GetFiles("Roms", "*.ch8").ToList();
            break;
        
        case "list_roms":
            Console.WriteLine("Available roms:");
            
            for(var i = 0; i < roms.Count; i++)
                Console.WriteLine($"\t{i+1}) {Path.GetFileName(roms[i])}");
            
            break;
        
        case "load":
            Console.WriteLine("Please enter which rom you would like to use:");
            var tmp_input = Console.ReadLine();

            var succeed = Int32.TryParse(tmp_input, out rom_index);

            if (!succeed || rom_index < 1 || rom_index > roms.Count)
            {
                Console.WriteLine("Invalid rom number");
                rom_index = -1;
            }
            else
            {
                rom_index--;
                chip.LoadRom(roms[rom_index]);
            }

            break;
        
        case "reset":
            chip = new Chip8(new Random());
            break;
        
        case "run":
            chip.RunProgram();
            break;
        
        case "cheepl":
            var translated = Cheepl.Translate(chip._memory);
            
            Console.WriteLine("Translated data:");
            
            foreach (var line in translated)
                Console.WriteLine("\t" + line);
            
            Console.WriteLine("-- end --");

            string fname = Path.ChangeExtension(Path.GetFileName(roms[rom_index]), ".chasm");
            
            File.WriteAllLines(fname, translated);
            break;
        
        default:
            Console.WriteLine($"Unknown option '{input}'");
            break;
    }
}

/*var test = new Chip8(new Random());
//test.PrintDebug(Debug.Display);
test.LoadRom("Roms/test.ch8");
//test.PrintDebug(Debug.Memory);

//test.ParseInput("roms/input.in");

test.RunProgram();

//test.PrintDebug(Debug.Input);
//test.PrintDebug(Debug.Display);*/
