#define ROM_PATH        "roms/sierpinsky.ch8"
#define MEM_SIZE        4096
#define RESERVED        512
#define MAX_LVL         16
#define FONT_SIZE       16
#define CHR_SIZE        5
#define SCREEN_WIDTH    64
#define SCREEN_HEIGHT   32

#define DEBUG_ON

// Silence logs, warnings and error messages
/* #define NOLOG */
/* #define NOWARN */
/* #define NOERR */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#include "log.h"

/* static byte compat_mode = 0; */

typedef unsigned char byte;
typedef unsigned short int word;

enum ChipState
{
    RUN,
    ERR,
    EXT,
};

struct Chip 
{
    byte reg[16];
    word pc;
    byte sp;
    word I;

    byte sound;
    byte timer;

    int redraw;

    word stack[MAX_LVL];
    byte mem[MEM_SIZE];
    byte gfx[SCREEN_WIDTH * SCREEN_HEIGHT];
    byte key[16];

    enum ChipState state;
};

void memdump(FILE *stream, struct Chip *chip)
{
    for (int i = 0; i < MEM_SIZE; i+=16)
    {
        fprintf(stream, "0x%04X\t", i);

        for (int j = 0; j < 14; j += 2)
        {
            fprintf(stream, "%02hhX", chip->mem[i+j]);
            fprintf(stream, "%02hhX ", chip->mem[i+j+1]);
        }

        fprintf(stream, "%02hhX", chip->mem[i+14]);
        fprintf(stream, "%02hhX\n", chip->mem[i+15]);
    }
}

void loadfont(struct Chip *chip)
{
    byte fontset[FONT_SIZE * CHR_SIZE] =
    {
        0xF0, 0x90, 0x90, 0x90, 0xF0, //0
        0x20, 0x60, 0x20, 0x20, 0x70, //1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, //2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, //3
        0x90, 0x90, 0xF0, 0x10, 0x10, //4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, //5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, //6
        0xF0, 0x10, 0x20, 0x40, 0x40, //7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, //8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, //9
        0xF0, 0x90, 0xF0, 0x90, 0x90, //A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, //B
        0xF0, 0x80, 0x80, 0x80, 0xF0, //C
        0xE0, 0x90, 0x90, 0x90, 0xE0, //D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, //E
        0xF0, 0x80, 0xF0, 0x80, 0x80  //F
    };

    memcpy(
        chip->mem,
        fontset,
        FONT_SIZE * CHR_SIZE
    );
}

void romfile2mem(struct Chip *chip, const char *rom_path)
{
    FILE *file = fopen(rom_path, "rb");

    if (file == NULL)
    {
        WARN("I/O Error");
        return;
    }

    fseek(file, 0, SEEK_END);
    size_t fsize = ftell(file);

    if (fsize >= (MEM_SIZE - RESERVED))
    {
        WARNX("Invalid ROM size");
        return;
    }

    rewind(file);

    if (fread(chip->mem + RESERVED, 1, fsize, file) != fsize)
    {
        WARNX("I/O Error");
    }

    LOGX("read file of size %zu", fsize);
}

