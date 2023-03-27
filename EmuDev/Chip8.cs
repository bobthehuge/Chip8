using System;
using System.Collections.Generic;
using System.IO;

namespace Emudev
{
    public enum Debug
    {
        Instruction,
        Timer,
        Register,
        Stack,
        Input,
        Display,
        Memory,
        All
    }
    
    public class Chip8
    {
        private int consoleLeftOrigin = 0;
        private int consoleTopOrigin = 0;
        private string separator = new String('-', 130);

        private Dictionary<byte, Action<ushort>> _handlers;

        private Stack<ushort> _stack;
        private Queue<bool[]> _input;

        private byte[] _memory;
        private byte[] _registers;
        private bool[] _gfx;

        private ushort _I;
        private ushort _PC;
        private byte _delayTimer;
        private byte _soundTimer;

        private Random _random;
 
        public Chip8(Random random)
        {
            this._handlers = new Dictionary<byte, Action<ushort>>();
            AddHandlers();
            
            this._stack = new Stack<ushort>();
            this._input = new Queue<bool[]>();
            
            this._memory = new byte[4096];
            InitRom();
            this._registers = new byte[16];
            this._gfx = new bool[2048];

            this._I = 0;
            this._PC = 0x200;
            this._delayTimer = 0; 
            this._soundTimer = 0;

            this._random = random;
        }

        public ushort this[int i]
        {
            get { return (ushort) ((_memory[i] << 8) + _memory[i + 1]); }
        }

        private byte GetNibble(ushort instruction, int n)
        {
            if(n == 1)
                return (byte)((instruction & 0xF000) >> 12);
        
            if(n == 2)
                return (byte)((instruction & 0x0F00) >> 8);
        
            if(n == 3)
                return (byte)((instruction & 0x00F0) >> 4);
        
            if(n == 4)
                return (byte)(instruction & 0x000F);

            return 0;
        }   

        private byte GetN(ushort instruction)
        {
            return (byte)(instruction & 0x000F);
        }

        private byte GetNN(ushort instruction)
        {
            return (byte)(instruction & 0x00FF);
        }
        
        private ushort GetNNN(ushort instruction)
        {
            return (ushort)(instruction & 0x0FFF);
        }
        
        private byte GetX(ushort instruction)
        {
            return GetNibble(instruction, 2);
        }
        
        private byte GetY(ushort instruction)
        {
            return GetNibble(instruction, 3);
        }

        public void ParseInput(string filepath)
        {
            var data = File.ReadLines(filepath);

            foreach(var l in data)
            {
                bool[] states = new bool[16];

                for(int i = 0; i < l.Length; i++)
                    switch(l[i])
                    {
                        case '1':
                            states[0] = true;
                            break;

                        case '2':
                            states[1] = true;
                            break;

                        case '3':
                            states[2] = true;
                            break;

                        case '4':
                            states[3] = true;
                            break;

                        case 'q':
                            states[4] = true;
                            break;

                        case 'w':
                            states[5] = true;
                            break;

                        case 'e':
                            states[6] = true;
                            break;

                        case 'r':
                            states[7] = true;
                            break;

                        case 'a':
                            states[8] = true;
                            break;

                        case 's':
                            states[9] = true;
                            break;

                        case 'd':
                            states[10] = true;
                            break;

                        case 'f':
                            states[11] = true;
                            break;

                        case 'z':
                            states[12] = true;
                            break;

                        case 'x':
                            states[13] = true;
                            break;

                        case 'c':
                            states[14] = true;
                            break;

                        case 'v':
                            states[15] = true;
                            break;

                    }

                _input.Enqueue(states);
            }
        }
        
        private void Display(bool clearScreen)
        {
            Console.SetCursorPosition(consoleLeftOrigin, consoleTopOrigin);
            Console.WriteLine(separator);

            for (int i = 0; i < 32; i++)
            {
                Console.Write("|");
                
                for (int j = 0; j < 64; j++)
                {
                    if (_gfx[i*64+j])
                        Console.Write("██");
                    else
                        Console.Write("  ");
                }

                Console.WriteLine("|");
            }
            
            Console.WriteLine(separator);
        }

