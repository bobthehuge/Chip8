using System;
using System.IO;
using Emudev;

namespace Emulator
{
    public class Cheepl //Deepl for Chip8
    {
        public static string[] Translate(byte[] data_tmp)
        {
            ushort[] data = new ushort[data_tmp.Length/2];

            for(int i = 0; i < data_tmp.Length-1; i+=2)
                data[i/2] = (ushort)((data_tmp[i] << 8) + data_tmp[i+1]);

            var trans = new string[data.Length];

            for(int i = 0 ; i < data.Length; i++)
            {
                var v = data[i];

                var NNN = Helpers.GetNNN(v);
                var NN = Helpers.GetNN(v);
                var N = Helpers.GetN(v);

                var X = Helpers.GetX(v);
                var Y = Helpers.GetY(v);

                switch(Helpers.GetNibble(v, 1))
                {
                    case 0x00:
                        switch(NNN)
                        {
                            case 0x0E0:
                                trans[i] = $"0x{v:X4}: CLS";
                                break;
                            
                            case 0x0EE:
                                trans[i] = $"0x{v:X4}: RET";
                                break;

                            default:
                                trans[i] = $"0x{v:X4}: SYS 0x{NNN:X4}";
                                break;
                        }
                        break;

                    case 0x01:
                        trans[i] = $"0x{v:X4}: JMP 0x{NNN:X3}";
                        break;

                    case 0x02:
                        trans[i] = $"0x{v:X4}: CALL 0x{NNN:X3}";
                        break;

                    case 0x03:
                        trans[i] = $"0x{v:X4}: SKIP_EQ V{X:X}, 0x{NN:X2}";
                        break;

                    case 0x04:
                        trans[i] = $"0x{v:X4}: SKIP_NEQ V{X:X}, 0x{NN:X2}";
                        break;

                    case 0x05:
                        trans[i] = $"0x{v:X4}: REG_SKIPE_Q V{X:X}, V{Y:X}";
                        break;

                    case 0x06:
                        trans[i] = $"0x{v:X4}: SET V{X:X}, 0x{NN:X2}";
                        break;

                    case 0x07:
                        trans[i] = $"0x{v:X4}: ADD V{X:X}, 0x{NN:X2}";
                        break;

                    case 0x08:
                        switch(N)
                        {
                            case 0x00:
                                trans[i] = $"0x{v:X4}: SWAP V{X:X}, V{Y:X}";
                                break;

                            case 0x01:
                                trans[i] = $"0x{v:X4}: OR V{X:X}, V{Y:X}";
                                break;

                            case 0x02:
                                trans[i] = $"0x{v:X4}: AND V{X:X}, V{Y:X}";
                                break;

                            case 0x03:
                                trans[i] = $"0x{v:X4}: XOR V{X:X}, V{Y:X}";
                                break;

                            case 0x04:
                                trans[i] = $"0x{v:X4}: ADD V{X:X}, V{Y:X}";
                                break;

                            case 0x05:
                                trans[i] = $"0x{v:X4}: SUB V{X:X}, V{Y:X}";
                                break;

                            case 0x06:
                                trans[i] = $"0x{v:X4}: SHR V{X:X}, V{Y:X}";
                                break;

                            case 0x07:
                                trans[i] = $"0x{v:X4}: SUBL V{X:X}, V{Y:X}";
                                break;

                            case 0x0E:
                                trans[i] = $"0x{v:X4}: SHL V{X:X}, V{Y:X}";
                                break;
                        }
                        break;

                    case 0x09:
                        trans[i] = $"0x{v:X4}: REG_SKIP_NEQ V{X:X}, V{Y:X}";
                        break;

                    case 0x0A:
                        trans[i] = $"0x{v:X4}: I_SET 0x{NNN:X3}";
                        break;

                    case 0x0B:
                        trans[i] = $"0x{v:X4}: 0_JMP 0x{NNN:X3}";
                        break;

                    case 0x0C:
                        trans[i] = $"0x{v:X4}: RND V{X:X}, 0x{NN:X2}";
                        break;

                    case 0x0D:
                        trans[i] = $"0x{v:X4}: DRW V{X:X}, V{Y:X}, 0x{N:X}";
                        break;

                    case 0x0E:
                        if(NN == 0x9E)
                            trans[i] = $"0x{v:X4}: KEY_SKIP V{X:X}";
                        else
                            trans[i] = $"0x{v:X4}: KEY_SKIP_N V{X:X}";
                        break;

                    case 0x0F:
                        switch(NN)
                        {
                            case 0x07:
                                trans[i] = $"0x{v:X4}: SET_V V{X:X}";
                                break;

                            case 0x0A:
                                trans[i] = $"0x{v:X4}: READ_KEY V{X:X}";
                                break;

                            case 0x15:
                                trans[i] = $"0x{v:X4}: SET_DEL V{X:X}";
                                break;

                            case 0x18:
                                trans[i] = $"0x{v:X4}: SET_SND V{X:X}";
                                break;

                            case 0x1E:
                                trans[i] = $"0x{v:X4}: ADD_I V{X:X}";
                                break;

                            case 0x29:
                                trans[i] = $"0x{v:X4}: SET_I_SPRITE_DIGIT V{X:X}";
                                break;

                            case 0x33:
                                trans[i] = $"0x{v:X4}: SAVE_BCD V{X:X}";
                                break;

                            case 0x55:
                                trans[i] = $"0x{v:X4}: SAVE_REG V{X:X}";
                                break;

                            case 0x65:
                                trans[i] = $"0x{v:X4}: LOAD_REG V{X:X}";
                                break;
                        }
                        break;
                }
            }

            return trans;
        }
    }
}