void cyclechip(struct Chip *chip)
{
    byte inst = chip->mem[chip->pc];
    byte x = inst & 0x0F;
    inst &= 0xF0 >> 4;

    byte kk = chip->mem[chip->pc + 1];
    word op = (word)inst << 8 | kk;

    word nnn = op & 0x0FFF;
    byte n = op & 0x000F;
    byte y = op & 0x00F0 >> 4;

    byte vX = chip->reg[(int)x];
    byte vY = chip->reg[(int)y];

    switch(inst)
    {
    case 0x0:
        switch(kk)
        {
        case 0xE0:
            memset(chip->gfx, 0, SCREEN_WIDTH * SCREEN_HEIGHT);
            chip->pc += 2;
            return;

        case 0xEE:
            chip->pc = chip->stack[chip->sp--];
            return;

        case 0xFD:
            chip->state = EXT;
            return;

        default:
            WARNX("Unknown instruction '0x%04X' @ '0x%04X'\n", op, chip->pc);
            chip->state = ERR;
            return;
        }

    case 0x1:
        chip->pc = nnn;
        return;

    case 0x2:
        if (chip->sp == MAX_LVL - 1)
        {
            WARNX("Reached maximum subroutines (%d)\n", MAX_LVL);
            chip->state = ERR;
            return;
        }

        chip->stack[chip->sp++] = chip->pc;
        chip->pc = nnn;
        return;

    case 0x3:
        if (vX == kk)
        {
            chip->pc += 2;
        }

        chip->pc += 2;
        return;

    case 0x4:
        if (vX != kk)
        {
            chip->pc += 2;
        }

        chip->pc += 2;
        return;

    case 0x5:
        if (vX == vY)
        {
            chip->pc += 2;
        }

        chip->pc += 2;
        return;

    case 0x6:
        chip->reg[(int)x] = kk;
        chip->pc += 2;
        return;

    case 0x7:
        chip->reg[(int)x] += kk;
        chip->pc += 2;
        return;

    case 0x8:
        switch(n)
        {
        case 0x0:
            chip->reg[(int)x] = vY;
            /* chip->reg[(int)y] = vX; */
            break;

        case 0x1:
            chip->reg[(int)x] |= vY;
            break;

        case 0x2:
            chip->reg[(int)x] &= vY;
            break;

        case 0x3:
            chip->reg[(int)x] ^= vY;
            break;

        case 0x4:
            chip->reg[0xF] = (vY > (0xFF - vX)) ? 0 : 1;
            chip->reg[(int)x] += vY;
            break;

        case 0x5:
            chip->reg[0xF] = (vX > vY) ? 1 : 0;
            chip->reg[(int)x] -= vY;
            break;

        case 0x6:
            chip->reg[0xF] = (vX & 1);
            chip->reg[(int)x] >>= 1;
            break;

        case 0x7:
            chip->reg[0xF] = (vX > vY) ? 1 : 0;
            chip->reg[(int)x] = vY - vX;
            break;

        case 0xE:
            chip->reg[0xF] = (vX & 1);
            chip->reg[(int)x] *= 2;
            break;
        }

        chip->pc += 2;
        return;

    case 0x9:
        if (vX != vY)
        {
            chip->pc += 2;
        }

        chip->pc += 2;
        return;

    case 0xA:
        chip->I = nnn;
        chip->pc += 2;
        return;

    case 0xB:
        chip->pc = nnn + chip->reg[0];
        return;

    case 0xC:
        chip->reg[(int)x] = (byte)(rand() % 0xFF) & kk;
        chip->pc += 2;
        return;

    case 0xD:
        {
            chip->reg[0xF] = 0;

            for (int yline = 0; yline < n; yline++)
            {
                word pixel = chip->mem[chip->I + yline];

                for (int xline = 0; xline < 8; xline++)
                {
                    if ((pixel & (0x80 >> xline)) != 0)
                    {
                        size_t idx = x + xline + ((y + yline) * SCREEN_WIDTH);

                        if (chip->gfx[idx] == 1)
                        {
                            chip->reg[0xF] = 1;
                        }

                        chip->gfx[idx] ^= 1;
                    }
                }
            }
        }

        chip->redraw = 1;
        chip->pc += 2;
        return;

    default:
        WARNX("Unknown instruction2 '0x%04X' @ '0x%04X'\n", op, chip->pc);
        chip->pc += 2;
        return;
    }
}

int main(void)
{
    srand(time(NULL));
    struct Chip chip = {0};

    chip.pc = RESERVED;
    chip.sp = 0;

    loadfont(&chip);

    romfile2mem(&chip, ROM_PATH);
    LOGX("Loaded '%s' to Chip's RAM", ROM_PATH);

    while (chip.state != ERR && chip.pc < MEM_SIZE)
    {
        cyclechip(&chip);
    }

    /* memdump(stdout, &chip); */

    return 0;
}