        private void DebugInstruction()
        {
            Console.WriteLine("PC = 0x{0:X3}", _PC);
            Console.WriteLine("Instruction = 0x{0:X4}", this[_PC]);
        }

        private void DebugTimer()
        {
            Console.WriteLine($"Delay = {_delayTimer}\nSound = {_soundTimer}");
        }

        private void DebugRegister()
        {
            Console.WriteLine("I = 0x{0:X4}", _I);

            for(int i = 0; i < _registers.Length-1; i++)
                Console.Write("V{0:X} = 0x{1:X4}, ", i, _registers[i]);

            Console.WriteLine("V{0:X} = 0x{1:X4}", 15, _registers[^1]);
        }

        private void DebugStack()
        {
            Console.Write("Stack = ");

            var tmp = _stack.ToArray();

            for(int i = 0; i < tmp.Length-1; i++)
                Console.Write("0x{0:X4} -> ", tmp[i]);

            Console.WriteLine("nil");
        }

        private void DebugInput()
        {
            var sep = "-----------------";

            var tmp = _input.Peek();
            
            var v0 = tmp[0] ? "0" : " ";
            var v1 = tmp[1] ? "1" : " ";
            var v2 = tmp[2] ? "2" : " ";
            var v3 = tmp[3] ? "3" : " ";
            var v4 = tmp[4] ? "C" : " ";
            var v5 = tmp[5] ? "4" : " ";
            var v6 = tmp[6] ? "5" : " ";
            var v7 = tmp[7] ? "6" : " ";
            var v8 = tmp[8] ? "D" : " ";
            var v9 = tmp[9] ? "7" : " ";
            var v10 = tmp[10] ? "8" : " ";
            var v11 = tmp[11] ? "9" : " ";
            var v12 = tmp[12] ? "A" : " ";
            var v13 = tmp[13] ? "0" : " ";
            var v14 = tmp[14] ? "B" : " ";
            var v15 = tmp[15] ? "F" : " ";

            Console.WriteLine(sep +"\n| {1} | {2} | {3} | {12} |\n| {4} | {5} | {6} | {13} |\n| {7} | {8} | {9} | {14} |\n| {10} | {0} | {11} | {15} |\n"+sep, 
                    v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15);
        }

        private string GetSegment(int start)
        {
            if(start >= 4096 || start < 0)
                throw new Exception("Index out of range of _memory");

            string slice = "";

            for(int i = start; i < start + 15; i++)
                slice += _memory[i].ToString("X2") + " ";

            return slice + _memory[start + 15].ToString("X2");
        }

        private void DebugMemory()
        {
            for(int i = 0; i < _memory.Length; i+=16)
                Console.WriteLine("{0:X3}   {1}", i, GetSegment(i));
        }

        public void PrintDebug(Debug part)
        {
            switch (part) {
                case Debug.Instruction:
                    DebugInstruction();
                    break;

                case Debug.Timer:
                    DebugTimer();
                    break;

                case Debug.Register:
                    DebugRegister();
                    break;

                case Debug.Stack:
                    DebugStack();
                    break;

                case Debug.Input:
                    DebugInput();
                    break;

                case Debug.Display:
                    Display(false);
                    break;

                case Debug.Memory:
                    DebugMemory();
                    break;

                case Debug.All:
                    DebugInstruction();
                    DebugTimer();
                    DebugRegister();
                    DebugStack();
                    DebugInput();
                    Display(false);
                    DebugMemory();
                    break;
            };
        }

        private void InitRom()
        {
            var font_seg = new byte[]
            {
                0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                0x20, 0x60, 0x20, 0x20, 0x70, // 1
                0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };

            for (int i = 0; i < font_seg.Length; i++)
                _memory[i] = font_seg[i];
        }

        private void AddHandlers()
        {
            _handlers.Add(0x00, Handle0);
            _handlers.Add(0x01, Handle1);
            _handlers.Add(0x02, Handle2);
            _handlers.Add(0x03, Handle3);
            _handlers.Add(0x04, Handle4);
            _handlers.Add(0x05, Handle5);
            _handlers.Add(0x06, Handle6);
            _handlers.Add(0x07, Handle7);
            _handlers.Add(0x08, Handle8);
            _handlers.Add(0x09, Handle9);
            _handlers.Add(0x0A, HandleA);
            _handlers.Add(0x0B, HandleB);
            _handlers.Add(0x0C, HandleC);
            _handlers.Add(0x0D, HandleD);
            _handlers.Add(0x0E, HandleE);
            _handlers.Add(0x0F, HandleF);
        }

        public void LoadRom(string filePath)
        {
            var data = File.ReadAllBytes(filePath);

            for(int i = 0; i < data.Length; i++)
                _memory[512 + i] = data[i];
        }

        public bool ExecuteOp()
        {
            var inst = this[_PC];

            var nib1 = GetNibble(inst, 1);

            if(GetNNN(inst) == _PC)
                return false;

            if(!_handlers.ContainsKey(nib1))
                throw new ArgumentException("Operation doesn't exist");

            _handlers[nib1].Invoke(inst);
            return true;
        }

        public void RunProgram()
        {
            var state = true;

            int tdelay = _delayTimer == 0 ? -1 : 4;
            int tsound = _soundTimer == 0 ? -1 : 4;

            do
            {
                //DebugInstruction();

                if(tdelay > 0)
                    tdelay--;

                if(tdelay == 0)
                {
                    _delayTimer--;
                    tdelay = 4;
                }

                if(tsound > 0)
                    tsound--;

                if(tsound == 0)
                {
                    _soundTimer--;
                    tsound = 4;
                }

                state = ExecuteOp();
                PrintDebug(Debug.Display);
                
                if(_input.Count > 0)
                    _input.Dequeue();
            } 
            while(state);
        }
        
        private void Handle0(ushort opcode)
        {
            if(opcode == 0x00E0)
            {
                for(int i = 0; i < _gfx.Length; i++)
                    _gfx[i] = false;

                _registers[^1] = 0;
                _PC += 2;
            }
            else if(opcode == 0x00EE)
            {
                if(_stack.Count == 0)
                    throw new IndexOutOfRangeException();
                else
                    _PC = _stack.Pop();
            }
            else
            {
                _PC = GetNNN(opcode);
            }
        }
        
        private void Handle1(ushort opcode)
        {
            _PC = GetNNN(opcode);
        }

        private void Handle2(ushort opcode)
        {
            _stack.Push((ushort)(_PC + 2));
            _PC = GetNNN(opcode);
        }

        private void Handle3(ushort opcode)
        {
            var NN = GetNN(opcode);
            var X = GetNibble(opcode, 2);

            _PC += (ushort)(_registers[X] == NN ? 4 : 2);
        }

        private void Handle4(ushort opcode)
        {
            var NN = GetNN(opcode);
            var X = GetNibble(opcode, 2);

            _PC += (ushort)(_registers[X] != NN ? 4 : 2);
        }

        private void Handle5(ushort opcode)
        {
            var X = GetX(opcode);
            var Y = GetY(opcode);

            _PC += (ushort)(_registers[X] == _registers[Y] ? 4 : 2);
        }

        private void Handle6(ushort opcode)
        {
            var NN = GetNN(opcode);
            var X = GetNibble(opcode, 2);

            _registers[X] = NN;
            _PC += 2;
        }

        private void Handle7(ushort opcode)
        {
            var NN = GetNN(opcode);
            var X = GetNibble(opcode, 2);
            
            _registers[X] += NN;
            _PC += 2;
        }

        private void Handle8(ushort opcode)
        {
            var X = GetX(opcode);
            var Y = GetY(opcode);
    
            var x = _registers[X];

            switch(GetN(opcode)){
                case 0x00:
                    _registers[X] = _registers[Y];
                    break;

                case 0x01:
                    _registers[X] |= _registers[Y];
                    break;

                case 0x02:
                    _registers[X] &= _registers[Y];
                    break;

                case 0x03:
                    _registers[X] ^= _registers[Y];
                    break;

                case 0x04:
                    var vX = (ushort)_registers[X];
                    var vY = (ushort)_registers[Y];

                    _registers[^1] = (byte)(vX + vY > 255 ? 1 : 0);
                    _registers[X] += _registers[Y];

                    break;

                case 0x05:
                    _registers[^1] = (byte)(_registers[X] > _registers[Y] ? 1 : 0);
                    _registers[X] -= _registers[Y];
                    
                    break;

                case 0x06:
                    _registers[X] = (byte)(_registers[X] >> 1);
                    _registers[^1] = (byte)(_registers[X] & 0x01);
                    break;

                case 0x07:
                    _registers[^1] = (byte)(_registers[Y] > _registers[X] ? 1 : 0);
                    _registers[X] = (byte)(_registers[Y] - _registers[X]);
                    
                    break;

                case 0x0E:
                    _registers[X] = (byte)(_registers[X] << 1);
                    _registers[^1] = 
                        (byte)((_registers[X] & 0x80) == 0x10 ? 1 : 0);
                    break;

            }

            _PC += 2;
        }
        
        private void Handle9(ushort opcode)
        {
            var X = GetX(opcode);
            var Y = GetY(opcode);

            _PC += (ushort)(_registers[X] != _registers[Y] ? 4 : 2);
        }

        private void HandleA(ushort opcode)
        {
            _I = GetNNN(opcode);
            _PC += 2;
        }

        private void HandleB(ushort opcode)
        {
            _PC = (ushort)(_registers[0] + GetNNN(opcode));
            _PC += 2;
        }

        private void HandleC(ushort opcode)
        {
            var X = GetX(opcode);
            var NN = GetNN(opcode);

            _registers[X] = (byte)((_random.Next(256) & 0xFF) & NN);
            _PC += 2;
        }

        private void HandleD(ushort opcode)
        {
            var end = (ushort)(GetN(opcode));

            for(ushort i = _I; i < end; i++)
            {
                var sp = Convert.ToString(this[i], 2);

                Console.WriteLine($"D: {sp}");

                for(int j = 0; j < sp.Length; j++)
                {
                    bool hasChanged = (!_gfx[j] && sp[j] == '1') || (_gfx[j] && sp[j] == '0');

                    _gfx[j] = sp[j] == '1';

                    _registers[^1] = hasChanged ? (byte)1 : (byte)0;
                }
            }

            _PC += 2;
        }

        private void HandleE(ushort opcode)
        {
            var X = GetX(opcode);
            if(GetNN(opcode) == 0x9E)
                _PC += (ushort)(_input.Peek()[_registers[X]] ? 4 : 2);
            else
                _PC += (ushort)(!_input.Peek()[_registers[X]] ? 4 : 2);
        }

        private void HandleF(ushort opcode)
        {
            var NN = GetNN(opcode);
            var X = GetX(opcode);

            switch (NN) {
                case 0x07:
                    _registers[X] = _delayTimer;
                    break;

                case 0x0A:
                    var tmp = (byte)(Console.ReadKey().Key);

                    if(tmp == 0 && _input.Count == 0)
                        throw new ArgumentNullException("input queue empty, expected a key");
                    
                    if(tmp == 0)
                    {
                        int i = 0;
                        var last_input = _input.Dequeue();

                        while(i < last_input.Length && !last_input[i])
                            i++;

                        if(i >= last_input.Length)
                            _registers[X] = 0x00;
                        else
                            _registers[X] = (byte)i;
                    }
                    else
                        _registers[X] = tmp;
                    break;

                case 0x15:
                    _delayTimer = _registers[X];
                    break;

                case 0x18:
                    _soundTimer = _registers[X];
                    break;

                case 0x1E:
                    _I += _registers[X];
                    break;

                case 0x29:
                    _I = (ushort)(_registers[X] * 5);
                    break;

                case 0x33:
                    _memory[_I] = (byte)(_registers[X] % 1000 / 100);
                    _memory[_I+1] = (byte)(_registers[X] % 100 / 100);
                    _memory[_I+2] = (byte)(_registers[X] % 10);
                    break;

                case 0x55:
                    for(byte i = 0; i <= X; i++)
                        _memory[_I + i] = _registers[i];
                    break;

                case 0x65:
                    for(byte i = 0; i <= X; i++)
                        _registers[i] = _memory[_I + i];
                    break;

            }

            _PC += 2;
        }
    }
}
